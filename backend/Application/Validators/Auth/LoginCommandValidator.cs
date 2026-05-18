using Application.Commands.Auth;
using Application.Common;
using Application.Interfaces.Validation;

namespace Application.Validators.Auth;

public sealed class LoginCommandValidator : IValidator<LoginCommand>
{
    public ValidationResult Validate(LoginCommand command)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            errors.Add("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors.Add("Password is required.");
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
