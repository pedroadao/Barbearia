using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Fakes;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Tests.Service;

public sealed class CalendarioServiceClienteAvulsoTests
{
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.Now);
    private static readonly DateTime Agora14h = Hoje.ToDateTime(new TimeOnly(14, 0));

    private static Barbeiro CriarBarbeiro() => new(
        "João Barbeiro",
        "joao@barbearia.com",
        "+5511999990000",
        instagram: null,
        new DateOnly(1990, 1, 1),
        InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);

    private static CalendarioService CriarService(
        Barbearia.Infrastructure.AppDbContext context,
        DateTime? agora = null,
        FakeLogAlteracaoPublisher? publisher = null) =>
        new(
            new CalendarioDao(context),
            new UsuarioDao(context),
            new FakeCurrentUserService(1),
            publisher ?? new FakeLogAlteracaoPublisher(),
            new FakeWhatsAppNotificationService(),
            () => agora ?? Agora14h);

    [Fact]
    public async Task AdicionarClienteAvulsoAsync_QuandoBarbeiroNaoExiste_DeveRetornarFalha()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var service = CriarService(context);

        var resultado = await service.AdicionarClienteAvulsoAsync(999, "Cliente Avulso", CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AdicionarClienteAvulsoAsync_QuandoNaoHaNenhumClienteNaFila_DeveEntrarNaHoraAtual()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        var service = CriarService(context);

        var resultado = await service.AdicionarClienteAvulsoAsync(barbeiro.Id, "Cliente Avulso", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.HoraInicio.Should().Be(new TimeOnly(14, 0));
        resultado.Value.HoraFim.Should().Be(new TimeOnly(14, 30));
        resultado.Value.NomeClienteAvulso.Should().Be("Cliente Avulso");
        resultado.Value.Disponivel.Should().BeFalse();
    }

    [Fact]
    public async Task AdicionarClienteAvulsoAsync_QuandoHaClienteApenasNoFuturo_DeveEntrarNaHoraAtual()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        context.Calendarios.Add(new Calendario(
            barbeiro.Id, Hoje, new TimeOnly(16, 0), new TimeOnly(16, 30), idCliente: 1, disponivel: false));
        await context.SaveChangesAsync();

        var service = CriarService(context);

        var resultado = await service.AdicionarClienteAvulsoAsync(barbeiro.Id, "Cliente Avulso", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.HoraInicio.Should().Be(new TimeOnly(14, 0));
    }

    [Fact]
    public async Task AdicionarClienteAvulsoAsync_QuandoJaHaClienteNaHoraAtual_DeveEntrarNaProximaMeiaHora()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        context.Calendarios.Add(new Calendario(
            barbeiro.Id, Hoje, new TimeOnly(14, 0), new TimeOnly(14, 30), idCliente: 1, disponivel: false));
        await context.SaveChangesAsync();

        var service = CriarService(context);

        var resultado = await service.AdicionarClienteAvulsoAsync(barbeiro.Id, "Cliente Avulso", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.HoraInicio.Should().Be(new TimeOnly(14, 30));

        (await context.Calendarios.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task AdicionarClienteAvulsoAsync_QuandoHaDoisClientesNaHoraAtualESeguinte_DeveEntrarNaMeiaHoraLivre()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        context.Calendarios.AddRange(
            new Calendario(barbeiro.Id, Hoje, new TimeOnly(14, 0), new TimeOnly(14, 30), idCliente: 1, disponivel: false),
            new Calendario(barbeiro.Id, Hoje, new TimeOnly(15, 0), new TimeOnly(15, 30), idCliente: 2, disponivel: false));
        await context.SaveChangesAsync();

        var service = CriarService(context);

        var resultado = await service.AdicionarClienteAvulsoAsync(barbeiro.Id, "Cliente Avulso", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.HoraInicio.Should().Be(new TimeOnly(14, 30));
    }

    [Fact]
    public async Task AdicionarClienteAvulsoAsync_QuandoHaHorarioDisponivelNaHoraAtual_DeveReservarEsseHorario()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        context.Calendarios.Add(new Calendario(
            barbeiro.Id, Hoje, new TimeOnly(14, 0), new TimeOnly(14, 30), idCliente: null, disponivel: true));
        await context.SaveChangesAsync();

        var service = CriarService(context);

        var resultado = await service.AdicionarClienteAvulsoAsync(barbeiro.Id, "Cliente Avulso", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.HoraInicio.Should().Be(new TimeOnly(14, 0));
        resultado.Value.IdCliente.Should().BeNull();
        resultado.Value.NomeClienteAvulso.Should().Be("Cliente Avulso");
        resultado.Value.Disponivel.Should().BeFalse();

        (await context.Calendarios.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task AdicionarClienteAvulsoAsync_QuandoAHoraAtualNaoEMultiplaDe30Minutos_DeveArredondarParaCima()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        var service = CriarService(context, Hoje.ToDateTime(new TimeOnly(14, 10)));

        var resultado = await service.AdicionarClienteAvulsoAsync(barbeiro.Id, "Cliente Avulso", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.HoraInicio.Should().Be(new TimeOnly(14, 30));
    }

    [Fact]
    public async Task AdicionarClienteAvulsoAsync_QuandoAHoraAtualEAntesDaAbertura_DeveEntrarNaAbertura()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        var service = CriarService(context, Hoje.ToDateTime(new TimeOnly(7, 0)));

        var resultado = await service.AdicionarClienteAvulsoAsync(barbeiro.Id, "Cliente Avulso", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.HoraInicio.Should().Be(new TimeOnly(9, 0));
    }

    [Fact]
    public async Task AdicionarClienteAvulsoAsync_QuandoBemSucedido_DevePublicarLog()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        var publisher = new FakeLogAlteracaoPublisher();
        var service = CriarService(context, publisher: publisher);

        var resultado = await service.AdicionarClienteAvulsoAsync(barbeiro.Id, "Cliente Avulso", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        publisher.MensagensPublicadas.Should().ContainSingle();
        publisher.MensagensPublicadas.Single().Metodo.Should().Be("AdicionarClienteAvulsoAsync");
    }
}
