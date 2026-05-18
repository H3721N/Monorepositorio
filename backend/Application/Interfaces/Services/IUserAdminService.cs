using Application.Common;
using Application.DTOs.Users;

namespace Application.Interfaces.Services;

public interface IUserAdminService
{
    Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<UserDto>> GetMeAsync(int userId, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeactivateAsync(int userId, CancellationToken cancellationToken);
    Task<ServiceResult<UserDto>> ChangeRolesAsync(int userId, UpdateUserRolesDto dto, CancellationToken cancellationToken);
}
