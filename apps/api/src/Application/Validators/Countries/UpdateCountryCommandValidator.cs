using Application.Commands.Countries;
using Application.Common;
using Application.Interfaces.Validation;

namespace Application.Validators.Countries;

public sealed class UpdateCountryCommandValidator : IValidator<UpdateCountryCommand>
{
    private readonly CreateCountryCommandValidator _baseValidator = new();

    public ValidationResult Validate(UpdateCountryCommand command)
    {
        var errors = new List<string>();

        if (command.Id <= 0)
        {
            errors.Add("Id must be greater than zero.");
        }

        var baseResult = _baseValidator.Validate(new CreateCountryCommand(command.Name, command.IsoCode));
        errors.AddRange(baseResult.Errors);

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
