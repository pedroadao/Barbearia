namespace Barbearia.Domain.Entities;

public sealed class Cliente : Usuario
{
    private Cliente()
    {
    }

    public Cliente(
        string nome,
        string email,
        string telefone,
        string? instagram,
        DateOnly dataNascimento,
        int idTipoUsuario)
        : base(nome, email, telefone, instagram, dataNascimento, idTipoUsuario)
    {
    }
}
