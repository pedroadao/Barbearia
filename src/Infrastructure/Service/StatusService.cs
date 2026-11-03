using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class StatusService(IStatusDao statusDao) : IStatusService
{
    public async Task<Result<StatusDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var status = await statusDao.GetByIdAsync(id, cancellationToken);

        return status is null
            ? Result.Failure<StatusDto>(DomainErrors.Status.NotFound(id))
            : status.ToDto();
    }

    public async Task<Result<IReadOnlyList<StatusDto>>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var status = await statusDao.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<StatusDto>>(status.Select(s => s.ToDto()).ToList());
    }

    public async Task<Result<StatusDto>> CriarAsync(CreateStatusDto dto, CancellationToken cancellationToken)
    {
        var status = new Status(dto.Descricao);

        var criado = await statusDao.AddAsync(status, cancellationToken);

        return criado.ToDto();
    }

    public async Task<Result<StatusDto>> AtualizarAsync(int id, UpdateStatusDto dto, CancellationToken cancellationToken)
    {
        var atualizado = await statusDao.UpdateAsync(id, dto.Descricao, cancellationToken);

        if (!atualizado)
            return Result.Failure<StatusDto>(DomainErrors.Status.NotFound(id));

        var status = await statusDao.GetByIdAsync(id, cancellationToken);

        return status!.ToDto();
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await statusDao.DeleteAsync(id, cancellationToken);

        return removido ? Result.Success() : Result.Failure(DomainErrors.Status.NotFound(id));
    }
}
