using System.Runtime.CompilerServices;
using Barbearia.Application.Abstractions;
using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class ClienteService(
    IClienteDao clienteDao,
    IUsuarioDao usuarioDao,
    ICurrentUserService currentUserService,
    ILogAlteracaoPublisher logPublisher) : IClienteService
{
    public async Task<Result<ClienteDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await clienteDao.GetByIdAsync(id, cancellationToken);

        return cliente is null
            ? Result.Failure<ClienteDto>(DomainErrors.Cliente.NotFound(id))
            : cliente.ToDto();
    }

    public async Task<Result<ClienteDto>> ObterPorAuthUserIdAsync(string authUserId, CancellationToken cancellationToken)
    {
        var cliente = await clienteDao.GetByAuthUserIdAsync(authUserId, cancellationToken);
        if (cliente is not null)
            return cliente.ToDto();

        var usuarioDeOutroTipo = await usuarioDao.GetByAuthUserIdAsync(authUserId, cancellationToken);

        return usuarioDeOutroTipo is not null
            ? Result.Failure<ClienteDto>(DomainErrors.Cliente.ContaDeOutroTipo())
            : Result.Failure<ClienteDto>(DomainErrors.Cliente.NotFoundPorAuthUser());
    }

    public async Task<Result<IReadOnlyList<ClienteDto>>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var clientes = await clienteDao.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ClienteDto>>(clientes.Select(c => c.ToDto()).ToList());
    }

    public async Task<Result<ClienteDto>> CriarAsync(CreateClienteDto dto, string authUserId, CancellationToken cancellationToken)
    {
        var existente = await clienteDao.GetByAuthUserIdAsync(authUserId, cancellationToken);
        if (existente is not null)
            return Result.Failure<ClienteDto>(DomainErrors.Cliente.JaAssociado());

        var usuarioDeOutroTipo = await usuarioDao.GetByAuthUserIdAsync(authUserId, cancellationToken);
        if (usuarioDeOutroTipo is not null)
            return Result.Failure<ClienteDto>(DomainErrors.Cliente.ContaDeOutroTipo());

        var cliente = new Cliente(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario);

        var criado = await clienteDao.AddAsync(cliente, authUserId, cancellationToken);
        var criadoDto = criado.ToDto();

        await PublicarLogAsync(TipoOperacaoLog.Criado, criadoDto, cancellationToken);

        return criadoDto;
    }

    public async Task<Result<ClienteDto>> AtualizarAsync(
        int id,
        UpdateClienteDto dto,
        string authUserId,
        bool ehAdministrador,
        CancellationToken cancellationToken)
    {
        var cliente = await clienteDao.GetByIdAsync(id, cancellationToken);

        if (cliente is null)
            return Result.Failure<ClienteDto>(DomainErrors.Cliente.NotFound(id));

        if (!ehAdministrador && cliente.AuthUserId != authUserId)
            return Result.Failure<ClienteDto>(
                DomainErrors.Cliente.NotFound(id));

        var atualizado = await clienteDao.UpdateAsync(
            id,
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario,
            cancellationToken);

        if (!atualizado)
            return Result.Failure<ClienteDto>(DomainErrors.Cliente.NotFound(id));

        var atualizadoCliente = await clienteDao.GetByIdAsync(id, cancellationToken);
        var atualizadoDto = atualizadoCliente!.ToDto();

        await PublicarLogAsync(TipoOperacaoLog.Atualizado, atualizadoDto, cancellationToken);

        return atualizadoDto;
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await clienteDao.DeleteAsync(id, cancellationToken);

        if (!removido)
            return Result.Failure(DomainErrors.Cliente.NotFound(id));

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
