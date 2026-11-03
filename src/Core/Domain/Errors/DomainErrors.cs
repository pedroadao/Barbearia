using Barbearia.Domain.Common;

namespace Barbearia.Domain.Errors;

public static class DomainErrors
{
    public static class Status
    {
        public static Error NotFound(int id) =>
            Error.NotFound("Status.NotFound", $"Status com Id '{id}' não foi encontrado.");
    }

    public static class TipoUsuario
    {
        public static Error NotFound(int id) =>
            Error.NotFound("TipoUsuario.NotFound", $"Tipo de usuário com Id '{id}' não foi encontrado.");
    }

    public static class Usuario
    {
        public static Error NotFound(int id) =>
            Error.NotFound("Usuario.NotFound", $"Usuário com Id '{id}' não foi encontrado.");
    }

    public static class Barbeiro
    {
        public static Error NotFound(int id) =>
            Error.NotFound("Barbeiro.NotFound", $"Barbeiro com Id '{id}' não foi encontrado.");
    }

    public static class Cliente
    {
        public static Error NotFound(int id) =>
            Error.NotFound("Cliente.NotFound", $"Cliente com Id '{id}' não foi encontrado.");
    }

    public static class Calendario
    {
        public static Error NotFound(int id) =>
            Error.NotFound("Calendario.NotFound", $"Horário com Id '{id}' não foi encontrado.");
    }
}
