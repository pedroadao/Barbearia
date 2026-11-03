using Barbearia.Domain.Entities;

namespace Barbearia.Infrastructure.Dao.Interface;

public interface IStatusDao
{
    Task<Status?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Status>> GetAllAsync(CancellationToken cancellationToken);

    Task<Status> AddAsync(Status status, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(int id, string descricao, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
