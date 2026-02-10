using System.Text.Json;
using Craft.Core.Common.Constants;

namespace Craft.Core;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
public class Result : IServiceResult
{
    public bool IsSuccess { get; protected init; }
    bool IServiceResult.IsSuccess => IsSuccess;
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; protected init; }
    public List<string>? Errors { get; protected init; }
    public int? StatusCode { get; protected init; }
    public ErrorType ErrorType { get; protected init; } = ErrorType.None;
    public bool HasErrors => Errors is { Count: > 0 };
    public string? Message => Errors is { Count: > 0 } ? string.Join(", ", Errors) : ErrorMessage;

    protected Result(bool isSuccess, string? errorMessage = null, List<string>? errors = null, int? statusCode = null, ErrorType errorType = ErrorType.None)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors;
        StatusCode = statusCode;
        ErrorType = errorType;
    }

    public static Result CreateSuccess() => new(true);

    public static Result CreateFailure(string errorMessage, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new(false, errorMessage, statusCode: statusCode, errorType: errorType);

    public static Result CreateFailure(List<string> errors, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new(false, errors: errors, statusCode: statusCode, errorType: errorType);

    /// <summary>
    /// Creates a not found failure result.
    /// </summary>
    public static Result NotFound(string message = "Resource not found")
        => new(false, message, statusCode: HttpStatusCodes.NotFound, errorType: ErrorType.NotFound);

    /// <summary>
    /// Creates an unauthorized failure result.
    /// </summary>
    public static Result Unauthorized(string message = "Unauthorized access")
        => new(false, message, statusCode: HttpStatusCodes.Unauthorized, errorType: ErrorType.Unauthorized);

    /// <summary>
    /// Creates a forbidden failure result.
    /// </summary>
    public static Result Forbidden(string message = "Access forbidden")
        => new(false, message, statusCode: HttpStatusCodes.Forbidden, errorType: ErrorType.Forbidden);

    public override string ToString() => JsonSerializer.Serialize(this);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; init; }

    private Result(bool isSuccess, T? value = default, string? errorMessage = null, List<string>? errors = null, int? statusCode = null, ErrorType errorType = ErrorType.None)
        : base(isSuccess, errorMessage, errors, statusCode, errorType)
    {
        Value = value;
    }

    public static Result<T> CreateSuccess(T value) => new(true, value);

    public static new Result<T> CreateFailure(string errorMessage, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new(false, errorMessage: errorMessage, statusCode: statusCode, errorType: errorType);

    public static new Result<T> CreateFailure(List<string> errors, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new(false, errors: errors, statusCode: statusCode, errorType: errorType);

    /// <summary>
    /// Creates a not found failure result.
    /// </summary>
    public static new Result<T> NotFound(string message = "Resource not found")
        => new(false, errorMessage: message, statusCode: HttpStatusCodes.NotFound, errorType: ErrorType.NotFound);

    /// <summary>
    /// Creates an unauthorized failure result.
    /// </summary>
    public static new Result<T> Unauthorized(string message = "Unauthorized access")
        => new(false, errorMessage: message, statusCode: HttpStatusCodes.Unauthorized, errorType: ErrorType.Unauthorized);

    /// <summary>
    /// Creates a forbidden failure result.
    /// </summary>
    public static new Result<T> Forbidden(string message = "Access forbidden")
        => new(false, errorMessage: message, statusCode: HttpStatusCodes.Forbidden, errorType: ErrorType.Forbidden);

    /// <summary>
    /// Maps the value to another type if successful.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess && Value != null
            ? Result<TNew>.CreateSuccess(mapper(Value))
            : Result<TNew>.CreateFailure(ErrorMessage ?? "Operation failed", ErrorType, StatusCode);
    }

    /// <summary>
    /// Binds the value to another Result if successful.
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        return IsSuccess && Value != null
            ? binder(Value)
            : Result<TNew>.CreateFailure(ErrorMessage ?? "Operation failed", ErrorType, StatusCode);
    }
}
