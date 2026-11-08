using Barbearia.Application.DTO;
using Barbearia.Domain.Common;

namespace Barbearia.Infrastructure.Service.Interface;

public interface ILogAlteracaoService
{
    Task<Result<IReadOnlyList<LogAlteracaoDto>>> ObterComFiltroAsync(
        LogAlteracaoFiltroDto filtro,
        CancellationToken cancellationToken);
}
