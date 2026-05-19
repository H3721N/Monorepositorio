namespace Application.DTOs.Users;

public sealed record UserDto(
    int Id,
    string Email,
    IReadOnlyCollection<string> Roles,
    bool Activo);
