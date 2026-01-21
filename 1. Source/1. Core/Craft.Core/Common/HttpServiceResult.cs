namespace Craft.Core.Common;

/// <summary>
/// Represents the result of an HTTP service operation.
/// </summary>
public record HttpServiceResult<T>
{
    public T? Data { get; init; }
    public bool Success { get; init; }
    public List<string>? Errors { get; init; }
    public int? StatusCode { get; init; }

    public bool HasErrors => Errors is { Count: > 0 };
    public bool IsFailure => !Success;
    public string? Message => Errors is { Count: > 0 } ? string.Join(", ", Errors) : null;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static HttpServiceResult<T> SuccessResult(T data, int? statusCode = 200)
        => new() { Data = data, Success = true, StatusCode = statusCode };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static HttpServiceResult<T> FailureResult(List<string>? errors = null, int? statusCode = 400)
        => new() { Success = false, Errors = errors, StatusCode = statusCode };
}
