using Barbearia.Domain.Entities;

namespace Barbearia.Infrastructure.Dao.Interface;

public interface IUsuarioDao
{
    Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken);

    Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        int id,
        string nome,
        string email,
        string telefone,
        string? instagram,
        DateOnly dataNascimento,
        int idTipoUsuario,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
