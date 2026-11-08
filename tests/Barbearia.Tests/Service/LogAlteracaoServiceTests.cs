using Barbearia.Application.DTO;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;

namespace Barbearia.Tests.Service;

public sealed class LogAlteracaoServiceTests
{
    private static async Task<AppDbContext> CriarContextoComLogsAsync()
    {
        var context = InMemoryAppDbContextFactory.Criar();

        context.LogAlteracoes.AddRange(
            new LogAlteracao(new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc), "{\"Id\":1}", idUsuario: 1, metodo: "CriarAsync", tabela: "Usuarios"),
            new LogAlteracao(new DateTime(2026, 11, 5, 10, 0, 0, DateTimeKind.Utc), "{\"Id\":2}", idUsuario: 2, metodo: "AtualizarAsync", tabela: "Usuarios"),
            new LogAlteracao(new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc), "{\"Id\":3}", idUsuario: 1, metodo: "RemoverAsync", tabela: "Calendarios"));
        await context.SaveChangesAsync(CancellationToken.None);

        return context;
    }

    [Fact]
    public async Task ObterComFiltroAsync_SemFiltro_DeveRetornarTodosOrdenadosPorDataDesc()
    {
        var context = await CriarContextoComLogsAsync();
        var service = new LogAlteracaoService(new LogAlteracaoDao(context), new UsuarioDao(context));

        var resultado = await service.ObterComFiltroAsync(
            new LogAlteracaoFiltroDto(null, null, null), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().HaveCount(3);
        resultado.Value.Select(l => l.IdUsuario).Should().ContainInOrder(1, 2, 1);
    }

    [Fact]
    public async Task ObterComFiltroAsync_FiltrandoPorUsuario_DeveRetornarApenasDoUsuario()
    {
        var context = await CriarContextoComLogsAsync();
        var service = new LogAlteracaoService(new LogAlteracaoDao(context), new UsuarioDao(context));

        var resultado = await service.ObterComFiltroAsync(
            new LogAlteracaoFiltroDto(null, null, IdUsuario: 1), CancellationToken.None);

        resultado.Value.Should().HaveCount(2);
        resultado.Value.Should().OnlyContain(l => l.IdUsuario == 1);
    }

    [Fact]
    public async Task ObterComFiltroAsync_FiltrandoPorPeriodo_DeveRetornarApenasDoIntervalo()
    {
        var context = await CriarContextoComLogsAsync();
        var service = new LogAlteracaoService(new LogAlteracaoDao(context), new UsuarioDao(context));

        var resultado = await service.ObterComFiltroAsync(
            new LogAlteracaoFiltroDto(
                DataInicio: new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc),
                DataFim: new DateTime(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc),
                IdUsuario: null),
            CancellationToken.None);

        resultado.Value.Should().ContainSingle();
        resultado.Value.Single().IdUsuario.Should().Be(2);
    }
}
