using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(usuario => usuario.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(usuario => usuario.Salt)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(usuario => usuario.RefreshToken)
            .HasMaxLength(256);

        builder.Property(usuario => usuario.Activo)
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique();

        builder.Navigation(usuario => usuario.Roles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
