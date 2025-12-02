using System.Net;
using Craft.Exceptions;
using Craft.Exceptions.Security;
using Craft.Security;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Middleware.RequestMiddleware;

/// <summary>
/// Global exception handler that implements IExceptionHandler for centralized error handling.
/// Uses RFC 7807 ProblemDetails format for standardized error responses.
/// Handles all CraftException types with proper status codes and validation error formatting.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IOptions<RequestMiddlewareSettings> settings,
    IWebHostEnvironment environment, ICurrentUser<Guid> currentUser) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly RequestMiddlewareSettings _settings = settings.Value;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly ICurrentUser<Guid> _currentUser = currentUser;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorId = Guid.NewGuid().ToString();
        var originalException = exception;

        // Try to get correlation ID from context items, use errorId as fallback
        var correlationId = context.Items.TryGetValue("CorrelationId", out var correlationIdValue)
            ? correlationIdValue?.ToString() ?? errorId
            : errorId;

        var unwrappedException = UnwrapException(exception);
        var statusCode = DetermineStatusCode(unwrappedException);

        // Check if response has already started before logging
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot write error response for {ErrorId}. Response has already started.", errorId);
            return true;
        }

        LogException(context, unwrappedException, originalException, errorId, statusCode);

        var problemDetails = CreateProblemDetails(context, unwrappedException, originalException, statusCode, errorId,
            correlationId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private Exception UnwrapException(Exception exception)
    {
        if (exception is CraftException)
            return exception;

        if (_settings.ExceptionHandling.IncludeInnerException)
        {
            var innerException = exception;
            while (innerException.InnerException != null)
                innerException = innerException.InnerException;

            return innerException;
        }

        return exception;
    }

    private static int DetermineStatusCode(Exception exception) =>
        exception switch
        {
            // CraftException types (uses StatusCode property from exception)
            CraftException craftEx => (int)craftEx.StatusCode,
            
            // Standard .NET exceptions
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            OperationCanceledException => (int)HttpStatusCode.RequestTimeout,
            
            _ => (int)HttpStatusCode.InternalServerError
        };

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception unwrappedException,
        Exception originalException, int statusCode, string errorId, string correlationId)
    {
        // Handle ModelValidationException specially with ValidationProblemDetails
        if (unwrappedException is ModelValidationException validationException && validationException.ValidationErrors.Count > 0)
            return CreateValidationProblemDetails(context, validationException, statusCode, errorId, correlationId, originalException);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetErrorTitle(statusCode, unwrappedException),
            Detail = unwrappedException.Message.Trim(),
            Instance = context.Request.Path,
            Type = GetErrorTypeUrl(statusCode)
        };

        AddDiagnosticExtensions(problemDetails, errorId, correlationId);
        AddCraftExceptionErrors(problemDetails, unwrappedException);
        AddDevelopmentExtensions(problemDetails, originalException);

        return problemDetails;
    }

    private ValidationProblemDetails CreateValidationProblemDetails(HttpContext context, ModelValidationException validationException,
        int statusCode, string errorId, string correlationId, Exception originalException)
    {
        var problemDetails = new ValidationProblemDetails(validationException.ValidationErrors)
        {
            Status = statusCode,
            Title = GetErrorTitle(statusCode, validationException),
            Detail = validationException.Message.Trim(),
            Instance = context.Request.Path,
            Type = GetErrorTypeUrl(statusCode)
        };

        AddDiagnosticExtensions(problemDetails, errorId, correlationId);
        
        if (validationException.Errors.Count > 0)
            problemDetails.Extensions["errors"] = validationException.Errors;

        AddDevelopmentExtensions(problemDetails, originalException);

        return problemDetails;
    }

    private void AddDiagnosticExtensions(ProblemDetails problemDetails, string errorId, string correlationId)
    {
        if (!_settings.ExceptionHandling.IncludeDiagnostics)
            return;

        problemDetails.Extensions["errorId"] = errorId;
        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        if (_currentUser.IsAuthenticated())
        {
            problemDetails.Extensions["userId"] = _currentUser.GetId();
            problemDetails.Extensions["userEmail"] = _currentUser.GetEmail();

            var tenant = _currentUser.GetTenant();
            if (!string.IsNullOrEmpty(tenant))
                problemDetails.Extensions["tenant"] = tenant;
        }
    }

    private static void AddCraftExceptionErrors(ProblemDetails problemDetails, Exception exception)
    {
        if (exception is CraftException { Errors.Count: > 0 } craftException)
            problemDetails.Extensions["errors"] = craftException.Errors;
    }

    private void AddDevelopmentExtensions(ProblemDetails problemDetails, Exception originalException)
    {
        if (!_settings.ExceptionHandling.IncludeStackTrace || !_environment.IsDevelopment())
            return;

        problemDetails.Extensions["stackTrace"] = originalException.StackTrace;
        problemDetails.Extensions["exceptionType"] = originalException.GetType().FullName;

        if (_settings.ExceptionHandling.IncludeInnerException && originalException.InnerException != null)
        {
            problemDetails.Extensions["innerException"] = originalException.InnerException.Message;
            problemDetails.Extensions["innerExceptionType"] = originalException.InnerException.GetType().FullName;
        }
    }

    private void LogException(HttpContext context, Exception unwrappedException, Exception originalException,
        string errorId, int statusCode)
    {
        var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;

        var userId = _currentUser.IsAuthenticated() ? _currentUser.GetId().ToString() : "Anonymous";
        var userEmail = _currentUser.GetEmail() ?? "Anonymous";
        var tenant = _currentUser.GetTenant() ?? "N/A";

        _logger.Log(logLevel, originalException,
            "Request failed with status {StatusCode} | ErrorId: {ErrorId} | Path: {Path} | Method: {Method} | User: {UserEmail} ({UserId}) | Tenant: {Tenant} | Message: {Message}",
            statusCode, errorId, context.Request.Path, context.Request.Method, userEmail, userId, tenant,
            unwrappedException.Message);
    }

    private static string GetErrorTitle(int statusCode, Exception? exception = null)
    {
        // Use custom titles for specific exception types
        if (exception != null)
        {
            return exception switch
            {
                ModelValidationException => "One or more validation errors occurred",
                NotFoundException => "Resource not found",
                AlreadyExistsException => "Resource already exists",
                InvalidCredentialsException => "Invalid credentials",
                UnauthorizedException => "Unauthorized access",
                ForbiddenException => "Access forbidden",
                _ => GetDefaultTitle(statusCode)
            };
        }

        return GetDefaultTitle(statusCode);
    }

    private static string GetDefaultTitle(int statusCode) =>
        statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            408 => "Request Timeout",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            501 => "Not Implemented",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => "An error occurred"
        };

    private static string GetErrorTypeUrl(int statusCode) =>
        statusCode switch
        {
            400 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
            401 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2",
            403 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.4",
            404 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
            408 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.9",
            409 => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10",
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
