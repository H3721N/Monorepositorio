using Application.Commands.Auth;
using Application.Commands.Countries;
using Application.Commands.Departments;
using Application.Commands.Users;
using Application.Validators.Auth;
using Application.Validators.Countries;
using Application.Validators.Departments;
using Application.Validators.Users;

namespace Application.UnitTests.Validators;

public sealed class ValidatorTests
{
    [Fact]
    public void CreateCountryCommandValidator_WhenCommandIsValid_ShouldPass()
    {
        var result = new CreateCountryCommandValidator().Validate(new CreateCountryCommand("Guatemala", "gt"));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CreateCountryCommandValidator_WhenFieldsAreInvalid_ShouldReturnErrors()
    {
        var result = new CreateCountryCommandValidator().Validate(new CreateCountryCommand("", "GTM"));

        Assert.False(result.IsValid);
        Assert.Contains("Name is required.", result.Errors);
        Assert.Contains("IsoCode must contain exactly 2 characters.", result.Errors);
    }

    [Fact]
    public void UpdateCountryCommandValidator_WhenIdAndFieldsAreInvalid_ShouldReturnErrors()
    {
        var result = new UpdateCountryCommandValidator().Validate(new UpdateCountryCommand(0, new string('x', 101), ""));

        Assert.False(result.IsValid);
        Assert.Contains("Id must be greater than zero.", result.Errors);
        Assert.Contains("Name cannot exceed 100 characters.", result.Errors);
        Assert.Contains("IsoCode is required.", result.Errors);
    }

    [Fact]
    public void CreateDepartmentCommandValidator_WhenCommandIsValid_ShouldPass()
    {
        var result = new CreateDepartmentCommandValidator().Validate(new CreateDepartmentCommand("Alta Verapaz", 1));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateDepartmentCommandValidator_WhenFieldsAreInvalid_ShouldReturnErrors()
    {
        var result = new CreateDepartmentCommandValidator().Validate(new CreateDepartmentCommand("", 0));

        Assert.False(result.IsValid);
        Assert.Contains("Name is required.", result.Errors);
        Assert.Contains("CountryId must be greater than zero.", result.Errors);
    }

    [Fact]
    public void UpdateDepartmentCommandValidator_WhenIdNameAndCountryAreInvalid_ShouldReturnErrors()
    {
        var result = new UpdateDepartmentCommandValidator().Validate(new UpdateDepartmentCommand(0, new string('x', 101), -1));

        Assert.False(result.IsValid);
        Assert.Contains("Id must be greater than zero.", result.Errors);
        Assert.Contains("Name cannot exceed 100 characters.", result.Errors);
        Assert.Contains("CountryId must be greater than zero.", result.Errors);
    }

    [Fact]
    public void LoginCommandValidator_WhenCredentialsAreMissing_ShouldReturnErrors()
    {
        var result = new LoginCommandValidator().Validate(new LoginCommand("", ""));

        Assert.False(result.IsValid);
        Assert.Contains("Email is required.", result.Errors);
        Assert.Contains("Password is required.", result.Errors);
    }

    [Fact]
    public void ChangePasswordCommandValidator_WhenCommandIsInvalid_ShouldReturnErrors()
    {
        var result = new ChangePasswordCommandValidator().Validate(new ChangePasswordCommand(0, "", ""));

        Assert.False(result.IsValid);
        Assert.Contains("UserId must be greater than zero.", result.Errors);
        Assert.Contains("CurrentPassword is required.", result.Errors);
        Assert.Contains("NewPassword is required.", result.Errors);
    }

    [Fact]
    public void ChangePasswordCommandValidator_WhenNewPasswordIsTooShort_ShouldReturnLengthError()
    {
        var result = new ChangePasswordCommandValidator().Validate(new ChangePasswordCommand(1, "current", "short"));

        Assert.False(result.IsValid);
        Assert.Contains("NewPassword must contain at least 8 characters.", result.Errors);
    }

    [Fact]
    public void CreateUserCommandValidator_WhenRolesAreMissingOrInvalid_ShouldReturnErrors()
    {
        var result = new CreateUserCommandValidator().Validate(new CreateUserCommand("", "", [0]));

        Assert.False(result.IsValid);
        Assert.Contains("Email is required.", result.Errors);
        Assert.Contains("Password is required.", result.Errors);
        Assert.Contains("Every RoleId must be greater than zero.", result.Errors);
    }

    [Fact]
    public void CreateUserCommandValidator_WhenRoleIdsAreEmpty_ShouldReturnMissingRoleError()
    {
        var result = new CreateUserCommandValidator().Validate(new CreateUserCommand("user@example.com", "password", []));

        Assert.False(result.IsValid);
        Assert.Contains("At least one RoleId is required.", result.Errors);
    }

    [Fact]
    public void UpdateUserRolesCommandValidator_WhenUserAndRolesAreInvalid_ShouldReturnErrors()
    {
        var result = new UpdateUserRolesCommandValidator().Validate(new UpdateUserRolesCommand(0, []));

        Assert.False(result.IsValid);
        Assert.Contains("UserId must be greater than zero.", result.Errors);
        Assert.Contains("At least one RoleId is required.", result.Errors);
    }

    [Fact]
    public void UpdateUserRolesCommandValidator_WhenRoleIdIsInvalid_ShouldReturnInvalidRoleError()
    {
        var result = new UpdateUserRolesCommandValidator().Validate(new UpdateUserRolesCommand(1, [0]));

        Assert.False(result.IsValid);
        Assert.Contains("Every RoleId must be greater than zero.", result.Errors);
    }
}
