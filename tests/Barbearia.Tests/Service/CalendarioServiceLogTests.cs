using Barbearia.Application.DTO;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Fakes;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;

namespace Barbearia.Tests.Service;

public sealed class CalendarioServiceLogTests
{
    private static CreateCalendarioDto CriarDtoValido(int? idCliente = null) => new(
        IdBarbeiro: 1,
        Data: new DateOnly(2026, 11, 10),
        HoraInicio: new TimeOnly(9, 0),
        HoraFim: new TimeOnly(10, 0),
        IdCliente: idCliente);

    private static CalendarioService CriarService(
        Barbearia.Infrastructure.Dao.Interface.ICalendarioDao calendarioDao,
        FakeLogAlteracaoPublisher publisher,
        FakeWhatsAppNotificationService whatsAppNotificationService,
        Barbearia.Infrastructure.AppDbContext context,
        int? idUsuarioAtual = 1) =>
        new(
            calendarioDao,
            new UsuarioDao(context),
            new FakeCurrentUserService(idUsuarioAtual),
            publisher,
            whatsAppNotificationService);

    [Fact]
    public async Task CriarAsync_QuandoBemSucedido_DevePublicarLogComOperacaoCriado()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var service = CriarService(new CalendarioDao(context), publisher, new FakeWhatsAppNotificationService(), context, idUsuarioAtual: 42);

        var resultado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        publisher.MensagensPublicadas.Should().ContainSingle();
        var mensagem = publisher.MensagensPublicadas.Single();
        mensagem.Operacao.Should().Be(TipoOperacaoLog.Criado);
        mensagem.Tabela.Should().Be("Calendarios");
        mensagem.Metodo.Should().Be("CriarAsync");
        mensagem.IdUsuario.Should().Be(42);
    }

    [Fact]
    public async Task RemoverAsync_QuandoBemSucedido_DevePublicarLogComOperacaoExcluido()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var dao = new CalendarioDao(context);
        var service = CriarService(dao, publisher, new FakeWhatsAppNotificationService(), context, idUsuarioAtual: 7);
        var criado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        var resultado = await service.RemoverAsync(criado.Value.Id, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        publisher.MensagensPublicadas.Should().HaveCount(2);
        publisher.MensagensPublicadas[1].Operacao.Should().Be(TipoOperacaoLog.Excluido);
    }

    [Fact]
    public async Task CriarAsync_QuandoConflitoDeHorario_NaoDevePublicarLog()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var dao = new CalendarioDao(context);
        var service = CriarService(dao, publisher, new FakeWhatsAppNotificationService(), context);
        await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        var resultado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        publisher.MensagensPublicadas.Should().HaveCount(1);
    }

    [Fact]
    public async Task CriarAsync_QuandoTemCliente_DeveEnviarWhatsAppParaOBarbeiro()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var barbeiro = new Barbeiro(
            "João Barbeiro",
            "joao@barbearia.com",
            "+5511999990000",
            instagram: null,
            new DateOnly(1990, 1, 1),
            InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);
        var cliente = new Cliente(
            "Maria Cliente",
            "maria@cliente.com",
            "+5511988880000",
            instagram: null,
            new DateOnly(1995, 1, 1),
            InMemoryAppDbContextFactory.IdTipoUsuarioCliente);
        context.Usuarios.AddRange(barbeiro, cliente);
        await context.SaveChangesAsync();

        var publisher = new FakeLogAlteracaoPublisher();
        var whatsApp = new FakeWhatsAppNotificationService();
        var dao = new CalendarioDao(context);
        var service = CriarService(dao, publisher, whatsApp, context);

        var dto = new CreateCalendarioDto(
            IdBarbeiro: barbeiro.Id,
            Data: new DateOnly(2026, 11, 10),
            HoraInicio: new TimeOnly(9, 0),
            HoraFim: new TimeOnly(10, 0),
            IdCliente: cliente.Id);

        var resultado = await service.CriarAsync(dto, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        whatsApp.MensagensEnviadas.Should().ContainSingle();
        var (telefone, mensagem) = whatsApp.MensagensEnviadas.Single();
        telefone.Should().Be(barbeiro.Telefone);
        mensagem.Should().Contain(cliente.Nome);
    }

    [Fact]
    public async Task CriarAsync_SemCliente_NaoDeveEnviarWhatsApp()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var whatsApp = new FakeWhatsAppNotificationService();
        var dao = new CalendarioDao(context);
        var service = CriarService(dao, publisher, whatsApp, context);

        var resultado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        whatsApp.MensagensEnviadas.Should().BeEmpty();
    }
}
