using System.Text.Json;

namespace Craft.Core;

/// <summary>
/// Represents the result of a service operation that can succeed or fail.
/// </summary>
/// <remarks>
/// <para>This is the primary result type for all service operations in the application.</para>
/// <para><b>Usage guidance:</b></para>
/// <list type="bullet">
///   <item><description>Use <see cref="ServiceResult"/> for operations without return data</description></item>
///   <item><description>Use <see cref="ServiceResult{T}"/> for operations that return data</description></item>
///   <item><description>Use <see cref="ServerResponse"/> or <see cref="ServerResponse{T}"/> for API responses to clients</description></item>
/// </list>
/// </remarks>
public class ServiceResult : IServiceResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; protected init; }

    bool IServiceResult.IsSuccess => IsSuccess;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the primary error message, if any.
    /// </summary>
    public string? ErrorMessage { get; protected init; }

    /// <summary>
    /// Gets the list of error messages, if any.
    /// </summary>
    public List<string>? Errors { get; protected init; }

    /// <summary>
    /// Gets the HTTP status code associated with the result, if any.
    /// </summary>
    public int? StatusCode { get; protected init; }

    /// <summary>
    /// Gets the type of error that occurred.
    /// </summary>
    public ErrorType ErrorType { get; protected init; } = ErrorType.None;

    /// <summary>
    /// Gets a value indicating whether there are any errors.
    /// </summary>
    public bool HasErrors => Errors is { Count: > 0 };

    /// <summary>
    /// Gets a combined message from all errors or the primary error message.
    /// </summary>
    public string? Message => Errors is { Count: > 0 } ? string.Join(", ", Errors) : ErrorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResult"/> class.
    /// </summary>
    protected ServiceResult(bool isSuccess, string? errorMessage = null, List<string>? errors = null, int? statusCode = null, ErrorType errorType = ErrorType.None)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors;
        StatusCode = statusCode;
        ErrorType = errorType;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static ServiceResult Success() => new(true);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static ServiceResult Failure(string errorMessage, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new(false, errorMessage, statusCode: statusCode, errorType: errorType);

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    public static ServiceResult Failure(List<string> errors, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new(false, errors: errors, statusCode: statusCode, errorType: errorType);

    /// <summary>
    /// Creates a not found failure result.
    /// </summary>
    public static ServiceResult NotFound(string message = "Resource not found")
        => new(false, message, statusCode: HttpStatusCodes.NotFound, errorType: ErrorType.NotFound);

    /// <summary>
    /// Creates an unauthorized failure result.
    /// </summary>
    public static ServiceResult Unauthorized(string message = "Unauthorized access")
        => new(false, message, statusCode: HttpStatusCodes.Unauthorized, errorType: ErrorType.Unauthorized);

    /// <summary>
    /// Creates a forbidden failure result.
    /// </summary>
    public static ServiceResult Forbidden(string message = "Access forbidden")
        => new(false, message, statusCode: HttpStatusCodes.Forbidden, errorType: ErrorType.Forbidden);

    /// <summary>
    /// Creates an internal server error result.
    /// </summary>
    public static ServiceResult InternalError(string message = "An unexpected error occurred")
        => new(false, message, statusCode: HttpStatusCodes.InternalServerError, errorType: ErrorType.Internal);

    /// <summary>
    /// Creates a conflict failure result.
    /// </summary>
    public static ServiceResult Conflict(string message = "Resource conflict")
        => new(false, message, statusCode: 409, errorType: ErrorType.Conflict);

    /// <summary>
    /// Creates a timeout failure result.
    /// </summary>
    public static ServiceResult Timeout(string message = "Operation timed out")
        => new(false, message, statusCode: 408, errorType: ErrorType.Timeout);

    /// <summary>
    /// Creates a result from an exception.
    /// </summary>
    public static ServiceResult FromException(Exception ex, int? statusCode = 500)
        => new(false, ex.Message, statusCode: statusCode, errorType: ErrorType.Internal);

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(this);
}

/// <summary>
/// Represents the result of a service operation that returns a value.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class ServiceResult<T> : ServiceResult
{
    /// <summary>
    /// Gets the value returned by the operation when successful.
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResult{T}"/> class.
    /// </summary>
    protected ServiceResult(bool isSuccess, T? value = default, string? errorMessage = null, List<string>? errors = null, int? statusCode = null, ErrorType errorType = ErrorType.None)
        : base(isSuccess, errorMessage, errors, statusCode, errorType)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static ServiceResult<T> Success(T value) => new(true, value);

    /// <summary>
    /// Creates a successful result with a value and status code.
    /// </summary>
    public static ServiceResult<T> Success(T value, int statusCode) => new(true, value, statusCode: statusCode);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static new ServiceResult<T> Failure(string errorMessage, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new(false, errorMessage: errorMessage, statusCode: statusCode, errorType: errorType);

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    public static new ServiceResult<T> Failure(List<string> errors, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new(false, errors: errors, statusCode: statusCode, errorType: errorType);

    /// <summary>
    /// Creates a not found failure result.
    /// </summary>
    public static new ServiceResult<T> NotFound(string message = "Resource not found")
        => new(false, errorMessage: message, statusCode: HttpStatusCodes.NotFound, errorType: ErrorType.NotFound);

    /// <summary>
    /// Creates an unauthorized failure result.
    /// </summary>
    public static new ServiceResult<T> Unauthorized(string message = "Unauthorized access")
        => new(false, errorMessage: message, statusCode: HttpStatusCodes.Unauthorized, errorType: ErrorType.Unauthorized);

    /// <summary>
    /// Creates a forbidden failure result.
    /// </summary>
    public static new ServiceResult<T> Forbidden(string message = "Access forbidden")
        => new(false, errorMessage: message, statusCode: HttpStatusCodes.Forbidden, errorType: ErrorType.Forbidden);

    /// <summary>
    /// Creates an internal server error result.
    /// </summary>
    public static new ServiceResult<T> InternalError(string message = "An unexpected error occurred")
        => new(false, errorMessage: message, statusCode: HttpStatusCodes.InternalServerError, errorType: ErrorType.Internal);

    /// <summary>
    /// Creates a conflict failure result.
    /// </summary>
    public static new ServiceResult<T> Conflict(string message = "Resource conflict")
        => new(false, errorMessage: message, statusCode: 409, errorType: ErrorType.Conflict);

    /// <summary>
    /// Creates a timeout failure result.
    /// </summary>
    public static new ServiceResult<T> Timeout(string message = "Operation timed out")
        => new(false, errorMessage: message, statusCode: 408, errorType: ErrorType.Timeout);

    /// <summary>
    /// Creates a result from an exception.
    /// </summary>
    public static new ServiceResult<T> FromException(Exception ex, int? statusCode = 500)
        => new(false, errorMessage: ex.Message, statusCode: statusCode, errorType: ErrorType.Internal);

    /// <summary>
    /// Maps the value to another type if successful.
    /// </summary>
    /// <typeparam name="TNew">The type to map to.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new result with the mapped value or the original error.</returns>
    public ServiceResult<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess && Value is not null
            ? ServiceResult<TNew>.Success(mapper(Value))
            : ServiceResult<TNew>.Failure(ErrorMessage ?? "Operation failed", ErrorType, StatusCode);
    }

    /// <summary>
    /// Binds the value to another result if successful.
    /// </summary>
    /// <typeparam name="TNew">The type of the new result value.</typeparam>
    /// <param name="binder">The binding function.</param>
    /// <returns>The bound result or a failure result.</returns>
    public ServiceResult<TNew> Bind<TNew>(Func<T, ServiceResult<TNew>> binder)
    {
        return IsSuccess && Value is not null
            ? binder(Value)
            : ServiceResult<TNew>.Failure(ErrorMessage ?? "Operation failed", ErrorType, StatusCode);
    }
}
