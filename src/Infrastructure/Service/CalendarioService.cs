using System.Runtime.CompilerServices;
using Barbearia.Application.Abstractions;
using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class CalendarioService(
    ICalendarioDao calendarioDao,
    IUsuarioDao usuarioDao,
    ICurrentUserService currentUserService,
    ILogAlteracaoPublisher logPublisher,
    IWhatsAppNotificationService whatsAppNotificationService,
    Func<DateTime>? agora = null) : ICalendarioService
{
    private static readonly TimeOnly HoraAbertura = new(9, 0);
    private static readonly TimeOnly HoraFechamento = new(19, 0);

    // O container roda em UTC independentemente do fuso do host, mas o horário comercial
    // (09:00-19:00) é referente ao horário de Brasília; por isso convertemos explicitamente
    // em vez de usar DateTime.Now (que seguiria o fuso do SO, não o do negócio).
    private static readonly TimeZoneInfo FusoHorarioBrasil = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    private readonly Func<DateTime> obterAgora =
        agora ?? (() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoHorarioBrasil));

    private static Error? ValidarHorario(TimeOnly horaInicio, TimeOnly horaFim)
    {
        if (horaInicio < HoraAbertura || horaFim > HoraFechamento)
            return DomainErrors.Calendario.ForaDoHorarioComercial();

        if (horaInicio.Minute % 30 != 0 || horaFim.Minute % 30 != 0 || horaFim <= horaInicio)
            return DomainErrors.Calendario.IntervaloInvalido();

        return null;
    }

    public async Task<Result<CalendarioDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var calendario = await calendarioDao.GetByIdAsync(id, cancellationToken);

        return calendario is null
            ? Result.Failure<CalendarioDto>(DomainErrors.Calendario.NotFound(id))
            : calendario.ToDto();
    }

    public async Task<Result<IReadOnlyList<CalendarioDto>>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var calendarios = await calendarioDao.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<CalendarioDto>>(calendarios.Select(c => c.ToDto()).ToList());
    }

    public async Task<Result<CalendarioDto>> CriarAsync(CreateCalendarioDto dto, CancellationToken cancellationToken)
    {
        var erroHorario = ValidarHorario(dto.HoraInicio, dto.HoraFim);
        if (erroHorario is not null)
            return Result.Failure<CalendarioDto>(erroHorario);

        var existeConflito = await calendarioDao.ExisteConflitoAsync(
            dto.IdBarbeiro,
            dto.Data,
            dto.HoraInicio,
            dto.HoraFim,
            idParaIgnorar: null,
            cancellationToken);

        if (existeConflito)
            return Result.Failure<CalendarioDto>(DomainErrors.Calendario.Conflito());

        var calendario = new Calendario(
            dto.IdBarbeiro,
            dto.Data,
            dto.HoraInicio,
            dto.HoraFim,
            idCliente: dto.IdCliente,
            disponivel: dto.IdCliente is null);

        var criado = await calendarioDao.AddAsync(calendario, cancellationToken);
        var criadoDto = criado.ToDto();

        await PublicarLogAsync(TipoOperacaoLog.Criado, criadoDto, cancellationToken);
        await NotificarBarbeiroAsync(criado, cancellationToken);

        return criadoDto;
    }

    public async Task<Result<CalendarioDto>> AtualizarAsync(
        int id,
        UpdateCalendarioDto dto,
        CancellationToken cancellationToken)
    {
        var erroHorario = ValidarHorario(dto.HoraInicio, dto.HoraFim);
        if (erroHorario is not null)
            return Result.Failure<CalendarioDto>(erroHorario);

        var existeConflito = await calendarioDao.ExisteConflitoAsync(
            dto.IdBarbeiro,
            dto.Data,
            dto.HoraInicio,
            dto.HoraFim,
            idParaIgnorar: id,
            cancellationToken);

        if (existeConflito)
            return Result.Failure<CalendarioDto>(DomainErrors.Calendario.Conflito());

        var atualizado = await calendarioDao.UpdateAsync(
            id,
            dto.IdBarbeiro,
            dto.Data,
            dto.HoraInicio,
            dto.HoraFim,
            dto.IdCliente,
            dto.Disponivel,
            cancellationToken);

        if (!atualizado)
            return Result.Failure<CalendarioDto>(DomainErrors.Calendario.NotFound(id));

        var calendario = await calendarioDao.GetByIdAsync(id, cancellationToken);
        var atualizadoDto = calendario!.ToDto();

        await PublicarLogAsync(TipoOperacaoLog.Atualizado, atualizadoDto, cancellationToken);

        return atualizadoDto;
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await calendarioDao.DeleteAsync(id, cancellationToken);

        if (!removido)
            return Result.Failure(DomainErrors.Calendario.NotFound(id));

        await PublicarLogAsync(TipoOperacaoLog.Excluido, new { Id = id }, cancellationToken);

        return Result.Success();
    }

    private static TimeOnly ArredondarParaProximoIntervalo(TimeOnly hora)
    {
        var minutosTotais = (hora.Hour * 60) + hora.Minute;
        var resto = minutosTotais % 30;

        if (resto == 0 && hora.Second == 0 && hora.Millisecond == 0)
            return new TimeOnly(hora.Hour, hora.Minute);

        var minutosArredondados = minutosTotais - resto + 30;

        return new TimeOnly(0, 0).Add(TimeSpan.FromMinutes(minutosArredondados));
    }

    public async Task<Result<CalendarioDto>> AdicionarClienteAvulsoAsync(
        int idBarbeiro,
        string nomeCliente,
        CancellationToken cancellationToken)
    {
        var barbeiro = await usuarioDao.GetByIdAsync(idBarbeiro, cancellationToken);
        if (barbeiro is null)
            return Result.Failure<CalendarioDto>(DomainErrors.Barbeiro.NotFound(idBarbeiro));

        var agoraAtual = obterAgora();
        var hoje = DateOnly.FromDateTime(agoraAtual);
        var horaInicio = ArredondarParaProximoIntervalo(TimeOnly.FromDateTime(agoraAtual));
        if (horaInicio < HoraAbertura)
            horaInicio = HoraAbertura;

        while (true)
        {
            var horaFim = horaInicio.AddMinutes(30);

            var erroHorario = ValidarHorario(horaInicio, horaFim);
            if (erroHorario is not null)
                return Result.Failure<CalendarioDto>(erroHorario);

            var existente = await calendarioDao.ObterPorHorarioAsync(idBarbeiro, hoje, horaInicio, cancellationToken);

            if (existente is null)
            {
                var novoHorario = new Calendario(
                    idBarbeiro, hoje, horaInicio, horaFim, idCliente: null, disponivel: false, nomeClienteAvulso: nomeCliente);

                var criado = await calendarioDao.AddAsync(novoHorario, cancellationToken);
                var criadoDto = criado.ToDto();

                await PublicarLogAsync(TipoOperacaoLog.Criado, criadoDto, cancellationToken);

                return criadoDto;
            }

            if (existente.Disponivel)
            {
                var reservado = await calendarioDao.ReservarClienteAvulsoAsync(existente.Id, nomeCliente, cancellationToken);
                if (!reservado)
                    return Result.Failure<CalendarioDto>(DomainErrors.Calendario.NotFound(existente.Id));

                var reservadoCompleto = (await calendarioDao.GetByIdAsync(existente.Id, cancellationToken))!;
                var reservadoDto = reservadoCompleto.ToDto();

                await PublicarLogAsync(TipoOperacaoLog.Criado, reservadoDto, cancellationToken);

                return reservadoDto;
            }

            horaInicio = horaInicio.AddMinutes(30);
        }
    }

    public async Task<Result> ReordenarFilaAsync(
        int idBarbeiro,
        DateOnly data,
        IReadOnlyList<int> idsEmOrdem,
        CancellationToken cancellationToken)
    {
        var sucesso = await calendarioDao.ReordenarFilaAsync(idBarbeiro, data, idsEmOrdem, cancellationToken);
        if (!sucesso)
            return Result.Failure(DomainErrors.Calendario.FilaInvalida());

        await PublicarLogAsync(
            TipoOperacaoLog.Atualizado,
            new { IdBarbeiro = idBarbeiro, Data = data, IdsEmOrdem = idsEmOrdem },
            cancellationToken);

        return Result.Success();
    }

    private async Task NotificarBarbeiroAsync(Calendario calendario, CancellationToken cancellationToken)
    {
        if (calendario.IdCliente is null)
            return;

        var barbeiro = await usuarioDao.GetByIdAsync(calendario.IdBarbeiro, cancellationToken);
        if (barbeiro is null || string.IsNullOrWhiteSpace(barbeiro.Telefone))
            return;

        var cliente = await usuarioDao.GetByIdAsync(calendario.IdCliente.Value, cancellationToken);
        var nomeCliente = cliente?.Nome ?? "Cliente";

        var mensagem = $"Novo agendamento: {nomeCliente} em {calendario.Data:dd/MM/yyyy} às {calendario.HoraInicio:HH:mm}.";

        await whatsAppNotificationService.EnviarAsync(barbeiro.Telefone, mensagem, cancellationToken);
    }

    private async Task PublicarLogAsync(
        TipoOperacaoLog operacao,
        object dados,
        CancellationToken cancellationToken,
        [CallerMemberName] string metodo = "")
    {
        var idUsuarioAtual = await currentUserService.ObterIdUsuarioAtualAsync(cancellationToken);
        if (idUsuarioAtual is null)
            return;

        await logPublisher.PublicarAsync(
            new LogAlteracaoMensagem(operacao, "Calendarios", metodo, dados, idUsuarioAtual.Value, DateTime.UtcNow),
            cancellationToken);
    }
}
