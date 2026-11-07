using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Dao;

public sealed class CalendarioDao(AppDbContext context) : ICalendarioDao
{
    public Task<Calendario?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Calendarios.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.IdStatus == StatusIds.Ativo, cancellationToken);

    public async Task<IReadOnlyList<Calendario>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Calendarios.AsNoTracking()
            .Where(c => c.IdStatus == StatusIds.Ativo)
            .ToListAsync(cancellationToken);

    public async Task<Calendario> AddAsync(Calendario calendario, CancellationToken cancellationToken)
    {
        await context.Calendarios.AddAsync(calendario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return calendario;
    }

    public Task<bool> ExisteConflitoAsync(
        int idBarbeiro,
        DateOnly data,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        int? idParaIgnorar,
        CancellationToken cancellationToken) =>
        context.Calendarios.AsNoTracking().AnyAsync(
            c => c.IdBarbeiro == idBarbeiro
                && c.Data == data
                && c.Id != idParaIgnorar
                && c.IdStatus == StatusIds.Ativo
                && c.HoraInicio < horaFim
                && horaInicio < c.HoraFim,
            cancellationToken);

    public async Task<bool> UpdateAsync(
        int id,
        int idBarbeiro,
        DateOnly data,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        int? idCliente,
        bool disponivel,
        CancellationToken cancellationToken)
    {
        var linhasAfetadas = await context.Calendarios
            .Where(c => c.Id == id && c.IdStatus == StatusIds.Ativo)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(c => c.IdBarbeiro, idBarbeiro)
                    .SetProperty(c => c.Data, data)
                    .SetProperty(c => c.HoraInicio, horaInicio)
                    .SetProperty(c => c.HoraFim, horaFim)
                    .SetProperty(c => c.IdCliente, idCliente)
                    .SetProperty(c => c.Disponivel, disponivel),
                cancellationToken);

        return linhasAfetadas > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var calendario = await context.Calendarios
            .FirstOrDefaultAsync(c => c.Id == id && c.IdStatus == StatusIds.Ativo, cancellationToken);

        if (calendario is null)
            return false;

        calendario.MarcarComoExcluido();
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
