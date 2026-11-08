using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Fakes;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;

namespace Barbearia.Tests.Service;

public sealed class CalendarioServiceLogTests
{
    private static CreateCalendarioDto CriarDtoValido() => new(
        IdBarbeiro: 1,
        Data: new DateOnly(2026, 11, 10),
        HoraInicio: new TimeOnly(9, 0),
        HoraFim: new TimeOnly(10, 0),
        IdCliente: null);

    [Fact]
    public async Task CriarAsync_QuandoBemSucedido_DevePublicarLogComOperacaoCriado()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var service = new CalendarioService(new CalendarioDao(context), new FakeCurrentUserService(idUsuario: 42), publisher);

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
        var service = new CalendarioService(dao, new FakeCurrentUserService(idUsuario: 7), publisher);
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
        var service = new CalendarioService(dao, new FakeCurrentUserService(), publisher);
        await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        var resultado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        publisher.MensagensPublicadas.Should().HaveCount(1);
    }
}
