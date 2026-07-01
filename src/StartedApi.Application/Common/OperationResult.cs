namespace StartedApi.Application.Common;

public sealed record OperationResult<T>
{
    private OperationResult(bool succeeded, T? value, string message, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Value = value;
        Message = message;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public T? Value { get; }

    public string Message { get; }

    public IReadOnlyList<string> Errors { get; }

    public static OperationResult<T> Success(T value, string message = "") =>
        new(true, value, message, Array.Empty<string>());

    public static OperationResult<T> Failure(string message, IReadOnlyList<string>? errors = null) =>
        new(false, default, message, errors ?? Array.Empty<string>());
}
