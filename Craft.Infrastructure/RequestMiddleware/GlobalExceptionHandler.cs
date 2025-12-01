using System.Net;
using Craft.Exceptions;
using Craft.Security;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Infrastructure.RequestMiddleware;

/// <summary>
/// Global exception handler that implements IExceptionHandler for centralized error handling.
/// Uses RFC 7807 ProblemDetails format for standardized error responses.
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

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? errorId;

        var unwrappedException = UnwrapException(exception);
        var statusCode = DetermineStatusCode(unwrappedException);

        LogException(context, unwrappedException, originalException, errorId, statusCode);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot write error response for {ErrorId}. Response has already started.", errorId);
            return true;
        }

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
            CraftException craftEx => (int)craftEx.StatusCode,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            _ => (int)HttpStatusCode.InternalServerError
        };

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception unwrappedException,
        Exception originalException, int statusCode, string errorId, string correlationId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetErrorTitle(statusCode),
            Detail = unwrappedException.Message.Trim(),
            Instance = context.Request.Path,
            Type = GetErrorTypeUrl(statusCode)
        };

        if (_settings.ExceptionHandling.IncludeDiagnostics)
        {
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

        if (unwrappedException is CraftException craftException && craftException.Errors.Count > 0)
            problemDetails.Extensions["errors"] = craftException.Errors;

        if (_settings.ExceptionHandling.IncludeStackTrace && _environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = originalException.StackTrace;
            problemDetails.Extensions["exceptionType"] = originalException.GetType().FullName;

            if (_settings.ExceptionHandling.IncludeInnerException && originalException.InnerException != null)
                problemDetails.Extensions["innerException"] = originalException.InnerException.Message;
        }

        return problemDetails;
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

    private static string GetErrorTitle(int statusCode) =>
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
            _ => "An error occurred"
        };

    private static string GetErrorTypeUrl(int statusCode)
    {
        var section = statusCode >= 500 ? "6" : "5";
        return $"https://datatracker.ietf.org/doc/html/rfc7231#section-6.{section}";
    }
}
