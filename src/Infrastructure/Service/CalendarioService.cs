using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class CalendarioService(ICalendarioDao calendarioDao) : ICalendarioService
{
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
        var calendario = new Calendario(
            dto.IdBarbeiro,
            dto.Data,
            dto.HoraInicio,
            dto.HoraFim,
            idCliente: dto.IdCliente,
            disponivel: dto.IdCliente is null);

        var criado = await calendarioDao.AddAsync(calendario, cancellationToken);

        return criado.ToDto();
    }

    public async Task<Result<CalendarioDto>> AtualizarAsync(
        int id,
        UpdateCalendarioDto dto,
        CancellationToken cancellationToken)
    {
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

        return calendario!.ToDto();
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await calendarioDao.DeleteAsync(id, cancellationToken);

        return removido ? Result.Success() : Result.Failure(DomainErrors.Calendario.NotFound(id));
    }
}
