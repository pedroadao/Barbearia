using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class UsuarioService(IUsuarioDao usuarioDao) : IUsuarioService
{
    public async Task<Result<UsuarioDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioDao.GetByIdAsync(id, cancellationToken);

        return usuario is null
            ? Result.Failure<UsuarioDto>(DomainErrors.Usuario.NotFound(id))
            : usuario.ToDto();
    }

    public async Task<Result<IReadOnlyList<UsuarioDto>>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var usuarios = await usuarioDao.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<UsuarioDto>>(usuarios.Select(u => u.ToDto()).ToList());
    }

    public async Task<Result<UsuarioDto>> CriarAsync(CreateUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = new Usuario(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario);

        var criado = await usuarioDao.AddAsync(usuario, cancellationToken);

        return criado.ToDto();
    }

    public async Task<Result<UsuarioDto>> AtualizarAsync(int id, UpdateUsuarioDto dto, CancellationToken cancellationToken)
    {
        var atualizado = await usuarioDao.UpdateAsync(
            id,
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario,
            cancellationToken);

        if (!atualizado)
            return Result.Failure<UsuarioDto>(DomainErrors.Usuario.NotFound(id));

        var usuario = await usuarioDao.GetByIdAsync(id, cancellationToken);

        return usuario!.ToDto();
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await usuarioDao.DeleteAsync(id, cancellationToken);

        return removido ? Result.Success() : Result.Failure(DomainErrors.Usuario.NotFound(id));
    }
}
