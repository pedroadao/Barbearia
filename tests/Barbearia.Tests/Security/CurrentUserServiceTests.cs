using System.Security.Claims;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Security;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Barbearia.Tests.Security;

public sealed class CurrentUserServiceTests
{
    private const string AuthUserId = "auth-user-1";

    private static IHttpContextAccessor CriarAccessor(string? authUserId)
    {
        var usuario = authUserId is null
            ? new ClaimsPrincipal(new ClaimsIdentity())
            : new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", authUserId)], "Test"));

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = usuario },
        };
    }

    [Fact]
    public async Task ObterIdUsuarioAtualAsync_QuandoClaimCorrespondeAUsuarioExistente_DeveRetornarId()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = new Barbeiro("João", "joao@barbearia.com", "11999999999", null, new DateOnly(1990, 1, 1), InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);
        barbeiro.AssociarAuthUser(AuthUserId);
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync(CancellationToken.None);

        var service = new CurrentUserService(CriarAccessor(AuthUserId), new UsuarioDao(context));

        var resultado = await service.ObterIdUsuarioAtualAsync(CancellationToken.None);

        resultado.Should().Be(barbeiro.Id);
    }

    [Fact]
    public async Task ObterIdUsuarioAtualAsync_QuandoNaoHaClaim_DeveRetornarNull()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var service = new CurrentUserService(CriarAccessor(null), new UsuarioDao(context));

        var resultado = await service.ObterIdUsuarioAtualAsync(CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterIdUsuarioAtualAsync_QuandoClaimNaoCorrespondeANenhumUsuario_DeveRetornarNull()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var service = new CurrentUserService(CriarAccessor("inexistente"), new UsuarioDao(context));

        var resultado = await service.ObterIdUsuarioAtualAsync(CancellationToken.None);

        resultado.Should().BeNull();
    }
}
