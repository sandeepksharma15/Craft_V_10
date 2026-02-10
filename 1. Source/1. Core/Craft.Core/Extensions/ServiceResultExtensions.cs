using Craft.Core.Common.Constants;

namespace Craft.Core.Common;

/// <summary>
/// Extension methods for <see cref="IServiceResult"/> and <see cref="ServiceResult{T}"/>.
/// </summary>
public static class ServiceResultExtensions
{
    /// <summary>
    /// Converts an exception to a failed ServiceResult.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="ex">The exception to convert.</param>
    /// <param name="statusCode">The HTTP status code (defaults to 500).</param>
    /// <returns>A failed ServiceResult containing the exception message.</returns>
    public static ServiceResult<T> ToServiceResult<T>(this Exception ex, int? statusCode = 500)
        => ServiceResult<T>.FromException(ex, statusCode);

    /// <summary>
    /// Returns the first error message or null.
    /// </summary>
    /// <param name="result">The service result.</param>
    /// <returns>The first error message, or null if no errors exist.</returns>
    public static string? FirstError(this IServiceResult result)
        => result.Errors?.Count > 0 ? result.Errors[0] : null;

    /// <summary>
    /// Returns the first error message or a default message.
    /// </summary>
    /// <param name="result">The service result.</param>
    /// <param name="defaultMessage">The default message if no errors exist.</param>
    /// <returns>The first error message or the default message.</returns>
    public static string FirstErrorOrDefault(this IServiceResult result, string defaultMessage = "An error occurred")
        => result.Errors?.Count > 0 ? result.Errors[0] : defaultMessage;

    /// <summary>
    /// Combines errors from multiple results into a single list.
    /// </summary>
    /// <param name="results">The collection of service results.</param>
    /// <returns>A list of distinct error messages.</returns>
    public static List<string> CombineErrors(this IEnumerable<IServiceResult> results)
        => [.. results.Where(r => r.Errors != null).SelectMany(r => r.Errors!).Distinct()];

    /// <summary>
    /// Checks if all results in the collection are successful.
    /// </summary>
    /// <param name="results">The collection of service results.</param>
    /// <returns>True if all results are successful.</returns>
    public static bool AllSuccessful(this IEnumerable<IServiceResult> results)
        => results.All(r => r.IsSuccess);

    /// <summary>
    /// Gets the most severe error type from a collection of results.
    /// </summary>
    /// <param name="results">The collection of service results.</param>
    /// <returns>The most severe error type.</returns>
    public static ErrorType GetMostSevereErrorType(this IEnumerable<IServiceResult> results)
        => results.Where(r => r.IsFailure).Select(r => r.ErrorType).DefaultIfEmpty(ErrorType.None).Max();

    /// <summary>
    /// Asynchronously maps the result if successful.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNew">The target value type.</typeparam>
    /// <param name="resultTask">The task returning a ServiceResult.</param>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A task returning the mapped result.</returns>
    public static async Task<ServiceResult<TNew>> MapAsync<T, TNew>(this Task<ServiceResult<T>> resultTask, Func<T, TNew> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapper);
    }

    /// <summary>
    /// Asynchronously maps the result with an async mapper function.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNew">The target value type.</typeparam>
    /// <param name="resultTask">The task returning a ServiceResult.</param>
    /// <param name="mapper">The async mapping function.</param>
    /// <returns>A task returning the mapped result.</returns>
    public static async Task<ServiceResult<TNew>> MapAsync<T, TNew>(this Task<ServiceResult<T>> resultTask, Func<T, Task<TNew>> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure || result.Value is null)
            return ServiceResult<TNew>.Failure(result.ErrorMessage ?? "Operation failed", result.ErrorType, result.StatusCode);

        var newValue = await mapper(result.Value).ConfigureAwait(false);
        return ServiceResult<TNew>.Success(newValue);
    }

    /// <summary>
    /// Asynchronously binds the result to another result if successful.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNew">The target value type.</typeparam>
    /// <param name="resultTask">The task returning a ServiceResult.</param>
    /// <param name="binder">The binding function.</param>
    /// <returns>A task returning the bound result.</returns>
    public static async Task<ServiceResult<TNew>> BindAsync<T, TNew>(this Task<ServiceResult<T>> resultTask, Func<T, Task<ServiceResult<TNew>>> binder)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure || result.Value is null)
            return ServiceResult<TNew>.Failure(result.ErrorMessage ?? "Operation failed", result.ErrorType, result.StatusCode);

        return await binder(result.Value).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="resultTask">The task returning a ServiceResult.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>The original result.</returns>
    public static async Task<ServiceResult<T>> TapAsync<T>(this Task<ServiceResult<T>> resultTask, Func<T, Task> action)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess && result.Value is not null)
            await action(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="resultTask">The task returning a ServiceResult.</param>
    /// <param name="action">The action to execute on failure.</param>
    /// <returns>The original result.</returns>
    public static async Task<ServiceResult<T>> TapErrorAsync<T>(this Task<ServiceResult<T>> resultTask, Func<IServiceResult, Task> action)
    {
        var result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            await action(result).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Returns a fallback value if the result is a failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The service result.</param>
    /// <param name="defaultValue">The default value to return on failure.</param>
    /// <returns>The result value or the default value.</returns>
    public static T? GetValueOrDefault<T>(this ServiceResult<T> result, T? defaultValue = default)
        => result.IsSuccess ? result.Value : defaultValue;

    /// <summary>
    /// Throws an exception if the result is a failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The service result.</param>
    /// <returns>The result value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result represents a failure.</exception>
    public static T GetValueOrThrow<T>(this ServiceResult<T> result)
        => result.IsSuccess && result.Value is not null
            ? result.Value
            : throw new InvalidOperationException(result.Message ?? "Operation failed");

    /// <summary>
    /// Executes one of two functions depending on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="result">The service result.</param>
    /// <param name="onSuccess">The function to execute on success.</param>
    /// <param name="onFailure">The function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public static TResult Match<T, TResult>(this ServiceResult<T> result, Func<T, TResult> onSuccess, Func<IServiceResult, TResult> onFailure)
        => result.IsSuccess && result.Value is not null ? onSuccess(result.Value) : onFailure(result);

    /// <summary>
    /// Asynchronously executes one of two functions depending on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="resultTask">The task returning a ServiceResult.</param>
    /// <param name="onSuccess">The async function to execute on success.</param>
    /// <param name="onFailure">The async function to execute on failure.</param>
    /// <returns>A task returning the result of the executed function.</returns>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<ServiceResult<T>> resultTask,
        Func<T, Task<TResult>> onSuccess,
        Func<IServiceResult, Task<TResult>> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess && result.Value is not null
            ? await onSuccess(result.Value).ConfigureAwait(false)
            : await onFailure(result).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a synchronous action if the result is successful.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The service result.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>The original result.</returns>
    public static ServiceResult<T> Tap<T>(this ServiceResult<T> result, Action<T> action)
    {
        if (result.IsSuccess && result.Value is not null)
            action(result.Value);

        return result;
    }

    /// <summary>
    /// Executes a synchronous action if the result is a failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The service result.</param>
    /// <param name="action">The action to execute on failure.</param>
    /// <returns>The original result.</returns>
    public static ServiceResult<T> TapError<T>(this ServiceResult<T> result, Action<IServiceResult> action)
    {
        if (result.IsFailure)
            action(result);

        return result;
    }

    /// <summary>
    /// Converts a non-generic ServiceResult to a generic ServiceResult with a default value.
    /// </summary>
    /// <typeparam name="T">The target value type.</typeparam>
    /// <param name="result">The non-generic service result.</param>
    /// <param name="value">The value to use if successful.</param>
    /// <returns>A generic ServiceResult.</returns>
    public static ServiceResult<T> ToServiceResult<T>(this ServiceResult result, T value)
    {
        return result.IsSuccess
            ? ServiceResult<T>.Success(value)
            : ServiceResult<T>.Failure(result.ErrorMessage ?? "Operation failed", result.ErrorType, result.StatusCode);
    }
}
