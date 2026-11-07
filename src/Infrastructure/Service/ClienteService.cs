using Barbearia.Application.DTO;
using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Errors;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Service.Interface;

namespace Barbearia.Infrastructure.Service;

public sealed class ClienteService(IClienteDao clienteDao) : IClienteService
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

        return cliente is null
            ? Result.Failure<ClienteDto>(DomainErrors.Cliente.NotFoundPorAuthUser())
            : cliente.ToDto();
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

        var cliente = new Cliente(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario);

        var criado = await clienteDao.AddAsync(cliente, authUserId, cancellationToken);

        return criado.ToDto();
    }

    public async Task<Result<ClienteDto>> AtualizarAsync(
        int id,
        UpdateClienteDto dto,
        string authUserId,
        CancellationToken cancellationToken)
    {
        var cliente = await clienteDao.GetByIdAsync(id, cancellationToken);

        if (cliente is null)
            return Result.Failure<ClienteDto>(DomainErrors.Cliente.NotFound(id));

        if (cliente.AuthUserId != authUserId)
            return Result.Failure<ClienteDto>(
                DomainErrors.Cliente.NotFound(id)); // não revela a outro usuário que o id existe

        var atualizado = await clienteDao.UpdateAsync(
            id,
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario,
            dto.MostrarCadastroSempre,
            cancellationToken);

        if (!atualizado)
            return Result.Failure<ClienteDto>(DomainErrors.Cliente.NotFound(id));

        var atualizadoCliente = await clienteDao.GetByIdAsync(id, cancellationToken);

        return atualizadoCliente!.ToDto();
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await clienteDao.DeleteAsync(id, cancellationToken);

        return removido ? Result.Success() : Result.Failure(DomainErrors.Cliente.NotFound(id));
    }
}
