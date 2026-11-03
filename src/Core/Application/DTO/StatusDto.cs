using Barbearia.Domain.Entities;

namespace Barbearia.Application.DTO;

public sealed record StatusDto(int Id, string Descricao);

public sealed record CreateStatusDto(string Descricao);

public sealed record UpdateStatusDto(string Descricao);

public static class StatusMappingExtensions
{
    public static StatusDto ToDto(this Status status) => new(status.Id, status.Descricao);
}
