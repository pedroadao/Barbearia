using System.Text;
using Barbearia.Api.Controllers;
using Barbearia.Application.DTO;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Service;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Tests.Controllers;

public sealed class LogAlteracoesControllerTests
{
    private static LogAlteracoesController CriarController(out Barbearia.Infrastructure.AppDbContext context)
    {
        context = InMemoryAppDbContextFactory.Criar();
        var dao = new LogAlteracaoDao(context);
        var service = new LogAlteracaoService(dao, new UsuarioDao(context));

        return new LogAlteracoesController(service);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComLogsFiltrados()
    {
        var controller = CriarController(out var context);
        context.LogAlteracoes.Add(new LogAlteracao(
            DateTime.UtcNow, "{\"Nome\":\"Teste\"}", idUsuario: 1, metodo: "AtualizarAsync", tabela: "Usuarios"));
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await controller.GetAll(new LogAlteracaoFiltroDto(null, null, null), CancellationToken.None);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<List<LogAlteracaoDto>>().Which.Should().ContainSingle();
    }

    [Fact]
    public async Task Exportar_DeveRetornarArquivoCsvComCabecalhoELinhas()
    {
        var controller = CriarController(out var context);
        context.LogAlteracoes.Add(new LogAlteracao(
            new DateTime(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc),
            "{\"Nome\":\"Teste\"}",
            idUsuario: 7,
            metodo: "RemoverAsync",
            tabela: "Usuarios"));
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await controller.Exportar(new LogAlteracaoFiltroDto(null, null, null), CancellationToken.None);

        var arquivo = resultado.Should().BeOfType<FileContentResult>().Subject;
        arquivo.ContentType.Should().Be("text/csv");

        var conteudo = Encoding.UTF8.GetString(arquivo.FileContents);
        conteudo.Should().StartWith("Id;DataAlteracao;NomeUsuario;Metodo;Tabela;Dados");
        conteudo.Should().Contain(";Usuário removido;RemoverAsync;Usuarios;");
    }
}
