namespace Application.Commands.Auth;

public sealed record ChangePasswordCommand(int UserId, string CurrentPassword, string NewPassword);
