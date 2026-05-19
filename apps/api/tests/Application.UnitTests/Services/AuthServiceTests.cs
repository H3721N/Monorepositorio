using Application.Commands.Auth;
using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validation;
using Application.Services;
using Application.UnitTests.TestHelpers;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.Services;

public sealed class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<LoginCommand>> _loginValidator = new();
    private readonly Mock<IValidator<ChangePasswordCommand>> _changePasswordValidator = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _loginValidator
            .Setup(validator => validator.Validate(It.IsAny<LoginCommand>()))
            .Returns(ValidationResult.Success());
        _changePasswordValidator
            .Setup(validator => validator.Validate(It.IsAny<ChangePasswordCommand>()))
            .Returns(ValidationResult.Success());

        _sut = new AuthService(
            _usuarios.Object,
            _passwordHasher.Object,
            _tokenService.Object,
            _unitOfWork.Object,
            _loginValidator.Object,
            _changePasswordValidator.Object);
    }

    [Fact]
    public async Task LoginAsync_WhenValidatorFails_ShouldReturnErrors()
    {
        _loginValidator
            .Setup(validator => validator.Validate(It.IsAny<LoginCommand>()))
            .Returns(ValidationResult.Failure(["Email is required."]));

        var result = await _sut.LoginAsync(new LoginDto { Email = "", Password = "" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Email is required.", result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldReturnInvalidCredentials()
    {
        _usuarios
            .Setup(repository => repository.GetByEmailWithRolesAsync("admin@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var result = await _sut.LoginAsync(new LoginDto { Email = "admin@example.com", Password = "secret" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid credentials.", result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ShouldReturnInvalidCredentials()
    {
        var user = new Usuario("admin@example.com", "hash", "salt").WithId(1);
        user.Deactivate();
        _usuarios
            .Setup(repository => repository.GetByEmailWithRolesAsync("admin@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginDto { Email = "admin@example.com", Password = "secret" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid credentials.", result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ShouldReturnInvalidCredentials()
    {
        var user = new Usuario("admin@example.com", "hash", "salt").WithId(1);
        _usuarios
            .Setup(repository => repository.GetByEmailWithRolesAsync("admin@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(hasher => hasher.VerifyPassword("bad", "salt", "hash"))
            .Returns(false);

        var result = await _sut.LoginAsync(new LoginDto { Email = "admin@example.com", Password = "bad" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid credentials.", result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnTokensAndPersistRefreshToken()
    {
        var user = new Usuario("admin@example.com", "hash", "salt").WithId(1);
        user.AssignRoles([new Rol("COUNTRY").WithId(1)]);
        var tokens = new TokenResponseDto("access", "refresh", DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7));
        _usuarios
            .Setup(repository => repository.GetByEmailWithRolesAsync("admin@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(hasher => hasher.VerifyPassword("secret", "salt", "hash"))
            .Returns(true);
        _tokenService
            .Setup(service => service.GenerateTokens(user))
            .Returns(tokens);

        var result = await _sut.LoginAsync(new LoginDto { Email = "admin@example.com", Password = "secret" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("access", result.Value!.AccessToken);
        Assert.Equal("refresh", user.RefreshToken);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsExpired_ShouldReturnInvalidRefreshToken()
    {
        var user = new Usuario("admin@example.com", "hash", "salt").WithId(1);
        user.SetRefreshToken("refresh", DateTime.UtcNow.AddMinutes(-1));
        _usuarios
            .Setup(repository => repository.GetByRefreshTokenWithRolesAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.RefreshAsync(new RefreshTokenDto { RefreshToken = "refresh" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid refresh token.", result.Errors);
    }

    [Fact]
    public async Task RefreshAsync_WhenUserDoesNotExist_ShouldReturnInvalidRefreshToken()
    {
        _usuarios
            .Setup(repository => repository.GetByRefreshTokenWithRolesAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var result = await _sut.RefreshAsync(new RefreshTokenDto { RefreshToken = "refresh" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid refresh token.", result.Errors);
    }

    [Fact]
    public async Task RefreshAsync_WhenUserIsInactive_ShouldReturnInvalidRefreshToken()
    {
        var user = new Usuario("admin@example.com", "hash", "salt").WithId(1);
        user.SetRefreshToken("refresh", DateTime.UtcNow.AddDays(1));
        user.Deactivate();
        _usuarios
            .Setup(repository => repository.GetByRefreshTokenWithRolesAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.RefreshAsync(new RefreshTokenDto { RefreshToken = "refresh" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid refresh token.", result.Errors);
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsValid_ShouldRotateRefreshToken()
    {
        var user = new Usuario("admin@example.com", "hash", "salt").WithId(1);
        user.SetRefreshToken("old", DateTime.UtcNow.AddDays(1));
        var tokens = new TokenResponseDto("access", "new", DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7));
        _usuarios
            .Setup(repository => repository.GetByRefreshTokenWithRolesAsync("old", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenService
            .Setup(service => service.GenerateTokens(user))
            .Returns(tokens);

        var result = await _sut.RefreshAsync(new RefreshTokenDto { RefreshToken = "old" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("new", user.RefreshToken);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenUserExists_ShouldClearRefreshToken()
    {
        var user = new Usuario("admin@example.com", "hash", "salt").WithId(1);
        user.SetRefreshToken("refresh", DateTime.UtcNow.AddDays(1));
        _usuarios
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.LogoutAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(user.RefreshToken);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        _usuarios
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var result = await _sut.LogoutAsync(1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("User was not found.", result.Errors);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenValidatorFails_ShouldReturnValidationErrors()
    {
        _changePasswordValidator
            .Setup(validator => validator.Validate(It.IsAny<ChangePasswordCommand>()))
            .Returns(ValidationResult.Failure(["NewPassword is required."]));

        var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("NewPassword is required.", result.Errors);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        _usuarios
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("User was not found.", result.Errors);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIsInvalid_ShouldReturnError()
    {
        var user = new Usuario("admin@example.com", "hash", "salt").WithId(1);
        _usuarios
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(hasher => hasher.VerifyPassword("old", "salt", "hash"))
            .Returns(false);

        var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Current password is invalid.", result.Errors);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIsValid_ShouldChangePasswordAndClearRefreshToken()
    {
        var user = new Usuario("admin@example.com", "hash", "salt").WithId(1);
        user.SetRefreshToken("refresh", DateTime.UtcNow.AddDays(1));
        _usuarios
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(hasher => hasher.VerifyPassword("old", "salt", "hash"))
            .Returns(true);
        _passwordHasher.Setup(hasher => hasher.GenerateSalt()).Returns("new-salt");
        _passwordHasher.Setup(hasher => hasher.HashPassword("new", "new-salt")).Returns("new-hash");

        var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(user.RefreshToken);
        Assert.Equal("new-salt", user.Salt);
        Assert.Equal("new-hash", user.PasswordHash);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
