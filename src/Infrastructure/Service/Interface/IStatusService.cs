using Barbearia.Application.DTO;
using Barbearia.Domain.Common;

namespace Barbearia.Infrastructure.Service.Interface;

public interface IStatusService
{
    Task<Result<StatusDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<StatusDto>>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<Result<StatusDto>> CriarAsync(CreateStatusDto dto, CancellationToken cancellationToken);

    Task<Result<StatusDto>> AtualizarAsync(int id, UpdateStatusDto dto, CancellationToken cancellationToken);

    Task<Result> RemoverAsync(int id, CancellationToken cancellationToken);
}
