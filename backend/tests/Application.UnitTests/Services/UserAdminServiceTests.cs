using Application.Commands.Users;
using Application.Common;
using Application.DTOs.Users;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validation;
using Application.Services;
using Application.UnitTests.TestHelpers;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.Services;

public sealed class UserAdminServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IRolRepository> _roles = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateUserCommand>> _createValidator = new();
    private readonly Mock<IValidator<UpdateUserRolesCommand>> _updateRolesValidator = new();
    private readonly UserAdminService _sut;

    public UserAdminServiceTests()
    {
        _createValidator
            .Setup(validator => validator.Validate(It.IsAny<CreateUserCommand>()))
            .Returns(ValidationResult.Success());
        _updateRolesValidator
            .Setup(validator => validator.Validate(It.IsAny<UpdateUserRolesCommand>()))
            .Returns(ValidationResult.Success());

        _sut = new UserAdminService(
            _usuarios.Object,
            _roles.Object,
            _passwordHasher.Object,
            _unitOfWork.Object,
            _createValidator.Object,
            _updateRolesValidator.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenRoleDoesNotExist_ShouldReturnRoleError()
    {
        _roles
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Rol("COUNTRY").WithId(1)]);

        var result = await _sut.CreateAsync(new CreateUserDto { Email = "new@example.com", Password = "secret", RoleIds = [1, 2] }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("One or more roles were not found.", result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WhenValidatorFails_ShouldReturnValidationErrors()
    {
        _createValidator
            .Setup(validator => validator.Validate(It.IsAny<CreateUserCommand>()))
            .Returns(ValidationResult.Failure(["Email is required."]));

        var result = await _sut.CreateAsync(new CreateUserDto(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Email is required.", result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailExists_ShouldReturnDuplicateEmail()
    {
        _roles
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Rol("COUNTRY").WithId(1)]);
        _usuarios
            .Setup(repository => repository.ExistsByEmailAsync("new@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(new CreateUserDto { Email = "new@example.com", Password = "secret", RoleIds = [1] }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("A user with the same email already exists.", result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsValid_ShouldCreateUserWithRoles()
    {
        var role = new Rol("COUNTRY").WithId(1);
        _roles
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([role]);
        _passwordHasher.Setup(hasher => hasher.GenerateSalt()).Returns("salt");
        _passwordHasher.Setup(hasher => hasher.HashPassword("secret", "salt")).Returns("hash");

        var result = await _sut.CreateAsync(new CreateUserDto { Email = "NEW@example.com", Password = "secret", RoleIds = [1] }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("new@example.com", result.Value!.Email);
        Assert.Contains("COUNTRY", result.Value.Roles);
        _usuarios.Verify(repository => repository.AddAsync(It.Is<Usuario>(user => user.Email == "new@example.com"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMeAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        _usuarios
            .Setup(repository => repository.GetByIdWithRolesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var result = await _sut.GetMeAsync(1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("User was not found.", result.Errors);
    }

    [Fact]
    public async Task GetMeAsync_WhenUserIsInactive_ShouldReturnNotFound()
    {
        var user = new Usuario("new@example.com", "hash", "salt").WithId(1);
        user.Deactivate();
        _usuarios
            .Setup(repository => repository.GetByIdWithRolesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetMeAsync(1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("User was not found.", result.Errors);
    }

    [Fact]
    public async Task DeactivateAsync_WhenUserExists_ShouldDeactivateUser()
    {
        var user = new Usuario("new@example.com", "hash", "salt").WithId(1);
        _usuarios
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.DeactivateAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(user.Activo);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        _usuarios
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var result = await _sut.DeactivateAsync(1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("User was not found.", result.Errors);
    }

    [Fact]
    public async Task ChangeRolesAsync_WhenValidatorFails_ShouldReturnValidationErrors()
    {
        _updateRolesValidator
            .Setup(validator => validator.Validate(It.IsAny<UpdateUserRolesCommand>()))
            .Returns(ValidationResult.Failure(["At least one RoleId is required."]));

        var result = await _sut.ChangeRolesAsync(1, new UpdateUserRolesDto(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("At least one RoleId is required.", result.Errors);
    }

    [Fact]
    public async Task ChangeRolesAsync_WhenRoleDoesNotExist_ShouldReturnRoleError()
    {
        _roles
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.ChangeRolesAsync(1, new UpdateUserRolesDto { RoleIds = [3] }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("One or more roles were not found.", result.Errors);
    }

    [Fact]
    public async Task ChangeRolesAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        var newRole = new Rol("USER_ADMIN").WithId(3);
        _roles
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([newRole]);
        _usuarios
            .Setup(repository => repository.GetByIdWithRolesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var result = await _sut.ChangeRolesAsync(1, new UpdateUserRolesDto { RoleIds = [3] }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("User was not found.", result.Errors);
    }

    [Fact]
    public async Task ChangeRolesAsync_WhenUserExists_ShouldReplaceRolesAndClearRefreshToken()
    {
        var user = new Usuario("new@example.com", "hash", "salt").WithId(1);
        user.AssignRoles([new Rol("COUNTRY").WithId(1)]);
        user.SetRefreshToken("refresh", DateTime.UtcNow.AddDays(1));
        var newRole = new Rol("USER_ADMIN").WithId(3);
        _roles
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([newRole]);
        _usuarios
            .Setup(repository => repository.GetByIdWithRolesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.ChangeRolesAsync(1, new UpdateUserRolesDto { RoleIds = [3] }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("USER_ADMIN", result.Value!.Roles);
        Assert.DoesNotContain("COUNTRY", result.Value.Roles);
        Assert.Null(user.RefreshToken);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
