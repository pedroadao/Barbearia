using Barbearia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Status> Status => Set<Status>();

    public DbSet<TipoUsuario> TiposUsuario => Set<TipoUsuario>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Calendario> Calendarios => Set<Calendario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
