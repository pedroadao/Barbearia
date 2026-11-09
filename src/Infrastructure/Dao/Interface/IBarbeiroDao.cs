using Barbearia.Domain.Entities;

namespace Barbearia.Infrastructure.Dao.Interface;

public interface IBarbeiroDao
{
    Task<Barbeiro?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Barbeiro?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken);

    Task<Barbeiro?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<IReadOnlyList<Barbeiro>> GetAllAsync(CancellationToken cancellationToken);

    Task<Barbeiro> AddAsync(Barbeiro barbeiro, CancellationToken cancellationToken);

    Task<Barbeiro?> AssociarAuthUserAsync(int id, string authUserId, CancellationToken cancellationToken);

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
