using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class TipoUsuarioService(ITipoUsuarioDao tipoUsuarioDao) : ITipoUsuarioService
{
    public async Task<Result<TipoUsuarioDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var tipoUsuario = await tipoUsuarioDao.GetByIdAsync(id, cancellationToken);

        return tipoUsuario is null
            ? Result.Failure<TipoUsuarioDto>(DomainErrors.TipoUsuario.NotFound(id))
            : tipoUsuario.ToDto();
    }

    public async Task<Result<IReadOnlyList<TipoUsuarioDto>>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var tipos = await tipoUsuarioDao.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TipoUsuarioDto>>(tipos.Select(t => t.ToDto()).ToList());
    }

    public async Task<Result<TipoUsuarioDto>> CriarAsync(CreateTipoUsuarioDto dto, CancellationToken cancellationToken)
    {
        var tipoUsuario = new TipoUsuario(dto.Descricao, dto.IdStatus);

        var criado = await tipoUsuarioDao.AddAsync(tipoUsuario, cancellationToken);

        return criado.ToDto();
    }

    public async Task<Result<TipoUsuarioDto>> AtualizarAsync(
        int id,
        UpdateTipoUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        var atualizado = await tipoUsuarioDao.UpdateAsync(id, dto.Descricao, dto.IdStatus, cancellationToken);

        if (!atualizado)
            return Result.Failure<TipoUsuarioDto>(DomainErrors.TipoUsuario.NotFound(id));

        var tipoUsuario = await tipoUsuarioDao.GetByIdAsync(id, cancellationToken);

        return tipoUsuario!.ToDto();
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await tipoUsuarioDao.DeleteAsync(id, cancellationToken);

        return removido ? Result.Success() : Result.Failure(DomainErrors.TipoUsuario.NotFound(id));
    }
}
