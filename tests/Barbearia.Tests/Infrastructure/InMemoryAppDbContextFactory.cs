using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Tests.Infrastructure;

internal static class InMemoryAppDbContextFactory
{
    public const int IdTipoUsuarioBarbeiro = 1;
    public const int IdTipoUsuarioCliente = 2;

    public static AppDbContext Criar()
    {
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(opcoes);

        var status = new Status("Ativo");
        context.Status.Add(status);
        context.SaveChanges();

        context.TiposUsuario.AddRange(
            new TipoUsuario(TipoUsuarioDescricoes.Barbeiro, status.Id),
            new TipoUsuario(TipoUsuarioDescricoes.Cliente, status.Id));
        context.SaveChanges();

        return context;
    }
}
