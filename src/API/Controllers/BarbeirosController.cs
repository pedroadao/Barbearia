using System.Security.Claims;
using Barbearia.Api.Authorization;
using Barbearia.Api.Extensions;
using Barbearia.Application.DTO;
using Barbearia.Infrastructure.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Api.Controllers;

[ApiController]
[Authorize]
[Route("barbeiros")]
public sealed class BarbeirosController(IBarbeiroService barbeiroService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.ObterTodosAsync(cancellationToken);

        return Ok(resultado.Value);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var authUserId = User?.FindFirstValue("sub") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (authUserId is null)
            return Unauthorized();

        var resultado = await barbeiroService.ObterPorAuthUserIdAsync(authUserId, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [HttpPost("associar")]
    public async Task<IActionResult> Associar(CancellationToken cancellationToken)
    {
        var authUserId = User?.FindFirstValue("sub") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User?.FindFirstValue("email") ?? User?.FindFirstValue(ClaimTypes.Email);

        if (authUserId is null || email is null)
            return Unauthorized();

        var resultado = await barbeiroService.AssociarAsync(authUserId, email, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [Authorize(Policy = Roles.Administrador)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.ObterPorIdAsync(id, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [Authorize(Policy = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateBarbeiroDto dto, CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.CriarAsync(dto, cancellationToken);

        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : resultado.ToErrorActionResult(this);
    }

    [Authorize(Policy = Roles.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateBarbeiroDto dto, CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.AtualizarAsync(id, dto, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [Authorize(Policy = Roles.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var resultado = await barbeiroService.RemoverAsync(id, cancellationToken);

        return resultado.IsSuccess ? NoContent() : resultado.ToErrorActionResult(this);
    }
}
