namespace Barbearia.Application.DTO;

public enum TipoOperacaoLog
{
    Criado,
    Atualizado,
    Excluido
}

public sealed record LogAlteracaoMensagem(
    TipoOperacaoLog Operacao,
    string Tabela,
    string Metodo,
    object Dados,
    int IdUsuario,
    DateTime DataAlteracao);
