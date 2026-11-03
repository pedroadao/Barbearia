using Barbearia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barbearia.Infrastructure.Mappings;

public sealed class TipoUsuarioMapping : IEntityTypeConfiguration<TipoUsuario>
{
    public void Configure(EntityTypeBuilder<TipoUsuario> builder)
    {
        builder.ToTable("tipo_usuario");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.IdStatus)
            .HasColumnName("id_status");

        builder.HasOne(t => t.Status)
            .WithMany()
            .HasForeignKey(t => t.IdStatus)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
