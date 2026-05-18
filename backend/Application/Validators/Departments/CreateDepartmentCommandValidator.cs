using Application.Commands.Departments;
using Application.Common;
using Application.Interfaces.Validation;

namespace Application.Validators.Departments;

public sealed class CreateDepartmentCommandValidator : IValidator<CreateDepartmentCommand>
{
    public ValidationResult Validate(CreateDepartmentCommand command)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            errors.Add("Name is required.");
        }
        else if (command.Name.Trim().Length > 100)
        {
            errors.Add("Name cannot exceed 100 characters.");
        }

        if (command.CountryId <= 0)
        {
            errors.Add("CountryId must be greater than zero.");
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
