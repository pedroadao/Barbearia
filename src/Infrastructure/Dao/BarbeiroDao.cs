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
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro,
                cancellationToken);

        return usuario is null ? null : ParaBarbeiro(usuario);
    }

    public async Task<IReadOnlyList<Barbeiro>> GetAllAsync(CancellationToken cancellationToken)
    {
        var usuarios = await context.Usuarios
            .AsNoTracking()
            .Where(u => u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro)
            .ToListAsync(cancellationToken);

        return usuarios.Select(ParaBarbeiro).ToList();
    }

    // Decisão de design: como o Id agora é gerado pelo banco (identity), o objeto
    // "barbeiro" recebido aqui ainda tem Id = 0 (default de int). Por isso criamos
    // um Usuario novo a partir dos campos do barbeiro, inserimos, e devolvemos um
    // Barbeiro reconstruído com o Id real que o Postgres gerou — é esse retorno que
    // o Service usa para montar o BarbeiroDto corretamente.
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
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro,
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
                u => u.Id == id && u.TipoUsuario!.Descricao == TipoUsuarioDescricoes.Barbeiro,
                cancellationToken);

        if (usuario is null)
            return false;

        context.Usuarios.Remove(usuario);
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

        return barbeiro;
    }
}
