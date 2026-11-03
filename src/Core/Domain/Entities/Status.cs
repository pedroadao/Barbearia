namespace Barbearia.Domain.Entities;

public sealed class Status
{
    private Status()
    {
    }

    public Status(string descricao)
    {
        Descricao = descricao;
    }

    public int Id { get; private set; }

    public string Descricao { get; private set; } = string.Empty;
}
