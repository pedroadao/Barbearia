using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barbearia.Infrastructure.Mappings;

public sealed class CalendarioMapping : IEntityTypeConfiguration<Calendario>
{
    public void Configure(EntityTypeBuilder<Calendario> builder)
    {
        builder.ToTable("calendario");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.IdBarbeiro)
            .HasColumnName("id_barbeiro");

        builder.Property(c => c.Data)
            .HasColumnName("data")
            .IsRequired();

        builder.Property(c => c.HoraInicio)
            .HasColumnName("hora_inicio")
            .IsRequired();

        builder.Property(c => c.HoraFim)
            .HasColumnName("hora_fim")
            .IsRequired();

        builder.Property(c => c.IdCliente)
            .HasColumnName("id_cliente");

        builder.Property(c => c.NomeClienteAvulso)
            .HasColumnName("nome_cliente_avulso")
            .HasMaxLength(200);

        builder.Property(c => c.Disponivel)
            .HasColumnName("disponivel")
            .IsRequired();

        builder.Property(c => c.IdStatus)
            .HasColumnName("id_status")
            .HasDefaultValue(StatusIds.Ativo)
            .IsRequired();

        builder.HasIndex(c => new { c.IdBarbeiro, c.Data, c.HoraInicio })
            .IsUnique()
            .HasFilter($"id_status = {StatusIds.Ativo}");

        builder.HasOne(c => c.Barbeiro)
            .WithMany()
            .HasForeignKey(c => c.IdBarbeiro)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Status)
            .WithMany()
            .HasForeignKey(c => c.IdStatus)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
