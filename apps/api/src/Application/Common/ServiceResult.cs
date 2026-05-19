namespace Application.Common;

public sealed class ServiceResult<T>
{
    private ServiceResult(bool isSuccess, T? value, IReadOnlyCollection<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyCollection<string> Errors { get; }

    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>(true, value, Array.Empty<string>());
    }

    public static ServiceResult<T> Failure(params string[] errors)
    {
        return new ServiceResult<T>(false, default, errors);
    }

    public static ServiceResult<T> Failure(IEnumerable<string> errors)
    {
        return new ServiceResult<T>(false, default, errors.ToArray());
    }
}
