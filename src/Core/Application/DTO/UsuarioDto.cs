using Barbearia.Domain.Entities;

namespace Barbearia.Application.DTO;

public sealed record UsuarioDto(
    int Id,
    string Nome,
    string Email,
    string Telefone,
    string? Instagram,
    DateOnly DataNascimento,
    int IdTipoUsuario);

public sealed record CreateUsuarioDto(
    string Nome,
    string Email,
    string Telefone,
    string? Instagram,
    DateOnly DataNascimento,
    int IdTipoUsuario);

public sealed record UpdateUsuarioDto(
    string Nome,
    string Email,
    string Telefone,
    string? Instagram,
    DateOnly DataNascimento,
    int IdTipoUsuario);

public static class UsuarioMappingExtensions
{
    public static UsuarioDto ToDto(this Usuario usuario) => new(
        usuario.Id,
        usuario.Nome,
        usuario.Email,
        usuario.Telefone,
        usuario.Instagram,
        usuario.DataNascimento,
        usuario.IdTipoUsuario);
}
