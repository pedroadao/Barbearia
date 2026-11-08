namespace Barbearia.Domain.Entities;

public sealed class LogAlteracao
{
    public LogAlteracao(DateTime dataAlteracao, string dados, int idUsuario, string metodo, string tabela)
    {
        DataAlteracao = dataAlteracao;
        Dados = dados;
        IdUsuario = idUsuario;
        Metodo = metodo;
        Tabela = tabela;
    }

    public int Id { get; private set; }

    public DateTime DataAlteracao { get; }

    public string Dados { get; } = string.Empty;

    public int IdUsuario { get; }

    public string Metodo { get; } = string.Empty;

    public string Tabela { get; } = string.Empty;
}
