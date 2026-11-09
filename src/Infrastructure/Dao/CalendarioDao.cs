using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao.Interface;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Dao;

public sealed class CalendarioDao(AppDbContext context) : ICalendarioDao
{
    public Task<Calendario?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Calendarios.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.IdStatus == StatusIds.Ativo, cancellationToken);

    public async Task<IReadOnlyList<Calendario>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Calendarios.AsNoTracking()
            .Where(c => c.IdStatus == StatusIds.Ativo)
            .ToListAsync(cancellationToken);

    public async Task<Calendario> AddAsync(Calendario calendario, CancellationToken cancellationToken)
    {
        await context.Calendarios.AddAsync(calendario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return calendario;
    }

    public Task<Calendario?> ObterPorHorarioAsync(
        int idBarbeiro, DateOnly data, TimeOnly horaInicio, CancellationToken cancellationToken) =>
        context.Calendarios.AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.IdBarbeiro == idBarbeiro && c.Data == data && c.HoraInicio == horaInicio && c.IdStatus == StatusIds.Ativo,
                cancellationToken);

    public async Task<bool> ReservarClienteAvulsoAsync(int id, string nomeClienteAvulso, CancellationToken cancellationToken)
    {
        var calendario = await context.Calendarios
            .FirstOrDefaultAsync(c => c.Id == id && c.IdStatus == StatusIds.Ativo, cancellationToken);

        if (calendario is null)
            return false;

        calendario.ReservarParaClienteAvulso(nomeClienteAvulso);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public Task<bool> ExisteConflitoAsync(
        int idBarbeiro,
        DateOnly data,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        int? idParaIgnorar,
        CancellationToken cancellationToken) =>
        context.Calendarios.AsNoTracking().AnyAsync(
            c => c.IdBarbeiro == idBarbeiro
                && c.Data == data
                && c.Id != idParaIgnorar
                && c.IdStatus == StatusIds.Ativo
                && c.HoraInicio < horaFim
                && horaInicio < c.HoraFim,
            cancellationToken);

    public async Task<bool> UpdateAsync(
        int id,
        int idBarbeiro,
        DateOnly data,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        int? idCliente,
        bool disponivel,
        CancellationToken cancellationToken)
    {
        var linhasAfetadas = await context.Calendarios
            .Where(c => c.Id == id && c.IdStatus == StatusIds.Ativo)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(c => c.IdBarbeiro, idBarbeiro)
                    .SetProperty(c => c.Data, data)
                    .SetProperty(c => c.HoraInicio, horaInicio)
                    .SetProperty(c => c.HoraFim, horaFim)
                    .SetProperty(c => c.IdCliente, idCliente)
                    .SetProperty(c => c.Disponivel, disponivel),
                cancellationToken);

        return linhasAfetadas > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var calendario = await context.Calendarios
            .FirstOrDefaultAsync(c => c.Id == id && c.IdStatus == StatusIds.Ativo, cancellationToken);

        if (calendario is null)
            return false;

        calendario.MarcarComoExcluido();
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<Calendario>> ObterFilaAsync(int idBarbeiro, DateOnly data, CancellationToken cancellationToken) =>
        await context.Calendarios.AsNoTracking()
            .Where(c => c.IdBarbeiro == idBarbeiro && c.Data == data && !c.Disponivel && c.IdStatus == StatusIds.Ativo)
            .OrderBy(c => c.HoraInicio)
            .ToListAsync(cancellationToken);

    public async Task<bool> ReordenarFilaAsync(
        int idBarbeiro,
        DateOnly data,
        IReadOnlyList<int> idsEmOrdem,
        CancellationToken cancellationToken)
    {
        var entradas = await context.Calendarios
            .Where(c => c.IdBarbeiro == idBarbeiro && c.Data == data && !c.Disponivel && c.IdStatus == StatusIds.Ativo)
            .OrderBy(c => c.HoraInicio)
            .ToListAsync(cancellationToken);

        if (entradas.Count != idsEmOrdem.Count ||
            !entradas.Select(e => e.Id).OrderBy(id => id).SequenceEqual(idsEmOrdem.OrderBy(id => id)))
            return false;

        var porId = entradas.ToDictionary(e => e.Id);
        var clientesEmOrdem = idsEmOrdem
            .Select(id => (porId[id].IdCliente, porId[id].NomeClienteAvulso))
            .ToList();

        for (var i = 0; i < entradas.Count; i++)
        {
            var (idCliente, nomeClienteAvulso) = clientesEmOrdem[i];
            entradas[i].TransferirCliente(idCliente, nomeClienteAvulso);
        }

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
