using Application.Commands.Users;
using Application.Common;
using Application.DTOs.Users;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validation;
using Domain.Entities;

namespace Application.Services;

public sealed class UserAdminService : IUserAdminService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateUserCommand> _createValidator;
    private readonly IValidator<UpdateUserRolesCommand> _updateRolesValidator;

    public UserAdminService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IValidator<CreateUserCommand> createValidator,
        IValidator<UpdateUserRolesCommand> updateRolesValidator)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateRolesValidator = updateRolesValidator;
    }

    public async Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(dto.Email, dto.Password, dto.RoleIds);
        var validation = _createValidator.Validate(command);
        if (!validation.IsValid)
        {
            return ServiceResult<UserDto>.Failure(validation.Errors);
        }

        var roles = await _rolRepository.GetByIdsAsync(command.RoleIds, cancellationToken);
        if (roles.Count != command.RoleIds.Distinct().Count())
        {
            return ServiceResult<UserDto>.Failure("One or more roles were not found.");
        }

        if (await _usuarioRepository.ExistsByEmailAsync(command.Email, null, cancellationToken))
        {
            return ServiceResult<UserDto>.Failure("A user with the same email already exists.");
        }

        var salt = _passwordHasher.GenerateSalt();
        var passwordHash = _passwordHasher.HashPassword(command.Password, salt);
        var usuario = new Usuario(command.Email, passwordHash, salt);
        usuario.AssignRoles(roles);

        await _usuarioRepository.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserDto>.Success(ToDto(usuario));
    }

    public async Task<ServiceResult<UserDto>> GetMeAsync(int userId, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdWithRolesAsync(userId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            return ServiceResult<UserDto>.Failure("User was not found.");
        }

        return ServiceResult<UserDto>.Success(ToDto(usuario));
    }

    public async Task<ServiceResult<bool>> DeactivateAsync(int userId, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            return ServiceResult<bool>.Failure("User was not found.");
        }

        usuario.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<UserDto>> ChangeRolesAsync(int userId, UpdateUserRolesDto dto, CancellationToken cancellationToken)
    {
        var command = new UpdateUserRolesCommand(userId, dto.RoleIds);
        var validation = _updateRolesValidator.Validate(command);
        if (!validation.IsValid)
        {
            return ServiceResult<UserDto>.Failure(validation.Errors);
        }

        var roles = await _rolRepository.GetByIdsAsync(command.RoleIds, cancellationToken);
        if (roles.Count != command.RoleIds.Distinct().Count())
        {
            return ServiceResult<UserDto>.Failure("One or more roles were not found.");
        }

        var usuario = await _usuarioRepository.GetByIdWithRolesAsync(userId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            return ServiceResult<UserDto>.Failure("User was not found.");
        }

        usuario.AssignRoles(roles);
        usuario.ClearRefreshToken();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserDto>.Success(ToDto(usuario));
    }

    private static UserDto ToDto(Usuario usuario)
    {
        var roles = usuario.Roles
            .Select(role => role.Nombre)
            .OrderBy(role => role)
            .ToArray();

        return new UserDto(usuario.Id, usuario.Email, roles, usuario.Activo);
    }
}
