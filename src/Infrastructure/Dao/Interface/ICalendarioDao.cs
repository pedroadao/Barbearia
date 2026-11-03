using Barbearia.Domain.Entities;

namespace Barbearia.Infrastructure.Dao.Interface;

public interface ICalendarioDao
{
    Task<Calendario?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Calendario>> GetAllAsync(CancellationToken cancellationToken);

    Task<Calendario> AddAsync(Calendario calendario, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        int id,
        int idBarbeiro,
        DateOnly data,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        int? idCliente,
        bool disponivel,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
