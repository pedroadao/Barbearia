using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Dao;

public sealed class UsuarioDao(AppDbContext context) : IUsuarioDao
{
    public Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.IdStatus == StatusIds.Ativo, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Usuarios.AsNoTracking()
            .Where(u => u.IdStatus == StatusIds.Ativo)
            .ToListAsync(cancellationToken);

    public async Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        await context.Usuarios.AddAsync(usuario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return usuario;
    }

    public async Task<bool> UpdateAsync(
        int id,
        string nome,
        string email,
        string telefone,
        string? instagram,
        DateOnly dataNascimento,
        int idTipoUsuario,
        CancellationToken cancellationToken)
    {
        var linhasAfetadas = await context.Usuarios
            .Where(u => u.Id == id && u.IdStatus == StatusIds.Ativo)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(u => u.Nome, nome)
                    .SetProperty(u => u.Email, email)
                    .SetProperty(u => u.Telefone, telefone)
                    .SetProperty(u => u.Instagram, instagram)
                    .SetProperty(u => u.DataNascimento, dataNascimento)
                    .SetProperty(u => u.IdTipoUsuario, idTipoUsuario),
                cancellationToken);

        return linhasAfetadas > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var linhasAfetadas = await context.Usuarios
            .Where(u => u.Id == id && u.IdStatus == StatusIds.Ativo)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(u => u.IdStatus, StatusIds.Excluido),
                cancellationToken);

        return linhasAfetadas > 0;
    }
}
