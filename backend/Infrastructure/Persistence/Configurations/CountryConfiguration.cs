using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(country => country.Id);

        builder.Property(country => country.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(country => country.IsoCode)
            .IsRequired()
            .HasMaxLength(2)
            .IsFixedLength();

        builder.HasIndex(country => country.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(country => country.IsoCode)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasMany(country => country.Departments)
            .WithOne(department => department.Country)
            .HasForeignKey(department => department.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(country => country.Departments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(country => !country.IsDeleted);
    }
}
