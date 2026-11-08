using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Fakes;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;

namespace Barbearia.Tests.Service;

public sealed class BarbeiroServiceLogTests
{
    private static CreateBarbeiroDto CriarDtoValido() => new(
        Nome: "João Silva",
        Email: "joao@barbearia.com",
        Telefone: "11999999999",
        Instagram: "@joaosilva",
        DataNascimento: new DateOnly(1990, 5, 20),
        IdTipoUsuario: InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);

    private static UpdateBarbeiroDto CriarDtoAtualizadoValido() => new(
        "João Souza", "joao2@barbearia.com", "11988888888", null,
        new DateOnly(1990, 5, 20), InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);

    [Fact]
    public async Task AtualizarAsync_QuandoBemSucedido_DevePublicarLogComOperacaoAtualizado()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var dao = new BarbeiroDao(context);
        var service = new BarbeiroService(dao, new FakeCurrentUserService(idUsuario: 7), publisher);
        var criado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        var resultado = await service.AtualizarAsync(criado.Value.Id, CriarDtoAtualizadoValido(), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        publisher.MensagensPublicadas.Should().HaveCount(2);
        var mensagem = publisher.MensagensPublicadas[1];
        mensagem.Operacao.Should().Be(TipoOperacaoLog.Atualizado);
        mensagem.Tabela.Should().Be("Usuarios");
        mensagem.Metodo.Should().Be("AtualizarAsync");
    }

    [Fact]
    public async Task RemoverAsync_QuandoBemSucedido_DevePublicarLogComOperacaoExcluido()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var dao = new BarbeiroDao(context);
        var service = new BarbeiroService(dao, new FakeCurrentUserService(idUsuario: 7), publisher);
        var criado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        var resultado = await service.RemoverAsync(criado.Value.Id, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        publisher.MensagensPublicadas.Should().HaveCount(2);
        publisher.MensagensPublicadas[1].Operacao.Should().Be(TipoOperacaoLog.Excluido);
    }

    [Fact]
    public async Task RemoverAsync_QuandoBarbeiroNaoExiste_NaoDevePublicarLog()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var service = new BarbeiroService(new BarbeiroDao(context), new FakeCurrentUserService(), publisher);

        var resultado = await service.RemoverAsync(999, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        publisher.MensagensPublicadas.Should().BeEmpty();
    }
}
