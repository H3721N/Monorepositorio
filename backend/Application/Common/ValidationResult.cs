namespace Application.Common;

public sealed class ValidationResult
{
    private ValidationResult(IReadOnlyCollection<string> errors)
    {
        Errors = errors;
    }

    public IReadOnlyCollection<string> Errors { get; }
    public bool IsValid => Errors.Count == 0;

    public static ValidationResult Success()
    {
        return new ValidationResult(Array.Empty<string>());
    }

    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        return new ValidationResult(errors.ToArray());
    }
}
