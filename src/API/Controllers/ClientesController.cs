using Barbearia.Api.Extensions;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Api.Controllers;

[ApiController]
[Route("clientes")]
public sealed class ClientesController(IClienteService clienteService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var resultado = await clienteService.ObterTodosAsync(cancellationToken);

        return Ok(resultado.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resultado = await clienteService.ObterPorIdAsync(id, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClienteDto dto, CancellationToken cancellationToken)
    {
        var resultado = await clienteService.CriarAsync(dto, cancellationToken);

        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : resultado.ToErrorActionResult(this);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateClienteDto dto, CancellationToken cancellationToken)
    {
        var resultado = await clienteService.AtualizarAsync(id, dto, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var resultado = await clienteService.RemoverAsync(id, cancellationToken);

        return resultado.IsSuccess ? NoContent() : resultado.ToErrorActionResult(this);
    }
}
