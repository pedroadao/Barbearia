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

        public static Error NotFoundPorAuthUser() =>
            Error.NotFound("Cliente.NotFoundPorAuthUser", "Nenhum cadastro de cliente associado ao usuário autenticado.");

        public static Error JaAssociado() =>
            Error.Conflict("Cliente.JaAssociado", "O usuário autenticado já possui um cadastro de cliente.");

        public static Error ContaDeOutroTipo() =>
            Error.Conflict(
                "Cliente.ContaDeOutroTipo",
                "O usuário autenticado já possui um cadastro de outro tipo (barbeiro ou administrador) e não pode completar um cadastro de cliente.");
    }

    public static class Calendario
    {
        public static Error NotFound(int id) =>
            Error.NotFound("Calendario.NotFound", $"Horário com Id '{id}' não foi encontrado.");

        public static Error Conflito() =>
            Error.Conflict("Calendario.Conflito", "Já existe um horário cadastrado para este barbeiro que conflita com o intervalo informado.");

        public static Error ForaDoHorarioComercial() =>
            Error.Validation("Calendario.ForaDoHorarioComercial", "O horário deve estar entre 09:00 e 19:00.");

        public static Error IntervaloInvalido() =>
            Error.Validation(
                "Calendario.IntervaloInvalido",
                "A hora de início e a hora de término devem ser múltiplas de 30 minutos, com o término depois do início.");
    }
}
