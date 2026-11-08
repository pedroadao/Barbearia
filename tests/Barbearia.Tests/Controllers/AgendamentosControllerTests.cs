using Barbearia.Api.Controllers;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Fakes;
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
        var service = new CalendarioService(dao, new FakeCurrentUserService(), new FakeLogAlteracaoPublisher());

        return new AgendamentosController(service);
    }

    private static CreateCalendarioDto CriarDtoValido() => new(
        IdBarbeiro: 1,
        Data: new DateOnly(2026, 11, 10),
        HoraInicio: new TimeOnly(9, 0),
        HoraFim: new TimeOnly(10, 0),
        IdCliente: null);

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
    public async Task Create_ComClienteInformado_DeveSalvarIdClienteEMarcarComoIndisponivel()
    {
        var controller = CriarController();

        var resultado = await controller.Create(CriarDtoValido() with { IdCliente = 2 }, CancellationToken.None);

        var created = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        var dto = created.Value.Should().BeOfType<CalendarioDto>().Subject;
        dto.IdCliente.Should().Be(2);
        dto.Disponivel.Should().BeFalse();
    }

    [Fact]
    public async Task Create_ComHorarioSobrepostoParaOMesmoBarbeiro_DeveRetornarConflict()
    {
        var controller = CriarController();
        await controller.Create(CriarDtoValido(), CancellationToken.None); // 09:00-10:00

        var resultado = await controller.Create(
            CriarDtoValido() with { HoraInicio = new TimeOnly(9, 30), HoraFim = new TimeOnly(10, 30) },
            CancellationToken.None);

        resultado.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_ComHorarioSobrepostoParaOutroBarbeiro_DeveRetornarCreated()
    {
        var controller = CriarController();
        await controller.Create(CriarDtoValido(), CancellationToken.None); // barbeiro 1, 09:00-10:00

        var resultado = await controller.Create(CriarDtoValido() with { IdBarbeiro = 2 }, CancellationToken.None);

        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ComHorarioAdjacenteSemSobreposicao_DeveRetornarCreated()
    {
        var controller = CriarController();
        await controller.Create(CriarDtoValido(), CancellationToken.None); // 09:00-10:00

        var resultado = await controller.Create(
            CriarDtoValido() with { HoraInicio = new TimeOnly(10, 0), HoraFim = new TimeOnly(11, 0) },
            CancellationToken.None);

        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Theory]
    [InlineData(8, 30, 9, 30)] // antes das 09:00
    [InlineData(18, 30, 19, 30)] // depois das 19:00
    public async Task Create_ComHorarioForaDoFuncionamento_DeveRetornarBadRequest(
        int horaInicioHora, int horaInicioMinuto, int horaFimHora, int horaFimMinuto)
    {
        var controller = CriarController();

        var resultado = await controller.Create(
            CriarDtoValido() with
            {
                HoraInicio = new TimeOnly(horaInicioHora, horaInicioMinuto),
                HoraFim = new TimeOnly(horaFimHora, horaFimMinuto),
            },
            CancellationToken.None);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData(9, 10, 10, 0)] // início não múltiplo de 30 minutos
    [InlineData(9, 0, 9, 45)] // término não múltiplo de 30 minutos
    [InlineData(10, 0, 9, 30)] // término antes do início
    public async Task Create_ComIntervaloInvalido_DeveRetornarBadRequest(
        int horaInicioHora, int horaInicioMinuto, int horaFimHora, int horaFimMinuto)
    {
        var controller = CriarController();

        var resultado = await controller.Create(
            CriarDtoValido() with
            {
                HoraInicio = new TimeOnly(horaInicioHora, horaInicioMinuto),
                HoraFim = new TimeOnly(horaFimHora, horaFimMinuto),
            },
            CancellationToken.None);

        resultado.Should().BeOfType<BadRequestObjectResult>();
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
