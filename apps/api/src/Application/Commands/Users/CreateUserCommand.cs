namespace Application.Commands.Users;

public sealed record CreateUserCommand(string Email, string Password, IReadOnlyCollection<int> RoleIds);
