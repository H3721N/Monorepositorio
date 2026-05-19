using Application.Common;

namespace Application.Interfaces.Validation;

public interface IValidator<in TCommand>
{
    ValidationResult Validate(TCommand command);
}
