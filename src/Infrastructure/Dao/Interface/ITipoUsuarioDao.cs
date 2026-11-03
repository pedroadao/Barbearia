using Barbearia.Domain.Entities;

namespace Barbearia.Infrastructure.Dao.Interface;

public interface ITipoUsuarioDao
{
    Task<TipoUsuario?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TipoUsuario>> GetAllAsync(CancellationToken cancellationToken);

    Task<TipoUsuario> AddAsync(TipoUsuario tipoUsuario, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(int id, string descricao, int idStatus, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
