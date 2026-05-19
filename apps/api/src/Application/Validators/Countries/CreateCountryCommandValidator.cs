using Application.Commands.Countries;
using Application.Common;
using Application.Interfaces.Validation;

namespace Application.Validators.Countries;

public sealed class CreateCountryCommandValidator : IValidator<CreateCountryCommand>
{
    public ValidationResult Validate(CreateCountryCommand command)
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

        if (string.IsNullOrWhiteSpace(command.IsoCode))
        {
            errors.Add("IsoCode is required.");
        }
        else if (command.IsoCode.Trim().Length != 2)
        {
            errors.Add("IsoCode must contain exactly 2 characters.");
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
