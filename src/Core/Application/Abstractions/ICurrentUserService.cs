namespace Barbearia.Application.Abstractions;

public interface ICurrentUserService
{
    Task<int?> ObterIdUsuarioAtualAsync(CancellationToken cancellationToken);
}
