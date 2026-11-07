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

        // Ids devem coincidir com StatusIds.Ativo/Excluido (seedados via migration em produção).
        var statusAtivo = new Status("Ativo");
        var statusExcluido = new Status("Excluído");
        context.Status.AddRange(statusAtivo, statusExcluido);
        context.SaveChanges();

        context.TiposUsuario.AddRange(
            new TipoUsuario(TipoUsuarioDescricoes.Barbeiro, statusAtivo.Id),
            new TipoUsuario(TipoUsuarioDescricoes.Cliente, statusAtivo.Id));
        context.SaveChanges();

        return context;
    }
}
