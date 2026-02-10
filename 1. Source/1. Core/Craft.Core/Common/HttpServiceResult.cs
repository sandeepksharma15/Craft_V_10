using Craft.Core.Common.Constants;

namespace Craft.Core.Common;

/// <summary>
/// Represents the result of an HTTP service operation.
/// </summary>
/// <remarks>
/// Use this type for HTTP client operations and external API calls.
/// For domain/application layer operations, use <see cref="Result{T}"/>.
/// For API responses to clients, use <see cref="ServerResponse"/>.
/// </remarks>
/// <typeparam name="T">The type of data returned on success.</typeparam>
public record HttpServiceResult<T> : IServiceResult
{
    /// <summary>
    /// Gets the data returned by the operation when successful.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the list of error messages, if any.
    /// </summary>
    public List<string>? Errors { get; init; }

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Gets the type of error that occurred.
    /// </summary>
    public ErrorType ErrorType { get; init; } = ErrorType.None;

    public bool HasErrors => Errors is { Count: > 0 };
    public bool IsFailure => !IsSuccess;
    public string? Message => Errors is { Count: > 0 } ? string.Join(", ", Errors) : null;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static HttpServiceResult<T> SuccessResult(T data, int? statusCode = 200)
        => new() { Data = data, IsSuccess = true, StatusCode = statusCode };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static HttpServiceResult<T> FailureResult(List<string>? errors = null, int? statusCode = 400, ErrorType errorType = ErrorType.Validation)
        => new() { IsSuccess = false, Errors = errors, StatusCode = statusCode, ErrorType = errorType };

    /// <summary>
    /// Creates a not found failure result.
    /// </summary>
    public static HttpServiceResult<T> NotFound(string message = "Resource not found")
        => new() { IsSuccess = false, Errors = [message], StatusCode = HttpStatusCodes.NotFound, ErrorType = ErrorType.NotFound };

    /// <summary>
    /// Creates an unauthorized failure result.
    /// </summary>
    public static HttpServiceResult<T> Unauthorized(string message = "Unauthorized access")
        => new() { IsSuccess = false, Errors = [message], StatusCode = HttpStatusCodes.Unauthorized, ErrorType = ErrorType.Unauthorized };

    /// <summary>
    /// Creates a forbidden failure result.
    /// </summary>
    public static HttpServiceResult<T> Forbidden(string message = "Access forbidden")
        => new() { IsSuccess = false, Errors = [message], StatusCode = HttpStatusCodes.Forbidden, ErrorType = ErrorType.Forbidden };

    /// <summary>
    /// Creates an internal server error result.
    /// </summary>
    public static HttpServiceResult<T> InternalError(string message = "An unexpected error occurred")
        => new() { IsSuccess = false, Errors = [message], StatusCode = HttpStatusCodes.InternalServerError, ErrorType = ErrorType.Internal };
}

/// <summary>
/// Extension methods for IServiceResult and result types.
/// </summary>
public static class ServiceResultExtensions
{
    /// <summary>
    /// Converts an exception to a failed HttpServiceResult.
    /// </summary>
    public static HttpServiceResult<T> ToServiceResult<T>(this Exception ex, int? statusCode = 500)
    {
        return new HttpServiceResult<T>
        {
            IsSuccess = false,
            Errors = [ex.Message],
            StatusCode = statusCode,
            ErrorType = ErrorType.Internal
        };
    }

    /// <summary>
    /// Maps the data of a HttpServiceResult to another type.
    /// </summary>
    public static HttpServiceResult<TNew> Map<T, TNew>(this HttpServiceResult<T> result, Func<T?, TNew?> mapper)
    {
        return new HttpServiceResult<TNew>
        {
            IsSuccess = result.IsSuccess,
            Data = mapper(result.Data),
            Errors = result.Errors,
            StatusCode = result.StatusCode,
            ErrorType = result.ErrorType
        };
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to HttpServiceResult&lt;T&gt;.
    /// </summary>
    public static HttpServiceResult<T> ToHttpServiceResult<T>(this Result<T> result, int? statusCode = null)
    {
        return new HttpServiceResult<T>
        {
            IsSuccess = result.IsSuccess,
            Data = result.Value,
            Errors = result.Errors,
            StatusCode = statusCode ?? result.StatusCode,
            ErrorType = result.ErrorType
        };
    }

    /// <summary>
    /// Returns the first error message or null.
    /// </summary>
    public static string? FirstError(this IServiceResult result)
        => result.Errors?.Count > 0 ? result.Errors[0] : null;

    /// <summary>
    /// Returns the first error message or a default message.
    /// </summary>
    public static string FirstErrorOrDefault(this IServiceResult result, string defaultMessage = "An error occurred")
        => result.Errors?.Count > 0 ? result.Errors[0] : defaultMessage;

    /// <summary>
    /// Combines errors from multiple results into a single list.
    /// </summary>
    public static List<string> CombineErrors(this IEnumerable<IServiceResult> results)
        => [.. results.Where(r => r.Errors != null).SelectMany(r => r.Errors!).Distinct()];

    /// <summary>
    /// Checks if all results in the collection are successful.
    /// </summary>
    public static bool AllSuccessful(this IEnumerable<IServiceResult> results)
        => results.All(r => r.IsSuccess);

    /// <summary>
    /// Gets the most severe error type from a collection of results.
    /// </summary>
    public static ErrorType GetMostSevereErrorType(this IEnumerable<IServiceResult> results)
        => results.Where(r => r.IsFailure).Select(r => r.ErrorType).DefaultIfEmpty(ErrorType.None).Max();

    /// <summary>
    /// Asynchronously maps the result if successful.
    /// </summary>
    public static async Task<Result<TNew>> MapAsync<T, TNew>(this Task<Result<T>> resultTask, Func<T, TNew> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapper);
    }

    /// <summary>
    /// Asynchronously maps the result with an async mapper function.
    /// </summary>
    public static async Task<Result<TNew>> MapAsync<T, TNew>(this Task<Result<T>> resultTask, Func<T, Task<TNew>> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure || result.Value is null)
            return Result<TNew>.CreateFailure(result.ErrorMessage ?? "Operation failed", result.ErrorType, result.StatusCode);

        var newValue = await mapper(result.Value).ConfigureAwait(false);
        return Result<TNew>.CreateSuccess(newValue);
    }

    /// <summary>
    /// Asynchronously binds the result to another result if successful.
    /// </summary>
    public static async Task<Result<TNew>> BindAsync<T, TNew>(this Task<Result<T>> resultTask, Func<T, Task<Result<TNew>>> binder)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure || result.Value is null)
            return Result<TNew>.CreateFailure(result.ErrorMessage ?? "Operation failed", result.ErrorType, result.StatusCode);

        return await binder(result.Value).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess && result.Value is not null)
            await action(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    public static async Task<Result<T>> TapErrorAsync<T>(this Task<Result<T>> resultTask, Func<IServiceResult, Task> action)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            await action(result).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Returns a fallback value if the result is a failure.
    /// </summary>
    public static T? GetValueOrDefault<T>(this Result<T> result, T? defaultValue = default)
        => result.IsSuccess ? result.Value : defaultValue;

    /// <summary>
    /// Throws an exception if the result is a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result represents a failure.</exception>
    public static T GetValueOrThrow<T>(this Result<T> result)
        => result.IsSuccess && result.Value is not null
            ? result.Value
            : throw new InvalidOperationException(result.Message ?? "Operation failed");

    /// <summary>
    /// Executes one of two functions depending on success or failure.
    /// </summary>
    public static TResult Match<T, TResult>(this Result<T> result, Func<T, TResult> onSuccess, Func<IServiceResult, TResult> onFailure)
        => result.IsSuccess && result.Value is not null ? onSuccess(result.Value) : onFailure(result);

    /// <summary>
    /// Asynchronously executes one of two functions depending on success or failure.
    /// </summary>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Result<T>> resultTask,
        Func<T, Task<TResult>> onSuccess,
        Func<IServiceResult, Task<TResult>> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess && result.Value is not null
            ? await onSuccess(result.Value).ConfigureAwait(false)
            : await onFailure(result).ConfigureAwait(false);
    }
}
