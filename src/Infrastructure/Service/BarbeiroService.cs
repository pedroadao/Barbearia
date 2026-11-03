using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class BarbeiroService(IBarbeiroDao barbeiroDao) : IBarbeiroService
{
    public async Task<Result<BarbeiroDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var barbeiro = await barbeiroDao.GetByIdAsync(id, cancellationToken);

        return barbeiro is null
            ? Result.Failure<BarbeiroDto>(DomainErrors.Barbeiro.NotFound(id))
            : barbeiro.ToDto();
    }

    public async Task<Result<IReadOnlyList<BarbeiroDto>>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var barbeiros = await barbeiroDao.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<BarbeiroDto>>(barbeiros.Select(b => b.ToDto()).ToList());
    }

    public async Task<Result<BarbeiroDto>> CriarAsync(CreateBarbeiroDto dto, CancellationToken cancellationToken)
    {
        var barbeiro = new Barbeiro(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario);

        var criado = await barbeiroDao.AddAsync(barbeiro, cancellationToken);

        return criado.ToDto();
    }

    public async Task<Result<BarbeiroDto>> AtualizarAsync(int id, UpdateBarbeiroDto dto, CancellationToken cancellationToken)
    {
        var atualizado = await barbeiroDao.UpdateAsync(
            id,
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario,
            cancellationToken);

        if (!atualizado)
            return Result.Failure<BarbeiroDto>(DomainErrors.Barbeiro.NotFound(id));

        var barbeiro = await barbeiroDao.GetByIdAsync(id, cancellationToken);

        return barbeiro!.ToDto();
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await barbeiroDao.DeleteAsync(id, cancellationToken);

        return removido ? Result.Success() : Result.Failure(DomainErrors.Barbeiro.NotFound(id));
    }
}
