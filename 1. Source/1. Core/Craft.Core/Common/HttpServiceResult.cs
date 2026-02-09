using System;
using System.Text.Json;

namespace Craft.Core.Common;

/// <summary>
/// Represents the result of an HTTP service operation.
/// </summary>
public record HttpServiceResult<T> : IServiceResult
{
    public T? Data { get; init; }
    public bool IsSuccess { get; init; }
    public List<string>? Errors { get; init; }
    public int? StatusCode { get; init; }

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
    public static HttpServiceResult<T> FailureResult(List<string>? errors = null, int? statusCode = 400)
        => new() { IsSuccess = false, Errors = errors, StatusCode = statusCode };
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
            StatusCode = statusCode
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
            StatusCode = result.StatusCode
        };
    }

    /// <summary>
    /// Converts a Result<T> to HttpServiceResult<T>.
    /// </summary>
    public static HttpServiceResult<T> ToHttpServiceResult<T>(this Result<T> result, int? statusCode = null)
    {
        return new HttpServiceResult<T>
        {
            IsSuccess = result.IsSuccess,
            Data = result.Value,
            Errors = result.Errors,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Returns the first error message or null.
    /// </summary>
    public static string? FirstError(this IServiceResult result)
    {
        return result.Errors?.Count > 0 ? result.Errors[0] : null;
    }
}
