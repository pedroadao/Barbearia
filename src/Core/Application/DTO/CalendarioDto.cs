using Barbearia.Domain.Entities;

namespace Barbearia.Application.DTO;

public sealed record CalendarioDto(
    int Id,
    int IdBarbeiro,
    DateOnly Data,
    TimeOnly HoraInicio,
    TimeOnly HoraFim,
    int? IdCliente,
    bool Disponivel);

public sealed record CreateCalendarioDto(
    int IdBarbeiro,
    DateOnly Data,
    TimeOnly HoraInicio,
    TimeOnly HoraFim);

public sealed record UpdateCalendarioDto(
    int IdBarbeiro,
    DateOnly Data,
    TimeOnly HoraInicio,
    TimeOnly HoraFim,
    int? IdCliente,
    bool Disponivel);

public static class CalendarioMappingExtensions
{
    public static CalendarioDto ToDto(this Calendario calendario) => new(
        calendario.Id,
        calendario.IdBarbeiro,
        calendario.Data,
        calendario.HoraInicio,
        calendario.HoraFim,
        calendario.IdCliente,
        calendario.Disponivel);
}
