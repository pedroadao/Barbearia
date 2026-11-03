namespace Barbearia.Domain.Entities;

public sealed class Barbeiro : Usuario
{
    private Barbeiro()
    {
    }

    public Barbeiro(
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
