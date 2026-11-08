using System.Runtime.CompilerServices;
using Barbearia.Application.Abstractions;
using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class UsuarioService(
    IUsuarioDao usuarioDao,
    ICurrentUserService currentUserService,
    ILogAlteracaoPublisher logPublisher) : IUsuarioService
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
        var criadoDto = criado.ToDto();

        await PublicarLogAsync(TipoOperacaoLog.Criado, criadoDto, cancellationToken);

        return criadoDto;
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
        var atualizadoDto = usuario!.ToDto();

        await PublicarLogAsync(TipoOperacaoLog.Atualizado, atualizadoDto, cancellationToken);

        return atualizadoDto;
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await usuarioDao.DeleteAsync(id, cancellationToken);

        if (!removido)
            return Result.Failure(DomainErrors.Usuario.NotFound(id));

        await PublicarLogAsync(TipoOperacaoLog.Excluido, new { Id = id }, cancellationToken);

        return Result.Success();
    }

    private async Task PublicarLogAsync(
        TipoOperacaoLog operacao,
        object dados,
        CancellationToken cancellationToken,
        [CallerMemberName] string metodo = "")
    {
        var idUsuarioAtual = await currentUserService.ObterIdUsuarioAtualAsync(cancellationToken);
        if (idUsuarioAtual is null)
            return;

        await logPublisher.PublicarAsync(
            new LogAlteracaoMensagem(operacao, "Usuarios", metodo, dados, idUsuarioAtual.Value, DateTime.UtcNow),
            cancellationToken);
    }
}
