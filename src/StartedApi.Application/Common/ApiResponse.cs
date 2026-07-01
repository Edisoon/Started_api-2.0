namespace StartedApi.Application.Common;

public sealed record ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ApiResponse<T> Ok(T data, string message = "") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors ?? Array.Empty<string>()
        };
}
