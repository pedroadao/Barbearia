using Barbearia.Api.Extensions;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Api.Controllers;

[ApiController]
[Route("barbeiros")]
public sealed class BarbeirosController(IBarbeiroService barbeiroService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.ObterTodosAsync(cancellationToken);

        return Ok(resultado.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.ObterPorIdAsync(id, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBarbeiroDto dto, CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.CriarAsync(dto, cancellationToken);

        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : resultado.ToErrorActionResult(this);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateBarbeiroDto dto, CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.AtualizarAsync(id, dto, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.RemoverAsync(id, cancellationToken);

        return resultado.IsSuccess ? NoContent() : resultado.ToErrorActionResult(this);
    }
}
