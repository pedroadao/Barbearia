using System.Runtime.CompilerServices;
using Barbearia.Application.Abstractions;
using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class BarbeiroService(
    IBarbeiroDao barbeiroDao,
    IUsuarioDao usuarioDao,
    ICurrentUserService currentUserService,
    ILogAlteracaoPublisher logPublisher) : IBarbeiroService
{
    public async Task<Result<BarbeiroDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var barbeiro = await barbeiroDao.GetByIdAsync(id, cancellationToken);

        return barbeiro is null
            ? Result.Failure<BarbeiroDto>(DomainErrors.Barbeiro.NotFound(id))
            : barbeiro.ToDto();
    }

    public async Task<Result<BarbeiroDto>> ObterPorAuthUserIdAsync(string authUserId, CancellationToken cancellationToken)
    {
        var barbeiro = await barbeiroDao.GetByAuthUserIdAsync(authUserId, cancellationToken);

        return barbeiro is null
            ? Result.Failure<BarbeiroDto>(DomainErrors.Barbeiro.NotFoundPorAuthUser())
            : barbeiro.ToDto();
    }

    public async Task<Result<IReadOnlyList<BarbeiroDto>>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var barbeiros = await barbeiroDao.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<BarbeiroDto>>(barbeiros.Select(b => b.ToDto()).ToList());
    }

    public async Task<Result<BarbeiroDto>> CriarAsync(CreateBarbeiroDto dto, CancellationToken cancellationToken)
    {
        var usuarioComMesmoEmail = await usuarioDao.GetByEmailAsync(dto.Email, cancellationToken);
        if (usuarioComMesmoEmail is not null)
            return Result.Failure<BarbeiroDto>(DomainErrors.Barbeiro.EmailJaCadastrado());

        var barbeiro = new Barbeiro(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario);

        var criado = await barbeiroDao.AddAsync(barbeiro, cancellationToken);
        var criadoDto = criado.ToDto();

        await PublicarLogAsync(TipoOperacaoLog.Criado, criadoDto, cancellationToken);

        return criadoDto;
    }

    public async Task<Result<BarbeiroDto>> AssociarAsync(string authUserId, string email, CancellationToken cancellationToken)
    {
        var existente = await barbeiroDao.GetByAuthUserIdAsync(authUserId, cancellationToken);
        if (existente is not null)
            return Result.Failure<BarbeiroDto>(DomainErrors.Barbeiro.JaAssociado());

        var usuarioDeOutroTipo = await usuarioDao.GetByAuthUserIdAsync(authUserId, cancellationToken);
        if (usuarioDeOutroTipo is not null)
            return Result.Failure<BarbeiroDto>(DomainErrors.Barbeiro.ContaDeOutroTipo());

        var barbeiroPorEmail = await barbeiroDao.GetByEmailAsync(email, cancellationToken);
        if (barbeiroPorEmail is null)
            return Result.Failure<BarbeiroDto>(DomainErrors.Barbeiro.NotFoundPorEmail());

        if (barbeiroPorEmail.AuthUserId is not null)
            return Result.Failure<BarbeiroDto>(DomainErrors.Barbeiro.JaAssociado());

        var associado = await barbeiroDao.AssociarAuthUserAsync(barbeiroPorEmail.Id, authUserId, cancellationToken);
        var associadoDto = associado!.ToDto();

        await PublicarLogAsync(TipoOperacaoLog.Atualizado, associadoDto, cancellationToken);

        return associadoDto;
    }

    public async Task<bool> ExistePendenteAssociacaoPorEmailAsync(string email, CancellationToken cancellationToken)
    {
        var barbeiro = await barbeiroDao.GetByEmailAsync(email, cancellationToken);

        return barbeiro is not null && barbeiro.AuthUserId is null;
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
        var atualizadoDto = barbeiro!.ToDto();

        await PublicarLogAsync(TipoOperacaoLog.Atualizado, atualizadoDto, cancellationToken);

        return atualizadoDto;
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await barbeiroDao.DeleteAsync(id, cancellationToken);

        if (!removido)
            return Result.Failure(DomainErrors.Barbeiro.NotFound(id));

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
