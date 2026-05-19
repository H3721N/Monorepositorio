using Domain.Entities;

namespace Application.UnitTests.Domain;

public sealed class DomainEntityTests
{
    [Fact]
    public void Country_WhenNameIsTooLong_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Country(new string('x', 101), "GT"));

        Assert.Contains("Country name cannot exceed 100 characters.", exception.Message);
    }

    [Fact]
    public void Country_WhenIsoCodeIsInvalid_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Country("Guatemala", "GTM"));

        Assert.Contains("Country ISO code must contain exactly 2 characters.", exception.Message);
    }

    [Fact]
    public void Department_WhenNameIsTooLong_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Department(new string('x', 101), 1));

        Assert.Contains("Department name cannot exceed 100 characters.", exception.Message);
    }

    [Fact]
    public void Department_WhenCountryIdIsInvalid_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Department("Guatemala", 0));

        Assert.Contains("CountryId must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Rol_WhenNombreIsTooLong_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Rol(new string('x', 51)));

        Assert.Contains("Role name cannot exceed 50 characters.", exception.Message);
    }

    [Fact]
    public void Usuario_WhenEmailIsTooLong_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Usuario(new string('x', 257), "hash", "salt"));

        Assert.Contains("Email cannot exceed 256 characters.", exception.Message);
    }

    [Fact]
    public void Usuario_AssignRoles_WhenRolesAreEmpty_ShouldThrow()
    {
        var usuario = new Usuario("admin@example.com", "hash", "salt");

        var exception = Assert.Throws<ArgumentException>(() => usuario.AssignRoles([]));

        Assert.Contains("At least one role is required.", exception.Message);
    }

    [Fact]
    public void BaseEntity_SoftDelete_WhenCalledTwice_ShouldKeepFirstDeletedTimestamp()
    {
        var country = new Country("Guatemala", "GT");

        country.SoftDelete();
        var firstDeletedAt = country.DeletedAtUtc;
        country.SoftDelete();

        Assert.True(country.IsDeleted);
        Assert.Equal(firstDeletedAt, country.DeletedAtUtc);
    }
}
