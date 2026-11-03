using Barbearia.Api.Controllers;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Tests.Controllers;

public sealed class ClientesControllerTests
{
    private static ClientesController CriarController()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var dao = new ClienteDao(context);
        var service = new ClienteService(dao);

        return new ClientesController(service);
    }

    private static CreateClienteDto CriarDtoValido() => new(
        Nome: "Maria Souza",
        Email: "maria@email.com",
        Telefone: "11988888888",
        Instagram: null,
        DataNascimento: new DateOnly(1995, 3, 10),
        IdTipoUsuario: InMemoryAppDbContextFactory.IdTipoUsuarioCliente);

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

        var dtoAtualizado = new UpdateClienteDto(
            "Maria Oliveira", "maria.oliveira@email.com", "11911112222", "@mariaoliveira",
            new DateOnly(1995, 3, 10), InMemoryAppDbContextFactory.IdTipoUsuarioCliente);

        var resultado = await controller.Update(id, dtoAtualizado, CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ClienteDto>().Which.Nome.Should().Be("Maria Oliveira");
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
