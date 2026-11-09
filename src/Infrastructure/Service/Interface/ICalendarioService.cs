using Barbearia.Application.DTO;
using Barbearia.Domain.Common;

namespace Barbearia.Infrastructure.Service.Interface;

public interface ICalendarioService
{
    Task<Result<CalendarioDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<CalendarioDto>>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<Result<CalendarioDto>> CriarAsync(CreateCalendarioDto dto, CancellationToken cancellationToken);

    Task<Result<CalendarioDto>> AtualizarAsync(int id, UpdateCalendarioDto dto, CancellationToken cancellationToken);

    Task<Result> RemoverAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Adiciona um cliente avulso (sem cadastro) à fila do barbeiro, ocupando o
    /// primeiro horário livre a partir do horário atual (arredondado para a
    /// próxima meia hora), respeitando as mesmas regras de horário comercial e
    /// conflito usadas para clientes cadastrados.
    /// </summary>
    Task<Result<CalendarioDto>> AdicionarClienteAvulsoAsync(
        int idBarbeiro,
        string nomeCliente,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reorganiza a ordem de atendimento da fila do barbeiro na data informada,
    /// redistribuindo os clientes pelos mesmos horários já cadastrados.
    /// </summary>
    Task<Result> ReordenarFilaAsync(
        int idBarbeiro,
        DateOnly data,
        IReadOnlyList<int> idsEmOrdem,
        CancellationToken cancellationToken);
}
