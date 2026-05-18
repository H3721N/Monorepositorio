using Application.Commands.Countries;
using Application.Common;
using Application.DTOs.Countries;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validation;
using Application.Services;
using Application.UnitTests.TestHelpers;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.Services;

public sealed class CountryServiceTests
{
    private readonly Mock<ICountryRepository> _countries = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateCountryCommand>> _createValidator = new();
    private readonly Mock<IValidator<UpdateCountryCommand>> _updateValidator = new();
    private readonly CountryService _sut;

    public CountryServiceTests()
    {
        _createValidator
            .Setup(validator => validator.Validate(It.IsAny<CreateCountryCommand>()))
            .Returns(ValidationResult.Success());
        _updateValidator
            .Setup(validator => validator.Validate(It.IsAny<UpdateCountryCommand>()))
            .Returns(ValidationResult.Success());

        _sut = new CountryService(_countries.Object, _unitOfWork.Object, _createValidator.Object, _updateValidator.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenCountriesExist_ShouldReturnDtos()
    {
        _countries
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Country("Guatemala", "GT").WithId(7)]);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        var country = Assert.Single(result);
        Assert.Equal(7, country.Id);
        Assert.Equal("Guatemala", country.Name);
        Assert.Equal("GT", country.IsoCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCountryDoesNotExist_ShouldReturnNotFound()
    {
        _countries
            .Setup(repository => repository.GetByIdWithDepartmentsAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        var result = await _sut.GetByIdAsync(9, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Country was not found.", result.Errors);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCountryExists_ShouldReturnDetailWithoutDeletedDepartments()
    {
        var country = new Country("Guatemala", "GT").WithId(1);
        var active = new Department("Guatemala", 1).WithId(2);
        var deleted = new Department("Deleted", 1).WithId(3);
        deleted.SoftDelete();
        var departmentsField = typeof(Country).GetField("_departments", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        departmentsField.SetValue(country, new List<Department> { active, deleted });
        _countries
            .Setup(repository => repository.GetByIdWithDepartmentsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        var result = await _sut.GetByIdAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var department = Assert.Single(result.Value!.Departments);
        Assert.Equal("Guatemala", department.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenValidatorFails_ShouldReturnValidationErrors()
    {
        _createValidator
            .Setup(validator => validator.Validate(It.IsAny<CreateCountryCommand>()))
            .Returns(ValidationResult.Failure(["Name is required."]));

        var result = await _sut.CreateAsync(new CreateCountryDto { Name = "", IsoCode = "GT" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Name is required.", result.Errors);
        _countries.Verify(repository => repository.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCountryIsDuplicate_ShouldReturnDuplicateErrors()
    {
        _countries
            .Setup(repository => repository.ExistsByNameAsync("Guatemala", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _countries
            .Setup(repository => repository.ExistsByIsoCodeAsync("GT", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(new CreateCountryDto { Name = " Guatemala ", IsoCode = "gt" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("A country with the same name already exists.", result.Errors);
        Assert.Contains("A country with the same IsoCode already exists.", result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WhenCommandIsValid_ShouldPersistAndReturnDto()
    {
        Country? addedCountry = null;
        _countries
            .Setup(repository => repository.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()))
            .Callback<Country, CancellationToken>((country, _) => addedCountry = country)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(new CreateCountryDto { Name = " Guatemala ", IsoCode = "gt" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Guatemala", result.Value!.Name);
        Assert.Equal("GT", result.Value.IsoCode);
        Assert.NotNull(addedCountry);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCountryDoesNotExist_ShouldReturnNotFound()
    {
        _countries
            .Setup(repository => repository.GetByIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        var result = await _sut.UpdateAsync(9, new UpdateCountryDto { Name = "Guatemala", IsoCode = "GT" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Country was not found.", result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidatorFails_ShouldReturnValidationErrors()
    {
        _updateValidator
            .Setup(validator => validator.Validate(It.IsAny<UpdateCountryCommand>()))
            .Returns(ValidationResult.Failure(["Id must be greater than zero."]));

        var result = await _sut.UpdateAsync(0, new UpdateCountryDto(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Id must be greater than zero.", result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateExists_ShouldReturnDuplicateErrors()
    {
        var country = new Country("Old", "OL").WithId(2);
        _countries
            .Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);
        _countries
            .Setup(repository => repository.ExistsByNameAsync("New", 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateAsync(2, new UpdateCountryDto { Name = "New", IsoCode = "NW" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("A country with the same name already exists.", result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_WhenCommandIsValid_ShouldUpdateCountry()
    {
        var country = new Country("Old", "OL").WithId(2);
        _countries
            .Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        var result = await _sut.UpdateAsync(2, new UpdateCountryDto { Name = "New", IsoCode = "NW" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", result.Value!.Name);
        Assert.Equal("NW", result.Value.IsoCode);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCountryExists_ShouldSoftDeleteCountryAndDepartments()
    {
        var country = new Country("Guatemala", "GT").WithId(1);
        _countries
            .Setup(repository => repository.GetByIdWithDepartmentsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        var result = await _sut.DeleteAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(country.IsDeleted);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCountryDoesNotExist_ShouldReturnNotFound()
    {
        _countries
            .Setup(repository => repository.GetByIdWithDepartmentsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        var result = await _sut.DeleteAsync(1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Country was not found.", result.Errors);
    }
}
