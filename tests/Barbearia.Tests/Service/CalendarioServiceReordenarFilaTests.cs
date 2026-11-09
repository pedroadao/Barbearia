using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Fakes;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;

namespace Barbearia.Tests.Service;

public sealed class CalendarioServiceReordenarFilaTests
{
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.Now);

    private static Barbeiro CriarBarbeiro() => new(
        "João Barbeiro",
        "joao@barbearia.com",
        "+5511999990000",
        instagram: null,
        new DateOnly(1990, 1, 1),
        InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);

    private static CalendarioService CriarService(
        Barbearia.Infrastructure.AppDbContext context,
        FakeLogAlteracaoPublisher? publisher = null) =>
        new(
            new CalendarioDao(context),
            new UsuarioDao(context),
            new FakeCurrentUserService(1),
            publisher ?? new FakeLogAlteracaoPublisher(),
            new FakeWhatsAppNotificationService());

    [Fact]
    public async Task ReordenarFilaAsync_QuandoListaCorresponde_DeveRedistribuirClientesPelosMesmosHorarios()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        var primeiro = new Calendario(
            barbeiro.Id, Hoje, new TimeOnly(9, 0), new TimeOnly(9, 30), idCliente: null, disponivel: false, nomeClienteAvulso: "Ana");
        var segundo = new Calendario(
            barbeiro.Id, Hoje, new TimeOnly(9, 30), new TimeOnly(10, 0), idCliente: null, disponivel: false, nomeClienteAvulso: "Beto");
        context.Calendarios.AddRange(primeiro, segundo);
        await context.SaveChangesAsync();

        var service = CriarService(context);

        var resultado = await service.ReordenarFilaAsync(
            barbeiro.Id, Hoje, [segundo.Id, primeiro.Id], CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();

        var atualizadoPrimeiroHorario = await context.Calendarios.FindAsync(primeiro.Id);
        var atualizadoSegundoHorario = await context.Calendarios.FindAsync(segundo.Id);

        atualizadoPrimeiroHorario!.NomeClienteAvulso.Should().Be("Beto");
        atualizadoSegundoHorario!.NomeClienteAvulso.Should().Be("Ana");
    }

    [Fact]
    public async Task ReordenarFilaAsync_QuandoListaNaoCorrespondeAFilaAtual_DeveRetornarFalha()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        context.Calendarios.Add(new Calendario(
            barbeiro.Id, Hoje, new TimeOnly(9, 0), new TimeOnly(9, 30), idCliente: null, disponivel: false, nomeClienteAvulso: "Ana"));
        await context.SaveChangesAsync();

        var service = CriarService(context);

        var resultado = await service.ReordenarFilaAsync(barbeiro.Id, Hoje, [999], CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ReordenarFilaAsync_QuandoBemSucedido_DevePublicarLog()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = CriarBarbeiro();
        context.Usuarios.Add(barbeiro);
        await context.SaveChangesAsync();

        var unico = new Calendario(
            barbeiro.Id, Hoje, new TimeOnly(9, 0), new TimeOnly(9, 30), idCliente: null, disponivel: false, nomeClienteAvulso: "Ana");
        context.Calendarios.Add(unico);
        await context.SaveChangesAsync();

        var publisher = new FakeLogAlteracaoPublisher();
        var service = CriarService(context, publisher);

        var resultado = await service.ReordenarFilaAsync(barbeiro.Id, Hoje, [unico.Id], CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        publisher.MensagensPublicadas.Should().ContainSingle();
        publisher.MensagensPublicadas.Single().Metodo.Should().Be("ReordenarFilaAsync");
    }
}
