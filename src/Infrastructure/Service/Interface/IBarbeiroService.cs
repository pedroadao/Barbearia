using Barbearia.Application.DTO;
using Barbearia.Domain.Common;

namespace Barbearia.Infrastructure.Service.Interface;

public interface IBarbeiroService
{
    Task<Result<BarbeiroDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<BarbeiroDto>>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<Result<BarbeiroDto>> CriarAsync(CreateBarbeiroDto dto, CancellationToken cancellationToken);

    Task<Result<BarbeiroDto>> AtualizarAsync(int id, UpdateBarbeiroDto dto, CancellationToken cancellationToken);

    Task<Result> RemoverAsync(int id, CancellationToken cancellationToken);
}
