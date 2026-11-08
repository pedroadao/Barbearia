using System.Text;
using Barbearia.Api.Authorization;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Api.Controllers;

[ApiController]
[Authorize(Policy = Roles.Administrador)]
[Route("log-alteracoes")]
public sealed class LogAlteracoesController(ILogAlteracaoService logAlteracaoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] LogAlteracaoFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var resultado = await logAlteracaoService.ObterComFiltroAsync(filtro, cancellationToken);

        return Ok(resultado.Value);
    }

    [HttpGet("exportar")]
    public async Task<IActionResult> Exportar(
        [FromQuery] LogAlteracaoFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var resultado = await logAlteracaoService.ObterComFiltroAsync(filtro, cancellationToken);

        var csv = GerarCsv(resultado.Value);
        var arquivo = Encoding.UTF8.GetBytes(csv);

        return File(arquivo, "text/csv", $"log-alteracoes-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static string GerarCsv(IReadOnlyList<LogAlteracaoDto> logs)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Id;DataAlteracao;NomeUsuario;Metodo;Tabela;Dados");

        foreach (var log in logs)
        {
            builder.AppendLine(string.Join(
                ';',
                log.Id,
                log.DataAlteracao.ToString("O"),
                log.NomeUsuario,
                log.Metodo,
                log.Tabela,
                EscaparCampoCsv(log.Dados)));
        }

        return builder.ToString();
    }

    private static string EscaparCampoCsv(string valor)
    {
        var escapado = valor.Replace("\"", "\"\"");
        return $"\"{escapado}\"";
    }
}
