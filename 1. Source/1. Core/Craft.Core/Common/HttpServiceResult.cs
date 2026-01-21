namespace Craft.Core.Common;

public class HttpServiceResult<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
    public int? StatusCode { get; set; }

    public bool HasErrors => Errors is { Count: > 0 };
    public bool IsFailure => !Success;
    public string? Message => Errors is { Count: > 0 } ? string.Join(", ", Errors) : null;

    public static HttpServiceResult<T> SuccessResult(T data, int? statusCode = 200)
        => new() { Data = data, Success = true, StatusCode = statusCode};

    public static HttpServiceResult<T> FailureResult(List<string>? errors = null, int? statusCode = 400)
        => new() { Success = false, Errors = errors, StatusCode = statusCode };
}
