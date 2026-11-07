using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Dao;
using Barbearia.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Tests.Dao;

public sealed class SoftDeleteTests
{
    [Fact]
    public async Task Usuario_NovaLinha_EntraComoAtivo()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var dao = new BarbeiroDao(context);

        var barbeiro = await dao.AddAsync(
            new Barbeiro("João", "joao@barbearia.com", "11999999999", null, new DateOnly(1990, 1, 1), InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro),
            CancellationToken.None);

        var usuario = await context.Usuarios.AsNoTracking().FirstAsync(u => u.Id == barbeiro.Id, CancellationToken.None);
        usuario.IdStatus.Should().Be(StatusIds.Ativo);
    }

    [Fact]
    public async Task BarbeiroDao_Delete_MarcaStatusExcluidoSemRemoverALinha()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var dao = new BarbeiroDao(context);

        var barbeiro = await dao.AddAsync(
            new Barbeiro("João", "joao@barbearia.com", "11999999999", null, new DateOnly(1990, 1, 1), InMemoryAppDbContextFactory.IdTipoUsuarioBarbeiro),
            CancellationToken.None);

        var removido = await dao.DeleteAsync(barbeiro.Id, CancellationToken.None);

        removido.Should().BeTrue();

        var usuario = await context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == barbeiro.Id, CancellationToken.None);
        usuario.Should().NotBeNull();
        usuario!.IdStatus.Should().Be(StatusIds.Excluido);

        (await dao.GetByIdAsync(barbeiro.Id, CancellationToken.None)).Should().BeNull();
        (await dao.GetAllAsync(CancellationToken.None)).Should().NotContain(b => b.Id == barbeiro.Id);
    }

    [Fact]
    public async Task CalendarioDao_Delete_MarcaStatusExcluidoSemRemoverALinha()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var dao = new CalendarioDao(context);

        var calendario = await dao.AddAsync(
            new Calendario(idBarbeiro: 1, new DateOnly(2026, 11, 10), new TimeOnly(9, 0), new TimeOnly(9, 30), idCliente: null, disponivel: true),
            CancellationToken.None);

        var removido = await dao.DeleteAsync(calendario.Id, CancellationToken.None);

        removido.Should().BeTrue();

        var registro = await context.Calendarios.AsNoTracking().FirstOrDefaultAsync(c => c.Id == calendario.Id, CancellationToken.None);
        registro.Should().NotBeNull();
        registro!.IdStatus.Should().Be(StatusIds.Excluido);

        (await dao.GetByIdAsync(calendario.Id, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task CalendarioDao_ExisteConflitoAsync_IgnoraHorariosExcluidos()
    {
        var context = InMemoryAppDbContextFactory.Criar();
        var dao = new CalendarioDao(context);

        var calendario = await dao.AddAsync(
            new Calendario(idBarbeiro: 1, new DateOnly(2026, 11, 10), new TimeOnly(9, 0), new TimeOnly(10, 0), idCliente: null, disponivel: true),
            CancellationToken.None);
        await dao.DeleteAsync(calendario.Id, CancellationToken.None);

        var existeConflito = await dao.ExisteConflitoAsync(
            idBarbeiro: 1,
            new DateOnly(2026, 11, 10),
            new TimeOnly(9, 0),
            new TimeOnly(10, 0),
            idParaIgnorar: null,
            CancellationToken.None);

        existeConflito.Should().BeFalse();
    }
}
