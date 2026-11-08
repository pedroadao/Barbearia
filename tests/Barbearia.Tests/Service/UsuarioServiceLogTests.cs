using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Fakes;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;

namespace Barbearia.Tests.Service;

public sealed class UsuarioServiceLogTests
{
    private static CreateUsuarioDto CriarDtoValido() => new(
        Nome: "João Silva",
        Email: "joao@email.com",
        Telefone: "11999999999",
        Instagram: null,
        DataNascimento: new DateOnly(1990, 5, 20),
        IdTipoUsuario: InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro);

    [Fact]
    public async Task CriarAsync_QuandoBemSucedido_DevePublicarLogComOperacaoCriado()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var service = new UsuarioService(new UsuarioDao(context), new FakeCurrentUserService(idUsuario: 42), publisher);

        var resultado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        publisher.MensagensPublicadas.Should().ContainSingle();
        var mensagem = publisher.MensagensPublicadas.Single();
        mensagem.Operacao.Should().Be(TipoOperacaoLog.Criado);
        mensagem.Tabela.Should().Be("Usuarios");
        mensagem.Metodo.Should().Be("CriarAsync");
        mensagem.IdUsuario.Should().Be(42);
    }

    [Fact]
    public async Task CriarAsync_QuandoNaoHaUsuarioAutenticado_NaoDevePublicarLog()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var publisher = new FakeLogAlteracaoPublisher();
        var service = new UsuarioService(new UsuarioDao(context), new FakeCurrentUserService(idUsuario: null), publisher);

        var resultado = await service.CriarAsync(CriarDtoValido(), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        publisher.MensagensPublicadas.Should().BeEmpty();
    }
}
