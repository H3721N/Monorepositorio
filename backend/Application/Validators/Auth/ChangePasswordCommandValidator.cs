using Application.Commands.Auth;
using Application.Common;
using Application.Interfaces.Validation;

namespace Application.Validators.Auth;

public sealed class ChangePasswordCommandValidator : IValidator<ChangePasswordCommand>
{
    public ValidationResult Validate(ChangePasswordCommand command)
    {
        var errors = new List<string>();

        if (command.UserId <= 0)
        {
            errors.Add("UserId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(command.CurrentPassword))
        {
            errors.Add("CurrentPassword is required.");
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword))
        {
            errors.Add("NewPassword is required.");
        }
        else if (command.NewPassword.Length < 8)
        {
            errors.Add("NewPassword must contain at least 8 characters.");
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
