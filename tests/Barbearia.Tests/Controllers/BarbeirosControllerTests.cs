using Barbearia.Api.Controllers;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Tests.Controllers;

public sealed class BarbeirosControllerTests
{
    private static BarbeirosController CriarController()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var dao = new BarbeiroDao(context);
        var service = new BarbeiroService(dao);

        return new BarbeirosController(service);
    }

    private static CreateBarbeiroDto CriarDtoValido() => new(
        Nome: "João Silva",
        Email: "joao@barbearia.com",
        Telefone: "11999999999",
        Instagram: "@joaosilva",
        DataNascimento: new DateOnly(1990, 5, 20),
        IdTipoUsuario: InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);

    [Fact]
    public async Task Create_DeveRetornarCreatedComIdGeradoPeloBanco()
    {
        var controller = CriarController();

        var resultado = await controller.Create(CriarDtoValido(), CancellationToken.None);

        var created = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        var dto = created.Value.Should().BeOfType<BarbeiroDto>().Subject;
        dto.Id.Should().BeGreaterThan(0);
        dto.Nome.Should().Be("João Silva");
    }

    [Fact]
    public async Task GetById_QuandoBarbeiroExiste_DeveRetornarOk()
    {
        var controller = CriarController();
        var criado = (CreatedAtActionResult)await controller.Create(CriarDtoValido(), CancellationToken.None);
        var dtoCreated = (BarbeiroDto)criado.Value!;

        var resultado = await controller.GetById(dtoCreated.Id, CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<BarbeiroDto>().Which.Id.Should().Be(dtoCreated.Id);
    }

    [Fact]
    public async Task GetById_QuandoBarbeiroNaoExiste_DeveRetornarNotFound()
    {
        var controller = CriarController();

        var resultado = await controller.GetById(999, CancellationToken.None);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAll_NaoDeveRetornarClientesCadastradosComOMesmoTipoUsuario()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiroDao = new BarbeiroDao(context);
        var clienteDao = new ClienteDao(context);
        var controller = new BarbeirosController(new BarbeiroService(barbeiroDao));

        await controller.Create(CriarDtoValido(), CancellationToken.None);
        await new ClientesController(new ClienteService(clienteDao)).Create(
            new CreateClienteDto("Maria", "maria@email.com", "11988888888", null,
                new DateOnly(1995, 1, 1), InMemoryAppDbContextFactory.IdTipoUsuarioCliente),
            CancellationToken.None);

        var resultado = await controller.GetAll(CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var lista = ok.Value.Should().BeAssignableTo<IReadOnlyList<BarbeiroDto>>().Subject;
        lista.Should().HaveCount(1);
        lista.Single().Nome.Should().Be("João Silva");
    }

    [Fact]
    public async Task Update_QuandoBarbeiroExiste_DevePersistirAsAlteracoes()
    {
        var controller = CriarController();
        var criado = (CreatedAtActionResult)await controller.Create(CriarDtoValido(), CancellationToken.None);
        var id = ((BarbeiroDto)criado.Value!).Id;

        var dtoAtualizado = new UpdateBarbeiroDto(
            "João Pereira", "joao.pereira@barbearia.com", "11977777777", null,
            new DateOnly(1990, 5, 20), InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);

        var resultado = await controller.Update(id, dtoAtualizado, CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<BarbeiroDto>().Which.Nome.Should().Be("João Pereira");
    }

    [Fact]
    public async Task Delete_QuandoBarbeiroExiste_DeveRemoverDaBase()
    {
        var controller = CriarController();
        var criado = (CreatedAtActionResult)await controller.Create(CriarDtoValido(), CancellationToken.None);
        var id = ((BarbeiroDto)criado.Value!).Id;

        var resultadoDelete = await controller.Delete(id, CancellationToken.None);
        var resultadoGet = await controller.GetById(id, CancellationToken.None);

        resultadoDelete.Should().BeOfType<NoContentResult>();
        resultadoGet.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_QuandoBarbeiroNaoExiste_DeveRetornarNotFound()
    {
        var controller = CriarController();

        var resultado = await controller.Delete(999, CancellationToken.None);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }
}
