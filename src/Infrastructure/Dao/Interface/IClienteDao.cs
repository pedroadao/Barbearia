using Barbearia.Domain.Entities;

namespace Barbearia.Infrastructure.Dao.Interface;

public interface IClienteDao
{
    Task<Cliente?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Cliente>> GetAllAsync(CancellationToken cancellationToken);

    Task<Cliente> AddAsync(Cliente cliente, CancellationToken cancellationToken);

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
