using System;
using System.Collections.Generic;

namespace Craft.Core.Common;

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

    /// <summary>
    /// Returns true if the result has any errors.
    /// </summary>
    public static bool HasAnyErrors(this IServiceResult result)
    {
        return result.Errors is { Count: > 0 };
    }
}
