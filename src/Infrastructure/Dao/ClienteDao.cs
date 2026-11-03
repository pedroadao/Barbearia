using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Dao;

public sealed class ClienteDao(AppDbContext context) : IClienteDao
{
    public async Task<Cliente?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Cliente,
                cancellationToken);

        return usuario is null ? null : ParaCliente(usuario);
    }

    public async Task<IReadOnlyList<Cliente>> GetAllAsync(CancellationToken cancellationToken)
    {
        var usuarios = await context.Usuarios
            .AsNoTracking()
            .Where(u => u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Cliente)
            .ToListAsync(cancellationToken);

        return usuarios.Select(ParaCliente).ToList();
    }

    public async Task<Cliente> AddAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        var usuario = new Usuario(
            cliente.Nome,
            cliente.Email,
            cliente.Telefone,
            cliente.Instagram,
            cliente.DataNascimento,
            cliente.IdTipoUsuario);

        await context.Usuarios.AddAsync(usuario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return ParaCliente(usuario);
    }

    public async Task<bool> UpdateAsync(
        int id,
        string nome,
        string email,
        string telefone,
        string? instagram,
        DateOnly dataNascimento,
        int idTipoUsuario,
        CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Cliente,
                cancellationToken);

        if (usuario is null)
            return false;

        usuario.Atualizar(nome, email, telefone, instagram, dataNascimento, idTipoUsuario);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Cliente,
                cancellationToken);

        if (usuario is null)
            return false;

        context.Usuarios.Remove(usuario);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static Cliente ParaCliente(Usuario usuario)
    {
        var cliente = new Cliente(
            usuario.Nome,
            usuario.Email,
            usuario.Telefone,
            usuario.Instagram,
            usuario.DataNascimento,
            usuario.IdTipoUsuario);

        cliente.DefinirId(usuario.Id);

        return cliente;
    }
}
