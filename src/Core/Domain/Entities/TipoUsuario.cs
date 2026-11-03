namespace Barbearia.Domain.Entities;

public sealed class TipoUsuario
{
    private TipoUsuario()
    {
    }

    public TipoUsuario(string descricao, int idStatus)
    {
        Descricao = descricao;
        IdStatus = idStatus;
    }

    public int Id { get; private set; }

    public string Descricao { get; private set; } = string.Empty;

    public int IdStatus { get; private set; }

    public Status? Status { get; private set; }
}
