using Application.Commands.Users;
using Application.Common;
using Application.Interfaces.Validation;

namespace Application.Validators.Users;

public sealed class UpdateUserRolesCommandValidator : IValidator<UpdateUserRolesCommand>
{
    public ValidationResult Validate(UpdateUserRolesCommand command)
    {
        var errors = new List<string>();

        if (command.UserId <= 0)
        {
            errors.Add("UserId must be greater than zero.");
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
