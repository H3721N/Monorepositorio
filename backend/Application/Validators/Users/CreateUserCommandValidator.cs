using Application.Commands.Users;
using Application.Common;
using Application.Interfaces.Validation;

namespace Application.Validators.Users;

public sealed class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public ValidationResult Validate(CreateUserCommand command)
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

        if (command.RoleIds.Count == 0)
        {
            errors.Add("At least one RoleId is required.");
        }

        if (command.RoleIds.Any(roleId => roleId <= 0))
        {
            errors.Add("Every RoleId must be greater than zero.");
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
