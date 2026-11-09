using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Dao;

public sealed class BarbeiroDao(AppDbContext context) : IBarbeiroDao
{
    public async Task<Barbeiro?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro && u.IdStatus == StatusIds.Ativo,
                cancellationToken);

        return usuario is null ? null : ParaBarbeiro(usuario);
    }

    public async Task<Barbeiro?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.AuthUserId == authUserId
                    && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro
                    && u.IdStatus == StatusIds.Ativo,
                cancellationToken);

        return usuario is null ? null : ParaBarbeiro(usuario);
    }

    public async Task<Barbeiro?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Email == email
                    && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro
                    && u.IdStatus == StatusIds.Ativo,
                cancellationToken);

        return usuario is null ? null : ParaBarbeiro(usuario);
    }

    public async Task<IReadOnlyList<Barbeiro>> GetAllAsync(CancellationToken cancellationToken)
    {
        var usuarios = await context.Usuarios
            .AsNoTracking()
            .Where(u => u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro && u.IdStatus == StatusIds.Ativo)
            .ToListAsync(cancellationToken);

        return usuarios.Select(ParaBarbeiro).ToList();
    }

    public async Task<Barbeiro> AddAsync(Barbeiro barbeiro, CancellationToken cancellationToken)
    {
        var usuario = new Usuario(
            barbeiro.Nome,
            barbeiro.Email,
            barbeiro.Telefone,
            barbeiro.Instagram,
            barbeiro.DataNascimento,
            barbeiro.IdTipoUsuario);

        await context.Usuarios.AddAsync(usuario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return ParaBarbeiro(usuario);
    }

    public async Task<Barbeiro?> AssociarAuthUserAsync(int id, string authUserId, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro && u.IdStatus == StatusIds.Ativo,
                cancellationToken);

        if (usuario is null)
            return null;

        usuario.AssociarAuthUser(authUserId);
        await context.SaveChangesAsync(cancellationToken);

        return ParaBarbeiro(usuario);
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
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro && u.IdStatus == StatusIds.Ativo,
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
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro && u.IdStatus == StatusIds.Ativo,
                cancellationToken);

        if (usuario is null)
            return false;

        usuario.MarcarComoExcluido();
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static Barbeiro ParaBarbeiro(Usuario usuario)
    {
        var barbeiro = new Barbeiro(
            usuario.Nome,
            usuario.Email,
            usuario.Telefone,
            usuario.Instagram,
            usuario.DataNascimento,
            usuario.IdTipoUsuario);

        barbeiro.DefinirId(usuario.Id);
        barbeiro.AssociarAuthUser(usuario.AuthUserId);

        return barbeiro;
    }
}
