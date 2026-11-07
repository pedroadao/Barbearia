using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barbearia.Infrastructure.Mappings;

public sealed class StatusMapping : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable("status");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasData(
            new { Id = StatusIds.Ativo, Descricao = "Ativo" },
            new { Id = StatusIds.Excluido, Descricao = "Excluído" });
    }
}
