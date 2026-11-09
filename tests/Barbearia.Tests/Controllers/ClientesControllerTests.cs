using System.Security.Claims;
using Barbearia.Api.Controllers;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Fakes;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Tests.Controllers;

public sealed class ClientesControllerTests
{
    private const string AuthUserId = "auth-user-1";

    private static ClientesController CriarController(
        string? authUserId = AuthUserId,
        Barbearia.Infrastructure.AppDbContext? context = null)
    {
        context ??= InMemoryAppDbContextFactory.Criar();
        var dao = new ClienteDao(context);
        var usuarioDao = new UsuarioDao(context);
        var service = new ClienteService(dao, usuarioDao, new FakeCurrentUserService(), new FakeLogAlteracaoPublisher());
        var barbeiroService = new BarbeiroService(
            new BarbeiroDao(context), usuarioDao, new FakeCurrentUserService(), new FakeLogAlteracaoPublisher());

        var controller = new ClientesController(service, barbeiroService);

        var usuario = authUserId is null
            ? new ClaimsPrincipal(new ClaimsIdentity())
            : new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", authUserId)], "Test"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = usuario },
        };

        return controller;
    }

    private static CreateClienteDto CriarDtoValido() => new(
        Nome: "Maria Souza",
        Email: "maria@email.com",
        Telefone: "11988888888",
        Instagram: null,
        DataNascimento: new DateOnly(1995, 3, 10),
        IdTipoUsuario: InMemoryAppDbContextFactory.IdTipoUsuarioCliente);

    private static UpdateClienteDto CriarDtoAtualizadoValido() => new(
        "Maria Oliveira", "maria.oliveira@email.com", "11911112222", "@mariaoliveira",
        new DateOnly(1995, 3, 10), InMemoryAppDbContextFactory.IdTipoUsuarioCliente);

    [Fact]
    public async Task Create_DeveRetornarCreatedComIdGeradoPeloBanco()
    {
        var controller = CriarController();

        var resultado = await controller.Create(CriarDtoValido(), CancellationToken.None);

        var created = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        var dto = created.Value.Should().BeOfType<ClienteDto>().Subject;
        dto.Id.Should().BeGreaterThan(0);
        dto.Email.Should().Be("maria@email.com");
    }

    [Fact]
    public async Task Create_SemUsuarioAutenticado_DeveRetornarUnauthorized()
    {
        var controller = CriarController(authUserId: null);

        var resultado = await controller.Create(CriarDtoValido(), CancellationToken.None);

        resultado.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Create_QuandoAuthUserJaPossuiCadastro_DeveRetornarConflict()
    {
        var controller = CriarController();
        await controller.Create(CriarDtoValido(), CancellationToken.None);

        var resultado = await controller.Create(CriarDtoValido(), CancellationToken.None);

        resultado.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_QuandoEmailJaPertenceABarbeiroPreCadastrado_DeveAssociarAContaDeBarbeiroEmVezDeErroDeBanco()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiroDao = new BarbeiroDao(context);
        var usuarioDao = new UsuarioDao(context);
        var barbeiroService = new BarbeiroService(
            barbeiroDao, usuarioDao, new FakeCurrentUserService(), new FakeLogAlteracaoPublisher());
        var criadoBarbeiro = await barbeiroService.CriarAsync(
            new CreateBarbeiroDto(
                "João Barbeiro", "maria@email.com", "11999999999", null,
                new DateOnly(1990, 5, 20), InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro),
            CancellationToken.None);

        var controller = CriarController(context: context);

        var resultado = await controller.Create(CriarDtoValido(), CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<BarbeiroDto>().Subject;
        dto.Id.Should().Be(criadoBarbeiro.Value.Id);

        var porAuthUser = await barbeiroService.ObterPorAuthUserIdAsync(AuthUserId, CancellationToken.None);
        porAuthUser.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Create_QuandoAuthUserJaEhBarbeiroAssociado_DeveRetornarOCadastroDeBarbeiroEmVezDeErro()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiroDao = new BarbeiroDao(context);
        var usuarioDao = new UsuarioDao(context);
        var barbeiroService = new BarbeiroService(
            barbeiroDao, usuarioDao, new FakeCurrentUserService(), new FakeLogAlteracaoPublisher());
        var criadoBarbeiro = await barbeiroService.CriarAsync(
            new CreateBarbeiroDto(
                "João Barbeiro", "maria@email.com", "11999999999", null,
                new DateOnly(1990, 5, 20), InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro),
            CancellationToken.None);

        var controller = CriarController(context: context);

        // Primeiro acesso (login) associa a conta de barbeiro.
        await controller.Create(CriarDtoValido(), CancellationToken.None);

        // Em um acesso (login) seguinte, o frontend chama POST /clientes novamente;
        // isso não deve mais falhar com Cliente.ContaDeOutroTipo.
        var resultado = await controller.Create(CriarDtoValido(), CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<BarbeiroDto>().Which.Id.Should().Be(criadoBarbeiro.Value.Id);
    }

    [Fact]
    public async Task GetMe_QuandoAindaNaoTemCadastro_DeveRetornarNotFound()
    {
        var controller = CriarController();

        var resultado = await controller.GetMe(CancellationToken.None);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMe_QuandoJaTemCadastro_DeveRetornarOCliente()
    {
        var controller = CriarController();
        await controller.Create(CriarDtoValido(), CancellationToken.None);

        var resultado = await controller.GetMe(CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ClienteDto>().Which.Email.Should().Be("maria@email.com");
    }

    [Fact]
    public async Task GetById_QuandoClienteNaoExiste_DeveRetornarNotFound()
    {
        var controller = CriarController();

        var resultado = await controller.GetById(999, CancellationToken.None);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_QuandoClienteExiste_DevePersistirAsAlteracoes()
    {
        var controller = CriarController();
        var criado = (CreatedAtActionResult)await controller.Create(CriarDtoValido(), CancellationToken.None);
        var id = ((ClienteDto)criado.Value!).Id;

        var resultado = await controller.Update(id, CriarDtoAtualizadoValido(), CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<ClienteDto>().Subject;
        dto.Nome.Should().Be("Maria Oliveira");
    }

    [Fact]
    public async Task Update_QuandoClienteNaoPertenceAoUsuarioAutenticado_DeveRetornarNotFound()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var controllerDoDono = CriarController(authUserId: "auth-user-1", context: context);
        var criado = (CreatedAtActionResult)await controllerDoDono.Create(CriarDtoValido(), CancellationToken.None);
        var id = ((ClienteDto)criado.Value!).Id;

        var controllerDeOutroUsuario = CriarController(authUserId: "auth-user-2", context: context);
        var resultado = await controllerDeOutroUsuario.Update(id, CriarDtoAtualizadoValido(), CancellationToken.None);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_QuandoClienteExiste_DeveRemoverDaBase()
    {
        var controller = CriarController();
        var criado = (CreatedAtActionResult)await controller.Create(CriarDtoValido(), CancellationToken.None);
        var id = ((ClienteDto)criado.Value!).Id;

        var resultadoDelete = await controller.Delete(id, CancellationToken.None);
        var resultadoGet = await controller.GetById(id, CancellationToken.None);

        resultadoDelete.Should().BeOfType<NoContentResult>();
        resultadoGet.Should().BeOfType<NotFoundObjectResult>();
    }
}
