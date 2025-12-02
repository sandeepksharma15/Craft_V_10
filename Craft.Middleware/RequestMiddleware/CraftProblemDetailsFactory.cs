using Craft.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Craft.Middleware.RequestMiddleware;

/// <summary>
/// Custom ProblemDetails factory that enriches error responses with diagnostic information,
/// correlation tracking, and user context following RFC 7807 standards.
/// </summary>
public class CraftProblemDetailsFactory(IOptions<RequestMiddlewareSettings> settings, ICurrentUser<Guid> currentUser,
    ProblemDetailsFactory? innerFactory = null) : ProblemDetailsFactory
{
    private readonly RequestMiddlewareSettings _settings = settings.Value;
    private readonly ICurrentUser<Guid> _currentUser = currentUser;
    private readonly ProblemDetailsFactory? _innerFactory = innerFactory;

    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> instance with enhanced diagnostic information.
    /// </summary>
    public override ProblemDetails CreateProblemDetails(HttpContext httpContext, int? statusCode = null,
        string? title = null, string? type = null, string? detail = null, string? instance = null)
    {
        statusCode ??= 500;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title ?? GetDefaultTitle(statusCode.Value),
            Type = type ?? GetTypeUrl(statusCode.Value),
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path
        };

        EnrichProblemDetails(httpContext, problemDetails);

        return problemDetails;
    }

    /// <summary>
    /// Creates a <see cref="ValidationProblemDetails"/> instance with enhanced diagnostic information.
    /// </summary>
    public override ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext,
        ModelStateDictionary modelStateDictionary, int? statusCode = null, string? title = null, string? type = null,
        string? detail = null, string? instance = null)
    {
        // Use configured validation status code if not explicitly provided
        statusCode ??= _settings.ModelValidationStatusCode;

        var problemDetails = new ValidationProblemDetails(modelStateDictionary)
        {
            Status = statusCode,
            Title = title ?? GetValidationTitle(statusCode.Value),
            Type = type ?? GetTypeUrl(statusCode.Value),
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path
        };

        EnrichProblemDetails(httpContext, problemDetails);

        return problemDetails;
    }

    /// <summary>
    /// Enriches ProblemDetails with diagnostic information, correlation tracking, and user context.
    /// </summary>
    private void EnrichProblemDetails(HttpContext httpContext, ProblemDetails problemDetails)
    {
        if (!_settings.ExceptionHandling.IncludeDiagnostics)
            return;

        // Add correlation ID if available
        if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
            problemDetails.Extensions["correlationId"] = correlationId?.ToString();

        // Add timestamp
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        // Add request ID (trace identifier) - only if not null or empty
        if (!string.IsNullOrWhiteSpace(httpContext.TraceIdentifier))
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        // Add user context if authenticated
        if (_currentUser.IsAuthenticated())
        {
            problemDetails.Extensions["userId"] = _currentUser.GetId();
            problemDetails.Extensions["userEmail"] = _currentUser.GetEmail();

            var tenant = _currentUser.GetTenant();
            if (!string.IsNullOrEmpty(tenant))
                problemDetails.Extensions["tenant"] = tenant;
        }

        // Add request path and method for context
        problemDetails.Extensions["path"] = httpContext.Request.Path.ToString();
        problemDetails.Extensions["method"] = httpContext.Request.Method;
    }

    /// <summary>
    /// Gets the default title for a given status code.
    /// </summary>
    private static string GetDefaultTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        405 => "Method Not Allowed",
        406 => "Not Acceptable",
        408 => "Request Timeout",
        409 => "Conflict",
        410 => "Gone",
        415 => "Unsupported Media Type",
        422 => "Unprocessable Entity",
        429 => "Too Many Requests",
        500 => "Internal Server Error",
        501 => "Not Implemented",
        502 => "Bad Gateway",
        503 => "Service Unavailable",
        504 => "Gateway Timeout",
        _ => "An error occurred"
    };

    /// <summary>
    /// Gets a validation-specific title based on status code.
    /// </summary>
    private static string GetValidationTitle(int statusCode) => statusCode switch
    {
        422 => "One or more validation errors occurred",
        400 => "Bad Request - Validation Failed",
        _ => "Validation errors occurred"
    };

    /// <summary>
    /// Gets the RFC type URL for a given status code.
    /// </summary>
    private static string GetTypeUrl(int statusCode) => statusCode switch
    {
        400 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
        401 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2",
        403 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.4",
        404 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
        405 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.6",
        406 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.7",
        408 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.9",
        409 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10",
        410 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.11",
        415 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.16",
        422 => "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2",
        429 => "https://datatracker.ietf.org/doc/html/rfc6585#section-4",
        500 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
        501 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.2",
        502 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.3",
        503 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.4",
        504 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.5",
        _ when statusCode >= 500 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6",
        _ => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5"
    };
}
