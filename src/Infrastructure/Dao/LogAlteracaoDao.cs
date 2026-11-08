using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Dao;

public sealed class LogAlteracaoDao(AppDbContext context) : ILogAlteracaoDao
{
    public async Task AddAsync(LogAlteracao logAlteracao, CancellationToken cancellationToken)
    {
        await context.LogAlteracoes.AddAsync(logAlteracao, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LogAlteracao>> ObterComFiltroAsync(
        DateTime? dataInicio,
        DateTime? dataFim,
        int? idUsuario,
        CancellationToken cancellationToken)
    {
        var query = context.LogAlteracoes.AsNoTracking().AsQueryable();

        if (dataInicio is not null)
            query = query.Where(l => l.DataAlteracao >= dataInicio);

        if (dataFim is not null)
            query = query.Where(l => l.DataAlteracao <= dataFim);

        if (idUsuario is not null)
            query = query.Where(l => l.IdUsuario == idUsuario);

        return await query
            .OrderByDescending(l => l.DataAlteracao)
            .ToListAsync(cancellationToken);
    }
}
