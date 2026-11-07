using Barbearia.Application.DTO;
using Barbearia.Domain.Common;

namespace Barbearia.Infrastructure.Service.Interface;

public interface IClienteService
{
    Task<Result<ClienteDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<ClienteDto>> ObterPorAuthUserIdAsync(string authUserId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ClienteDto>>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<Result<ClienteDto>> CriarAsync(CreateClienteDto dto, string authUserId, CancellationToken cancellationToken);

    Task<Result<ClienteDto>> AtualizarAsync(
        int id,
        UpdateClienteDto dto,
        string authUserId,
        bool ehAdministrador,
        CancellationToken cancellationToken);

    Task<Result> RemoverAsync(int id, CancellationToken cancellationToken);
}
