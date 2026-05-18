using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(rol => rol.Id);

        builder.Property(rol => rol.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(rol => rol.Nombre)
            .IsUnique();

        builder.HasMany(rol => rol.Usuarios)
            .WithMany(usuario => usuario.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "UsuarioRoles",
                right => right
                    .HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey("UsuarioId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Rol>()
                    .WithMany()
                    .HasForeignKey("RolId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("UsuarioRoles");
                    join.HasKey("UsuarioId", "RolId");
                });

        builder.Navigation(rol => rol.Usuarios)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
