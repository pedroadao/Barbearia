using Barbearia.Domain.Entities;

namespace Barbearia.Application.DTO;

public sealed record TipoUsuarioDto(int Id, string Descricao, int IdStatus);

public sealed record CreateTipoUsuarioDto(string Descricao, int IdStatus);

public sealed record UpdateTipoUsuarioDto(string Descricao, int IdStatus);

public static class TipoUsuarioMappingExtensions
{
    public static TipoUsuarioDto ToDto(this TipoUsuario tipoUsuario) =>
        new(tipoUsuario.Id, tipoUsuario.Descricao, tipoUsuario.IdStatus);
}
