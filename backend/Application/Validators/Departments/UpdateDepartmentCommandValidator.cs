using Application.Commands.Departments;
using Application.Common;
using Application.Interfaces.Validation;

namespace Application.Validators.Departments;

public sealed class UpdateDepartmentCommandValidator : IValidator<UpdateDepartmentCommand>
{
    private readonly CreateDepartmentCommandValidator _baseValidator = new();

    public ValidationResult Validate(UpdateDepartmentCommand command)
    {
        var errors = new List<string>();

        if (command.Id <= 0)
        {
            errors.Add("Id must be greater than zero.");
        }

        var baseResult = _baseValidator.Validate(new CreateDepartmentCommand(command.Name, command.CountryId));
        errors.AddRange(baseResult.Errors);

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
