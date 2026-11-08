using Barbearia.Domain.Entities;

namespace Barbearia.Infrastructure.Dao.Interface;

public interface ILogAlteracaoDao
{
    Task AddAsync(LogAlteracao logAlteracao, CancellationToken cancellationToken);

    Task<IReadOnlyList<LogAlteracao>> ObterComFiltroAsync(
        DateTime? dataInicio,
        DateTime? dataFim,
        int? idUsuario,
        CancellationToken cancellationToken);
}
