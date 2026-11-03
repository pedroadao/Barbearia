using Barbearia.Api.Controllers;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Tests.Controllers;

public sealed class AgendamentosControllerTests
{
    private static AgendamentosController CriarController()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var dao = new CalendarioDao(context);
        var service = new CalendarioService(dao);

        return new AgendamentosController(service);
    }

    private static CreateCalendarioDto CriarDtoValido() => new(
        IdBarbeiro: 1,
        Data: new DateOnly(2026, 11, 10),
        HoraInicio: new TimeOnly(9, 0),
        HoraFim: new TimeOnly(10, 0));

    [Fact]
    public async Task Create_DeveRetornarCreatedComHorarioDisponivel()
    {
        var controller = CriarController();

        var resultado = await controller.Create(CriarDtoValido(), CancellationToken.None);

        var created = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        var dto = created.Value.Should().BeOfType<CalendarioDto>().Subject;
        dto.Id.Should().BeGreaterThan(0);
        dto.Disponivel.Should().BeTrue();
        dto.IdCliente.Should().BeNull();
    }

    [Fact]
    public async Task GetById_QuandoAgendamentoExiste_DeveRetornarOk()
    {
        var controller = CriarController();
        var criado = (CreatedAtActionResult)await controller.Create(CriarDtoValido(), CancellationToken.None);
        var id = ((CalendarioDto)criado.Value!).Id;

        var resultado = await controller.GetById(id, CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<CalendarioDto>().Which.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_QuandoAgendamentoNaoExiste_DeveRetornarNotFound()
    {
        var controller = CriarController();

        var resultado = await controller.GetById(999, CancellationToken.None);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAll_DeveRetornarTodosOsAgendamentosCriados()
    {
        var controller = CriarController();
        await controller.Create(CriarDtoValido(), CancellationToken.None);
        await controller.Create(CriarDtoValido() with { HoraInicio = new TimeOnly(11, 0), HoraFim = new TimeOnly(12, 0) }, CancellationToken.None);

        var resultado = await controller.GetAll(CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<CalendarioDto>>().Which.Should().HaveCount(2);
    }
}
