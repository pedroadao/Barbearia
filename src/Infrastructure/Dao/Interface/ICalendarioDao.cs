using Barbearia.Domain.Entities;

namespace Barbearia.Infrastructure.Dao.Interface;

public interface ICalendarioDao
{
    Task<Calendario?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Calendario>> GetAllAsync(CancellationToken cancellationToken);

    Task<Calendario> AddAsync(Calendario calendario, CancellationToken cancellationToken);

    /// <summary>
    /// Verifica se já existe, para o mesmo barbeiro e data, algum horário cujo
    /// intervalo [horaInicio, horaFim) sobreponha o informado. Use
    /// <paramref name="idParaIgnorar"/> ao validar uma atualização, para não
    /// conflitar com o próprio registro sendo alterado.
    /// </summary>
    Task<bool> ExisteConflitoAsync(
        int idBarbeiro,
        DateOnly data,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        int? idParaIgnorar,
        CancellationToken cancellationToken);

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
