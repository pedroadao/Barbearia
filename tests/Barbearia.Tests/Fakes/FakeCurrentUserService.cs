using Barbearia.Application.Abstractions;

namespace Barbearia.Tests.Fakes;

internal sealed class FakeCurrentUserService(int? idUsuario = 1) : ICurrentUserService
{
    public Task<int?> ObterIdUsuarioAtualAsync(CancellationToken cancellationToken) =>
        Task.FromResult(idUsuario);
}
