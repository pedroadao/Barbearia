using Barbearia.Application.DTO;
using Barbearia.Domain.Common;

namespace Barbearia.Infrastructure.Service.Interface;

public interface ICalendarioService
{
    Task<Result<CalendarioDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<CalendarioDto>>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<Result<CalendarioDto>> CriarAsync(CreateCalendarioDto dto, CancellationToken cancellationToken);

    Task<Result<CalendarioDto>> AtualizarAsync(int id, UpdateCalendarioDto dto, CancellationToken cancellationToken);

    Task<Result> RemoverAsync(int id, CancellationToken cancellationToken);
}
