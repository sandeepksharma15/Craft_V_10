using Craft.Core.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Craft.Core.Extensions;

/// <summary>
/// Extension methods for converting IServiceResult to IActionResult.
/// </summary>
public static class ActionResultExtensions
{
    /// <summary>
    /// Converts an IServiceResult to an appropriate IActionResult.
    /// </summary>
    /// <param name="result">The service result to convert.</param>
    /// <returns>An IActionResult with the appropriate status code and content.</returns>
    public static IActionResult ToActionResult(this IServiceResult result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return CreateErrorResult(result);
    }

    /// <summary>
    /// Converts a ServiceResult&lt;T&gt; to an appropriate IActionResult with data.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult with the appropriate status code and content.</returns>
    public static IActionResult ToActionResult<T>(this ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return CreateErrorResult(result);
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to an appropriate IActionResult with data.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult with the appropriate status code and content.</returns>
    [Obsolete("Use ServiceResult<T>.ToActionResult() instead.")]
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return CreateErrorResult(result);
    }

    /// <summary>
    /// Converts an IServiceResult to an ActionResult&lt;T&gt; for typed controller actions.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="result">The service result to convert.</param>
    /// <returns>An ActionResult&lt;T&gt; with the appropriate status code and content.</returns>
    public static ActionResult<T> ToTypedActionResult<T>(this IServiceResult result)
    {
        if (result.IsSuccess)
        {
            if (result is ServiceResult<T> typedResult)
                return typedResult.Value!;

            if (result is Result<T> legacyResult)
                return legacyResult.Value!;

            return new OkResult();
        }

        return CreateErrorResult(result);
    }

    /// <summary>
    /// Converts a ServiceResult&lt;T&gt; to a ServerResponse&lt;T&gt; and wraps it in an IActionResult.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult containing a ServerResponse&lt;T&gt;.</returns>
    public static IActionResult ToServerResponseResult<T>(this ServiceResult<T> result)
    {
        var response = ServerResponse<T>.FromServiceResult(result);
        return new ObjectResult(response) { StatusCode = response.StatusCode };
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to a ServerResponse&lt;T&gt; and wraps it in an IActionResult.
    /// </summary>
    [Obsolete("Use ServiceResult<T>.ToServerResponseResult() instead.")]
    public static IActionResult ToServerResponseResult<T>(this Result<T> result)
    {
        var response = ServerResponse<T>.FromResult(result);
        return new ObjectResult(response) { StatusCode = response.StatusCode };
    }

    /// <summary>
    /// Converts an IServiceResult to a ServerResponse and wraps it in an IActionResult.
    /// </summary>
    /// <param name="result">The service result to convert.</param>
    /// <returns>An IActionResult containing a ServerResponse.</returns>
    public static IActionResult ToServerResponseResult(this IServiceResult result)
    {
        var response = ServerResponse.FromResult(result);
        return new ObjectResult(response) { StatusCode = response.StatusCode };
    }

    /// <summary>
    /// Returns an OkObjectResult if successful, otherwise returns a problem details response.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult with problem details on failure.</returns>
    public static IActionResult ToActionResultWithProblemDetails<T>(this ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return CreateProblemDetailsResult(result);
    }

    /// <summary>
    /// Returns an OkObjectResult if successful, otherwise returns a problem details response.
    /// </summary>
    [Obsolete("Use ServiceResult<T>.ToActionResultWithProblemDetails() instead.")]
    public static IActionResult ToActionResultWithProblemDetails<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return CreateProblemDetailsResult(result);
    }

    private static ObjectResult CreateErrorResult(IServiceResult result)
    {
        var statusCode = result.StatusCode ?? GetDefaultStatusCode(result.ErrorType);

        var errorResponse = new
        {
            result.IsSuccess,
            result.Errors,
            result.Message,
            result.ErrorType
        };

        return new ObjectResult(errorResponse) { StatusCode = statusCode };
    }

    private static ObjectResult CreateProblemDetailsResult(IServiceResult result)
    {
        var statusCode = result.StatusCode ?? GetDefaultStatusCode(result.ErrorType);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetProblemTitle(result.ErrorType),
            Detail = result.Message,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        if (result.Errors?.Count > 0)
            problemDetails.Extensions["errors"] = result.Errors;

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }

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

    private static string GetProblemTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => "Resource Not Found",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Validation => "Validation Error",
        ErrorType.Conflict => "Conflict",
        ErrorType.Internal => "Internal Server Error",
        ErrorType.Timeout => "Request Timeout",
        _ => "Bad Request"
    };
}
