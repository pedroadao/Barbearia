using Barbearia.Domain.Entities;

namespace Barbearia.Application.DTO;

public sealed record ClienteDto(
    int Id,
    string Nome,
    string Email,
    string Telefone,
    string? Instagram,
    DateOnly DataNascimento,
    int IdTipoUsuario,
    bool MostrarCadastroSempre);

public sealed record CreateClienteDto(
    string Nome,
    string Email,
    string Telefone,
    string? Instagram,
    DateOnly DataNascimento,
    int IdTipoUsuario);

public sealed record UpdateClienteDto(
    string Nome,
    string Email,
    string Telefone,
    string? Instagram,
    DateOnly DataNascimento,
    int IdTipoUsuario,
    bool MostrarCadastroSempre);

public static class ClienteMappingExtensions
{
    public static ClienteDto ToDto(this Cliente cliente) => new(
        cliente.Id,
        cliente.Nome,
        cliente.Email,
        cliente.Telefone,
        cliente.Instagram,
        cliente.DataNascimento,
        cliente.IdTipoUsuario,
        cliente.MostrarCadastroSempre);
}
