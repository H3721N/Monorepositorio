using Application.Commands.Departments;
using Application.Common;
using Application.DTOs.Departments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validation;
using Application.Services;
using Application.UnitTests.TestHelpers;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.Services;

public sealed class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _departments = new();
    private readonly Mock<ICountryRepository> _countries = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateDepartmentCommand>> _createValidator = new();
    private readonly Mock<IValidator<UpdateDepartmentCommand>> _updateValidator = new();
    private readonly DepartmentService _sut;

    public DepartmentServiceTests()
    {
        _createValidator
            .Setup(validator => validator.Validate(It.IsAny<CreateDepartmentCommand>()))
            .Returns(ValidationResult.Success());
        _updateValidator
            .Setup(validator => validator.Validate(It.IsAny<UpdateDepartmentCommand>()))
            .Returns(ValidationResult.Success());

        _sut = new DepartmentService(
            _departments.Object,
            _countries.Object,
            _unitOfWork.Object,
            _createValidator.Object,
            _updateValidator.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenCountryDoesNotExist_ShouldReturnRelationError()
    {
        _countries
            .Setup(repository => repository.ExistsByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.CreateAsync(new CreateDepartmentDto { Name = "Guatemala", CountryId = 3 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Country was not found.", result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WhenValidatorFails_ShouldReturnValidationErrors()
    {
        _createValidator
            .Setup(validator => validator.Validate(It.IsAny<CreateDepartmentCommand>()))
            .Returns(ValidationResult.Failure(["Name is required."]));

        var result = await _sut.CreateAsync(new CreateDepartmentDto { Name = "", CountryId = 1 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Name is required.", result.Errors);
        _departments.Verify(repository => repository.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenDepartmentIsDuplicateForCountry_ShouldReturnDuplicateError()
    {
        _countries
            .Setup(repository => repository.ExistsByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _departments
            .Setup(repository => repository.ExistsByNameForCountryAsync("Guatemala", 3, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(new CreateDepartmentDto { Name = " Guatemala ", CountryId = 3 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("A department with the same name already exists for this country.", result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WhenCommandIsValid_ShouldPersistDepartment()
    {
        _countries
            .Setup(repository => repository.ExistsByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(new CreateDepartmentDto { Name = " Guatemala ", CountryId = 3 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Guatemala", result.Value!.Name);
        Assert.Equal(3, result.Value.CountryId);
        _departments.Verify(repository => repository.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenDepartmentDoesNotExist_ShouldReturnNotFound()
    {
        _departments
            .Setup(repository => repository.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var result = await _sut.UpdateAsync(4, new UpdateDepartmentDto { Name = "Guatemala", CountryId = 3 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Department was not found.", result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidatorFails_ShouldReturnValidationErrors()
    {
        _updateValidator
            .Setup(validator => validator.Validate(It.IsAny<UpdateDepartmentCommand>()))
            .Returns(ValidationResult.Failure(["Id must be greater than zero."]));

        var result = await _sut.UpdateAsync(0, new UpdateDepartmentDto { Name = "", CountryId = 0 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Id must be greater than zero.", result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_WhenRelationValidationFails_ShouldReturnRelationErrors()
    {
        var department = new Department("Guatemala", 1).WithId(4);
        _departments
            .Setup(repository => repository.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);
        _countries
            .Setup(repository => repository.ExistsByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _departments
            .Setup(repository => repository.ExistsByNameForCountryAsync("Guatemala", 2, 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateAsync(4, new UpdateDepartmentDto { Name = "Guatemala", CountryId = 2 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("A department with the same name already exists for this country.", result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_WhenCommandIsValid_ShouldUpdateDepartment()
    {
        var department = new Department("Old", 1).WithId(4);
        _departments
            .Setup(repository => repository.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);
        _countries
            .Setup(repository => repository.ExistsByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateAsync(4, new UpdateDepartmentDto { Name = "New", CountryId = 2 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", result.Value!.Name);
        Assert.Equal(2, result.Value.CountryId);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenDepartmentDoesNotExist_ShouldReturnNotFound()
    {
        _departments
            .Setup(repository => repository.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var result = await _sut.DeleteAsync(5, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Department was not found.", result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_WhenDepartmentExists_ShouldSoftDeleteIt()
    {
        var department = new Department("Guatemala", 1).WithId(4);
        _departments
            .Setup(repository => repository.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        var result = await _sut.DeleteAsync(4, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(department.IsDeleted);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByCountryIdAsync_WhenDepartmentsExist_ShouldReturnDtos()
    {
        _departments
            .Setup(repository => repository.GetByCountryIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Department("Guatemala", 1).WithId(2)]);

        var result = await _sut.GetByCountryIdAsync(1, CancellationToken.None);

        var department = Assert.Single(result);
        Assert.Equal(2, department.Id);
        Assert.Equal("Guatemala", department.Name);
    }
}
