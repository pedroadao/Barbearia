using Barbearia.Application.DTO;
using Barbearia.Domain.Common;

namespace Barbearia.Infrastructure.Service.Interface;

public interface ITipoUsuarioService
{
    Task<Result<TipoUsuarioDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<TipoUsuarioDto>>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<Result<TipoUsuarioDto>> CriarAsync(CreateTipoUsuarioDto dto, CancellationToken cancellationToken);

    Task<Result<TipoUsuarioDto>> AtualizarAsync(int id, UpdateTipoUsuarioDto dto, CancellationToken cancellationToken);

    Task<Result> RemoverAsync(int id, CancellationToken cancellationToken);
}
