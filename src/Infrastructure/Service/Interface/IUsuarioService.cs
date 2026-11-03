using Barbearia.Application.DTO;
using Barbearia.Domain.Common;

namespace Barbearia.Infrastructure.Service.Interface;

public interface IUsuarioService
{
    Task<Result<UsuarioDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<UsuarioDto>>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<Result<UsuarioDto>> CriarAsync(CreateUsuarioDto dto, CancellationToken cancellationToken);

    Task<Result<UsuarioDto>> AtualizarAsync(int id, UpdateUsuarioDto dto, CancellationToken cancellationToken);

    Task<Result> RemoverAsync(int id, CancellationToken cancellationToken);
}
