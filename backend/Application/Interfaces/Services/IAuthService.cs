using Application.Common;
using Application.DTOs.Auth;

namespace Application.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult<TokenResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<TokenResponseDto>> RefreshAsync(RefreshTokenDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> LogoutAsync(int userId, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellationToken);
}
