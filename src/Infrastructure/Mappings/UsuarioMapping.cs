using Barbearia.Domain.Common;
using Barbearia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barbearia.Infrastructure.Mappings;

public sealed class UsuarioMapping : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.Instagram)
            .HasColumnName("instagram")
            .HasMaxLength(50);

        builder.Property(u => u.DataNascimento)
            .HasColumnName("data_nascimento")
            .IsRequired();

        builder.Property(u => u.IdTipoUsuario)
            .HasColumnName("id_tipo_usuario");

        builder.Property(u => u.AuthUserId)
            .HasColumnName("auth_user_id")
            .HasMaxLength(100);

        builder.Property(u => u.MostrarCadastroSempre)
            .HasColumnName("mostrar_cadastro_sempre")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.IdStatus)
            .HasColumnName("id_status")
            .HasDefaultValue(StatusIds.Ativo)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.AuthUserId)
            .IsUnique()
            .HasFilter("auth_user_id IS NOT NULL");

        builder.HasOne(u => u.TipoUsuario)
            .WithMany()
            .HasForeignKey(u => u.IdTipoUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Status)
            .WithMany()
            .HasForeignKey(u => u.IdStatus)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
