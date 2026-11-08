using System.Security.Claims;
using Barbearia.Application.Abstractions;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.AspNetCore.Http;

namespace Barbearia.Infrastructure.Security;

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IUsuarioDao usuarioDao) : ICurrentUserService
{
    public async Task<int?> ObterIdUsuarioAtualAsync(CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var authUserId = user?.FindFirstValue("sub") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (authUserId is null)
            return null;

        var usuario = await usuarioDao.GetByAuthUserIdAsync(authUserId, cancellationToken);

        return usuario?.Id;
    }
}
