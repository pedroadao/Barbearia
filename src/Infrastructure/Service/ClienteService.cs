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

    public async Task<Result<IReadOnlyList<ClienteDto>>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var clientes = await clienteDao.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ClienteDto>>(clientes.Select(c => c.ToDto()).ToList());
    }

    public async Task<Result<ClienteDto>> CriarAsync(CreateClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = new Cliente(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Instagram,
            dto.DataNascimento,
            dto.IdTipoUsuario);

        var criado = await clienteDao.AddAsync(cliente, cancellationToken);

        return criado.ToDto();
    }

    public async Task<Result<ClienteDto>> AtualizarAsync(int id, UpdateClienteDto dto, CancellationToken cancellationToken)
    {
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

        var cliente = await clienteDao.GetByIdAsync(id, cancellationToken);

        return cliente!.ToDto();
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await clienteDao.DeleteAsync(id, cancellationToken);

        return removido ? Result.Success() : Result.Failure(DomainErrors.Cliente.NotFound(id));
    }
}
