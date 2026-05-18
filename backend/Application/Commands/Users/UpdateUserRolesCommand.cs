namespace Application.Commands.Users;

public sealed record UpdateUserRolesCommand(int UserId, IReadOnlyCollection<int> RoleIds);
