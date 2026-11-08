using Barbearia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barbearia.Infrastructure.Mappings;

public sealed class LogAlteracaoMapping : IEntityTypeConfiguration<LogAlteracao>
{
    public void Configure(EntityTypeBuilder<LogAlteracao> builder)
    {
        builder.ToTable("log_alteracao");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.Property(l => l.DataAlteracao)
            .HasColumnName("data_alteracao")
            .IsRequired();

        builder.Property(l => l.Dados)
            .HasColumnName("dados")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(l => l.IdUsuario)
            .HasColumnName("id_usuario")
            .IsRequired();

        builder.Property(l => l.Metodo)
            .HasColumnName("metodo")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.Tabela)
            .HasColumnName("tabela")
            .HasMaxLength(100)
            .IsRequired();
    }
}
