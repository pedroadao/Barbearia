using Barbearia.Domain.Entities;

namespace Barbearia.Infrastructure.Dao.Interface;

public interface ICalendarioDao
{
    Task<Calendario?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Calendario>> GetAllAsync(CancellationToken cancellationToken);

    Task<Calendario> AddAsync(Calendario calendario, CancellationToken cancellationToken);

    /// <summary>
    /// Retorna o horário cadastrado (disponível ou não) do barbeiro, na data e
    /// hora de início informadas, ou null se não houver nenhum.
    /// </summary>
    Task<Calendario?> ObterPorHorarioAsync(int idBarbeiro, DateOnly data, TimeOnly horaInicio, CancellationToken cancellationToken);

    Task<bool> ReservarClienteAvulsoAsync(int id, string nomeClienteAvulso, CancellationToken cancellationToken);

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

    /// <summary>
    /// Retorna os horários ocupados (não disponíveis) do barbeiro na data
    /// informada, ordenados por hora de início — representa a fila do dia.
    /// </summary>
    Task<IReadOnlyList<Calendario>> ObterFilaAsync(int idBarbeiro, DateOnly data, CancellationToken cancellationToken);

    /// <summary>
    /// Reorganiza a fila do barbeiro na data informada, redistribuindo os
    /// clientes (cadastrados ou avulsos) das entradas existentes pelos mesmos
    /// horários, na ordem de <paramref name="idsEmOrdem"/>. Retorna false se a
    /// lista não corresponder exatamente às entradas atuais da fila.
    /// </summary>
    Task<bool> ReordenarFilaAsync(
        int idBarbeiro,
        DateOnly data,
        IReadOnlyList<int> idsEmOrdem,
        CancellationToken cancellationToken);
}
