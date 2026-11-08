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
    ICurrentUserService currentUserService,
    ILogAlteracaoPublisher logPublisher) : ICalendarioService
{
    private static readonly TimeOnly HoraAbertura = new(9, 0);
    private static readonly TimeOnly HoraFechamento = new(19, 0);

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
