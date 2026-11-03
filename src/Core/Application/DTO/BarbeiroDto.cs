using Barbearia.Domain.Entities;

namespace Barbearia.Application.DTO;

public sealed record BarbeiroDto(
    int Id,
    string Nome,
    string Email,
    string Telefone,
    string? Instagram,
    DateOnly DataNascimento,
    int IdTipoUsuario);

public sealed record CreateBarbeiroDto(
    string Nome,
    string Email,
    string Telefone,
    string? Instagram,
    DateOnly DataNascimento,
    int IdTipoUsuario);

public sealed record UpdateBarbeiroDto(
    string Nome,
    string Email,
    string Telefone,
    string? Instagram,
    DateOnly DataNascimento,
    int IdTipoUsuario);

public static class BarbeiroMappingExtensions
{
    public static BarbeiroDto ToDto(this Barbeiro barbeiro) => new(
        barbeiro.Id,
        barbeiro.Nome,
        barbeiro.Email,
        barbeiro.Telefone,
        barbeiro.Instagram,
        barbeiro.DataNascimento,
        barbeiro.IdTipoUsuario);
}
