using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Dao;

public sealed class CalendarioDao(AppDbContext context) : ICalendarioDao
{
    public Task<Calendario?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Calendarios.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Calendario>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Calendarios.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Calendario> AddAsync(Calendario calendario, CancellationToken cancellationToken)
    {
        await context.Calendarios.AddAsync(calendario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return calendario;
    }

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
            .Where(c => c.Id == id)
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
        var linhasAfetadas = await context.Calendarios
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return linhasAfetadas > 0;
    }
}
