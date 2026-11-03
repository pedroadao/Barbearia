using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Dao;

public sealed class StatusDao(AppDbContext context) : IStatusDao
{
    public Task<Status?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Status.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Status>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Status.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Status> AddAsync(Status status, CancellationToken cancellationToken)
    {
        await context.Status.AddAsync(status, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return status;
    }

    public async Task<bool> UpdateAsync(int id, string descricao, CancellationToken cancellationToken)
    {
        var linhasAfetadas = await context.Status
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(s => s.Descricao, descricao),
                cancellationToken);

        return linhasAfetadas > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var linhasAfetadas = await context.Status
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return linhasAfetadas > 0;
    }
}
