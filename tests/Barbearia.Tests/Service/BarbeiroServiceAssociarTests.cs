using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Fakes;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;

namespace Barbearia.Tests.Service;

public sealed class BarbeiroServiceAssociarTests
{
    private static CreateBarbeiroDto CriarDtoValido() => new(
        Nome: "João Silva",
        Email: "joao@barbearia.com",
        Telefone: "11999999999",
        Instagram: "@joaosilva",
        DataNascimento: new DateOnly(1990, 5, 20),
        IdTipoUsuario: InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);

    private static BarbeiroService CriarService(Barbearia.Infrastructure.AppDbContext context) =>
        new(new BarbeiroDao(context), new UsuarioDao(context), new FakeCurrentUserService(), new FakeLogAlteracaoPublisher());

    [Fact]
    public async Task AssociarAsync_QuandoBarbeiroPreCadastradoSemLogin_DeveAssociarAuthUserId()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var service = CriarService(context);
        var criado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        var resultado = await service.AssociarAsync("auth-user-joao", "joao@barbearia.com", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Id.Should().Be(criado.Value.Id);

        var porAuthUser = await service.ObterPorAuthUserIdAsync("auth-user-joao", CancellationToken.None);
        porAuthUser.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AssociarAsync_QuandoNaoExisteBarbeiroComOEmail_DeveRetornarNotFound()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var service = CriarService(context);

        var resultado = await service.AssociarAsync("auth-user-x", "inexistente@barbearia.com", CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Code.Should().Be("Barbeiro.NotFoundPorEmail");
    }

    [Fact]
    public async Task AssociarAsync_QuandoBarbeiroJaAssociadoAOutroAuthUser_DeveRetornarConflito()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var service = CriarService(context);
        await service.CriarAsync(CriarDtoValido(), CancellationToken.None);
        await service.AssociarAsync("auth-user-joao", "joao@barbearia.com", CancellationToken.None);

        var resultado = await service.AssociarAsync("auth-user-outro", "joao@barbearia.com", CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Code.Should().Be("Barbeiro.JaAssociado");
    }

    [Fact]
    public async Task AssociarAsync_QuandoAuthUserJaPossuiBarbeiroAssociado_DeveRetornarConflito()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var service = CriarService(context);
        await service.CriarAsync(CriarDtoValido(), CancellationToken.None);
        await service.AssociarAsync("auth-user-joao", "joao@barbearia.com", CancellationToken.None);

        var outroDto = CriarDtoValido() with { Email = "outro@barbearia.com" };
        await service.CriarAsync(outroDto, CancellationToken.None);

        var resultado = await service.AssociarAsync("auth-user-joao", "outro@barbearia.com", CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Code.Should().Be("Barbeiro.JaAssociado");
    }
}
