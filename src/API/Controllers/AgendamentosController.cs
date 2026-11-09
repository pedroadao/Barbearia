using Barbearia.Api.Extensions;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Api.Controllers;

[ApiController]
[Authorize]
[Route("agendamentos")]
public sealed class AgendamentosController(ICalendarioService calendarioService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var resultado = await calendarioService.ObterTodosAsync(cancellationToken);

        return Ok(resultado.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resultado = await calendarioService.ObterPorIdAsync(id, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCalendarioDto dto, CancellationToken cancellationToken)
    {
        var resultado = await calendarioService.CriarAsync(dto, cancellationToken);

        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : resultado.ToErrorActionResult(this);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var resultado = await calendarioService.RemoverAsync(id, cancellationToken);

        return resultado.IsSuccess ? NoContent() : resultado.ToErrorActionResult(this);
    }

    [HttpPost("fila/cliente-avulso")]
    public async Task<IActionResult> AdicionarClienteAvulso(AdicionarClienteAvulsoDto dto, CancellationToken cancellationToken)
    {
        var resultado = await calendarioService.AdicionarClienteAvulsoAsync(dto.IdBarbeiro, dto.NomeCliente, cancellationToken);

        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : resultado.ToErrorActionResult(this);
    }

    [HttpPut("fila/reordenar")]
    public async Task<IActionResult> ReordenarFila(ReordenarFilaDto dto, CancellationToken cancellationToken)
    {
        var resultado = await calendarioService.ReordenarFilaAsync(dto.IdBarbeiro, dto.Data, dto.IdsEmOrdem, cancellationToken);

        return resultado.IsSuccess ? NoContent() : resultado.ToErrorActionResult(this);
    }
}
