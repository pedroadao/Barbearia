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
[Route("clientes")]
public sealed class ClientesController(IClienteService clienteService, IBarbeiroService barbeiroService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var resultado = await clienteService.ObterTodosAsync(cancellationToken);

        return Ok(resultado.Value);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var authUserId = ObterAuthUserId();
        if (authUserId is null)
            return Unauthorized();

        var resultado = await clienteService.ObterPorAuthUserIdAsync(authUserId, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [Authorize(Policy = Roles.Administrador)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resultado = await clienteService.ObterPorIdAsync(id, cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClienteDto dto, CancellationToken cancellationToken)
    {
        var authUserId = ObterAuthUserId();
        if (authUserId is null)
            return Unauthorized();

        // O usuário autenticado já é um barbeiro (login associado em um acesso anterior):
        // retorna o cadastro de barbeiro em vez de tentar criar um cliente.
        var barbeiroExistente = await barbeiroService.ObterPorAuthUserIdAsync(authUserId, cancellationToken);
        if (barbeiroExistente.IsSuccess)
            return Ok(barbeiroExistente.Value);

        // O e-mail já pertence a um barbeiro pré-cadastrado (pelo administrador) sem login
        // associado: redireciona para a associação de conta em vez de tentar criar um cliente,
        // o que violaria o índice único de e-mail.
        if (await barbeiroService.ExistePendenteAssociacaoPorEmailAsync(dto.Email, cancellationToken))
        {
            var resultadoAssociacao = await barbeiroService.AssociarAsync(authUserId, dto.Email, cancellationToken);

            return resultadoAssociacao.IsSuccess
                ? Ok(resultadoAssociacao.Value)
                : resultadoAssociacao.ToErrorActionResult(this);
        }

        var resultado = await clienteService.CriarAsync(dto, authUserId, cancellationToken);

        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value.Id }, resultado.Value)
            : resultado.ToErrorActionResult(this);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateClienteDto dto, CancellationToken cancellationToken)
    {
        var authUserId = ObterAuthUserId();
        if (authUserId is null)
            return Unauthorized();

        var resultado = await clienteService.AtualizarAsync(id, dto, authUserId, EhAdministrador(), cancellationToken);

        return resultado.IsSuccess ? Ok(resultado.Value) : resultado.ToErrorActionResult(this);
    }

    [Authorize(Policy = Roles.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var resultado = await clienteService.RemoverAsync(id, cancellationToken);

        return resultado.IsSuccess ? NoContent() : resultado.ToErrorActionResult(this);
    }

    // O handler JWT Bearer pode mapear o claim "sub" do token para ClaimTypes.NameIdentifier
    // (handler legado) ou mantê-lo como "sub" (handler atual), então checa os dois.
    private string? ObterAuthUserId() =>
        User?.FindFirstValue("sub") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool EhAdministrador() =>
        User?.Claims.Any(claim =>
            (claim.Type == "role" || claim.Type == ClaimTypes.Role)
            && claim.Value == Roles.Administrador) ?? false;
}
