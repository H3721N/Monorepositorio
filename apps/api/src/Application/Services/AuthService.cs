using Application.Commands.Auth;
using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validation;

namespace Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<LoginCommand> _loginValidator;
    private readonly IValidator<ChangePasswordCommand> _changePasswordValidator;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IValidator<LoginCommand> loginValidator,
        IValidator<ChangePasswordCommand> changePasswordValidator)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<ServiceResult<TokenResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(dto.Email, dto.Password);
        var validation = _loginValidator.Validate(command);
        if (!validation.IsValid)
        {
            return ServiceResult<TokenResponseDto>.Failure(validation.Errors);
        }

        var usuario = await _usuarioRepository.GetByEmailWithRolesAsync(command.Email, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            return ServiceResult<TokenResponseDto>.Failure("Invalid credentials.");
        }

        if (!_passwordHasher.VerifyPassword(command.Password, usuario.Salt, usuario.PasswordHash))
        {
            return ServiceResult<TokenResponseDto>.Failure("Invalid credentials.");
        }

        var tokens = _tokenService.GenerateTokens(usuario);
        usuario.SetRefreshToken(tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<TokenResponseDto>.Success(tokens);
    }

    public async Task<ServiceResult<TokenResponseDto>> RefreshAsync(RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByRefreshTokenWithRolesAsync(dto.RefreshToken, cancellationToken);
        if (usuario is null || !usuario.Activo || usuario.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ServiceResult<TokenResponseDto>.Failure("Invalid refresh token.");
        }

        var tokens = _tokenService.GenerateTokens(usuario);
        usuario.SetRefreshToken(tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<TokenResponseDto>.Success(tokens);
    }

    public async Task<ServiceResult<bool>> LogoutAsync(int userId, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            return ServiceResult<bool>.Failure("User was not found.");
        }

        usuario.ClearRefreshToken();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(userId, dto.CurrentPassword, dto.NewPassword);
        var validation = _changePasswordValidator.Validate(command);
        if (!validation.IsValid)
        {
            return ServiceResult<bool>.Failure(validation.Errors);
        }

        var usuario = await _usuarioRepository.GetByIdAsync(userId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            return ServiceResult<bool>.Failure("User was not found.");
        }

        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, usuario.Salt, usuario.PasswordHash))
        {
            return ServiceResult<bool>.Failure("Current password is invalid.");
        }

        var salt = _passwordHasher.GenerateSalt();
        var passwordHash = _passwordHasher.HashPassword(command.NewPassword, salt);
        usuario.ChangePassword(passwordHash, salt);
        usuario.ClearRefreshToken();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }
}
