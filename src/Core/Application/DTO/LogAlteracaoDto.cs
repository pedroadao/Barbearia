using Barbearia.Domain.Entities;

namespace Barbearia.Application.DTO;

public sealed record LogAlteracaoDto(
    int Id,
    DateTime DataAlteracao,
    string Dados,
    int IdUsuario,
    string NomeUsuario,
    string Metodo,
    string Tabela);

public sealed record LogAlteracaoFiltroDto(
    DateTime? DataInicio,
    DateTime? DataFim,
    int? IdUsuario);

public static class LogAlteracaoMappingExtensions
{
    public static LogAlteracaoDto ToDto(this LogAlteracao logAlteracao, string nomeUsuario) => new(
        logAlteracao.Id,
        logAlteracao.DataAlteracao,
        logAlteracao.Dados,
        logAlteracao.IdUsuario,
        nomeUsuario,
        logAlteracao.Metodo,
        logAlteracao.Tabela);
}
