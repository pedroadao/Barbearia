using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Dao;

public sealed class TipoUsuarioDao(AppDbContext context) : ITipoUsuarioDao
{
    public Task<TipoUsuario?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.TiposUsuario.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TipoUsuario>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.TiposUsuario.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<TipoUsuario> AddAsync(TipoUsuario tipoUsuario, CancellationToken cancellationToken)
    {
        await context.TiposUsuario.AddAsync(tipoUsuario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return tipoUsuario;
    }

    public async Task<bool> UpdateAsync(int id, string descricao, int idStatus, CancellationToken cancellationToken)
    {
        var linhasAfetadas = await context.TiposUsuario
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.Descricao, descricao)
                    .SetProperty(t => t.IdStatus, idStatus),
                cancellationToken);

        return linhasAfetadas > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var linhasAfetadas = await context.TiposUsuario
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return linhasAfetadas > 0;
    }
}
