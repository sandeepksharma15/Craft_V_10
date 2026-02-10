using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Core.Common.Constants;
using Microsoft.AspNetCore.Identity;

namespace Craft.Core;

/// <summary>
/// Represents a standardized API response for client consumption.
/// </summary>
/// <remarks>
/// <para>Use this type for API controller responses to clients.</para>
/// <para>For domain/application layer operations, use <see cref="ServiceResult{T}"/>.</para>
/// </remarks>
public class ServerResponse : IServiceResult
{
    /// <summary>
    /// Gets or sets the response data payload.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the list of error messages.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the type of error that occurred.
    /// </summary>
    public ErrorType ErrorType { get; set; } = ErrorType.None;

    /// <summary>
    /// Gets or sets a unique identifier for this error for tracking purposes.
    /// </summary>
    public string? ErrorId { get; set; }

    /// <summary>
    /// Gets or sets the expiration date for cached responses or tokens.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets a human-readable message describing the result.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the source of the response (e.g., service name).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets a support message for error scenarios.
    /// </summary>
    public string? SupportMessage { get; set; }

    // IServiceResult implementation
    [JsonIgnore]
    int? IServiceResult.StatusCode => StatusCode;

    [JsonIgnore]
    List<string>? IServiceResult.Errors => Errors;

    public bool HasErrors => Errors.Count > 0;
    public bool IsFailure => !IsSuccess;

    public ServerResponse() { }

    public ServerResponse(IdentityResult identityResult)
    {
        IsSuccess = identityResult.Succeeded;
        Errors = [.. identityResult.Errors.Select(x => x.Description)];
        Data = null;
        ErrorType = identityResult.Succeeded ? ErrorType.None : ErrorType.Validation;
        StatusCode = identityResult.Succeeded ? HttpStatusCodes.Ok : HttpStatusCodes.BadRequest;
    }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ServerResponse Success(object? data = null, string? message = null)
        => new()
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = HttpStatusCodes.Ok
        };

    /// <summary>
    /// Creates a failure response.
    /// </summary>
    public static ServerResponse Failure(string error, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new()
        {
            IsSuccess = false,
            Errors = [error],
            ErrorType = errorType,
            StatusCode = statusCode ?? GetDefaultStatusCode(errorType)
        };

    /// <summary>
    /// Creates a failure response with multiple errors.
    /// </summary>
    public static ServerResponse Failure(List<string> errors, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new()
        {
            IsSuccess = false,
            Errors = errors,
            ErrorType = errorType,
            StatusCode = statusCode ?? GetDefaultStatusCode(errorType)
        };

    /// <summary>
    /// Creates a not found response.
    /// </summary>
    public static ServerResponse NotFound(string message = "Resource not found")
        => Failure(message, ErrorType.NotFound, HttpStatusCodes.NotFound);

    /// <summary>
    /// Creates an unauthorized response.
    /// </summary>
    public static ServerResponse Unauthorized(string message = "Unauthorized access")
        => Failure(message, ErrorType.Unauthorized, HttpStatusCodes.Unauthorized);

    /// <summary>
    /// Creates a forbidden response.
    /// </summary>
    public static ServerResponse Forbidden(string message = "Access forbidden")
        => Failure(message, ErrorType.Forbidden, HttpStatusCodes.Forbidden);

    /// <summary>
    /// Creates an internal server error response.
    /// </summary>
    public static ServerResponse InternalError(string message = "An unexpected error occurred", string? errorId = null)
        => new()
        {
            IsSuccess = false,
            Errors = [message],
            ErrorType = ErrorType.Internal,
            StatusCode = HttpStatusCodes.InternalServerError,
            ErrorId = errorId ?? Guid.NewGuid().ToString("N")[..8]
        };

    /// <summary>
    /// Creates a response from an IServiceResult.
    /// </summary>
    public static ServerResponse FromResult(IServiceResult result)
        => new()
        {
            IsSuccess = result.IsSuccess,
            Errors = result.Errors?.ToList() ?? [],
            Message = result.Message,
            ErrorType = result.ErrorType,
            StatusCode = result.StatusCode ?? GetDefaultStatusCode(result.ErrorType)
        };

    private static int GetDefaultStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => HttpStatusCodes.NotFound,
        ErrorType.Unauthorized => HttpStatusCodes.Unauthorized,
        ErrorType.Forbidden => HttpStatusCodes.Forbidden,
        ErrorType.Validation => HttpStatusCodes.BadRequest,
        ErrorType.Conflict => 409,
        ErrorType.Internal => HttpStatusCodes.InternalServerError,
        ErrorType.Timeout => 408,
        _ => HttpStatusCodes.BadRequest
    };

    public override string ToString() => JsonSerializer.Serialize(this);
}

/// <summary>
/// Represents a strongly-typed standardized API response for client consumption.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public class ServerResponse<T> : IServiceResult
{
    /// <summary>
    /// Gets or sets the response data payload.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the list of error messages.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the type of error that occurred.
    /// </summary>
    public ErrorType ErrorType { get; set; } = ErrorType.None;

    /// <summary>
    /// Gets or sets a human-readable message describing the result.
    /// </summary>
    public string? Message { get; set; }

    // IServiceResult implementation
    [JsonIgnore]
    int? IServiceResult.StatusCode => StatusCode;

    [JsonIgnore]
    List<string>? IServiceResult.Errors => Errors;

    public bool HasErrors => Errors.Count > 0;
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ServerResponse<T> Success(T data, string? message = null)
        => new()
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = HttpStatusCodes.Ok
        };

    /// <summary>
    /// Creates a failure response.
    /// </summary>
    public static ServerResponse<T> Failure(string error, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new()
        {
            IsSuccess = false,
            Errors = [error],
            ErrorType = errorType,
            StatusCode = statusCode ?? GetDefaultStatusCode(errorType)
        };

    /// <summary>
    /// Creates a failure response with multiple errors.
    /// </summary>
    public static ServerResponse<T> Failure(List<string> errors, ErrorType errorType = ErrorType.Validation, int? statusCode = null)
        => new()
        {
            IsSuccess = false,
            Errors = errors,
            ErrorType = errorType,
            StatusCode = statusCode ?? GetDefaultStatusCode(errorType)
        };

    /// <summary>
    /// Creates a not found response.
    /// </summary>
    public static ServerResponse<T> NotFound(string message = "Resource not found")
        => Failure(message, ErrorType.NotFound, HttpStatusCodes.NotFound);

    /// <summary>
    /// Creates a response from a ServiceResult&lt;T&gt;.
    /// </summary>
    public static ServerResponse<T> FromServiceResult(ServiceResult<T> result)
        => new()
        {
            IsSuccess = result.IsSuccess,
            Data = result.Value,
            Errors = result.Errors?.ToList() ?? [],
            Message = result.Message,
            ErrorType = result.ErrorType,
            StatusCode = result.StatusCode ?? GetDefaultStatusCode(result.ErrorType)
        };

    private static int GetDefaultStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => HttpStatusCodes.NotFound,
        ErrorType.Unauthorized => HttpStatusCodes.Unauthorized,
        ErrorType.Forbidden => HttpStatusCodes.Forbidden,
        ErrorType.Validation => HttpStatusCodes.BadRequest,
        ErrorType.Conflict => 409,
        ErrorType.Internal => HttpStatusCodes.InternalServerError,
        ErrorType.Timeout => 408,
        _ => HttpStatusCodes.BadRequest
    };

    public override string ToString() => JsonSerializer.Serialize(this);
}
