using System.Text;
using Craft.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Middleware.RequestMiddleware;

/// <summary>
/// Middleware for detailed response logging with proper stream buffering.
/// </summary>
public class ResponseLoggingMiddleware(ILogger<ResponseLoggingMiddleware> logger, IOptions<RequestMiddlewareSettings> settings,
    ICurrentUser<Guid> currentUser) : IMiddleware
{
    private readonly ILogger<ResponseLoggingMiddleware> _logger = logger;
    private readonly LoggingSettings _loggingSettings = settings.Value.Logging;
    private readonly ICurrentUser<Guid> _currentUser = currentUser;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (ShouldSkipLogging(context.Request.Path))
        {
            await next(context);
            return;
        }

        if (!_loggingSettings.LogResponseBody)
        {
            await next(context);
            LogResponseMetadata(context);
            return;
        }

        var originalBodyStream = context.Response.Body;

        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);

            var responseBodyText = await GetResponseBodyAsync(context, responseBody);

            LogResponse(context, responseBodyText);

            await CopyResponseBodyAsync(responseBody, originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<string?> GetResponseBodyAsync(HttpContext context, MemoryStream responseBody)
    {
        if (responseBody.Length == 0)
            return null;

        if (IsSensitivePath(context.Request.Path))
            return "[REDACTED - Sensitive endpoint]";

        var contentType = context.Response.ContentType ?? string.Empty;

        if (!contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) &&
            !contentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase) &&
            !contentType.Contains("text/", StringComparison.OrdinalIgnoreCase))
            return $"[Binary content: {contentType}]";

        try
        {
            responseBody.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
            var text = await reader.ReadToEndAsync();

            if (text.Length > _loggingSettings.MaxResponseBodyLength)
                return $"{text[.._loggingSettings.MaxResponseBodyLength]}... [TRUNCATED]";

            return text;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read response body");
            return "[ERROR reading body]";
        }
        finally
        {
            responseBody.Seek(0, SeekOrigin.Begin);
        }
    }

    private static async Task CopyResponseBodyAsync(MemoryStream responseBody, Stream originalBodyStream)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private void LogResponse(HttpContext context, string? responseBody)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
        var userEmail = _currentUser.GetEmail() ?? "Anonymous";
        var userId = _currentUser.IsAuthenticated() ? _currentUser.GetId().ToString() : "Anonymous";
        var tenant = _currentUser.GetTenant() ?? "N/A";

        var logData = new
        {
            CorrelationId = correlationId,
            context.Response.StatusCode,
            context.Response.ContentType,
            context.Response.ContentLength,
            Headers = _loggingSettings.LogHeaders ? FilterHeaders(context.Response.Headers) : null,
            Body = responseBody,
            User = new
            {
                Id = userId,
                Email = userEmail,
                Tenant = tenant
            }
        };

        var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(logLevel,
            "Response sent | StatusCode: {StatusCode} | Path: {Path} | User: {UserEmail} | CorrelationId: {CorrelationId}",
            context.Response.StatusCode, context.Request.Path, userEmail, correlationId);

        _logger.LogDebug("Response details: {@ResponseData}", logData);
    }

    private void LogResponseMetadata(HttpContext context)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
        var userEmail = _currentUser.GetEmail() ?? "Anonymous";

        var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(logLevel,
            "Response sent | StatusCode: {StatusCode} | Path: {Path} | User: {UserEmail} | CorrelationId: {CorrelationId}",
            context.Response.StatusCode, context.Request.Path, userEmail, correlationId);
    }

    private Dictionary<string, string> FilterHeaders(IHeaderDictionary headers)
    {
        var filteredHeaders = new Dictionary<string, string>();

        foreach (var header in headers)
        {
            if (_loggingSettings.SensitiveHeaders.Any(s =>
                header.Key.Contains(s, StringComparison.OrdinalIgnoreCase)))
                filteredHeaders[header.Key] = "[REDACTED]";
            else
                filteredHeaders[header.Key] = header.Value.ToString();
        }

        return filteredHeaders;
    }

    private bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;

        return _loggingSettings.ExcludedPaths.Any(excluded =>
            pathValue.Contains(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsSensitivePath(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;

        return _loggingSettings.SensitivePaths.Any(sensitive =>
            pathValue.Contains(sensitive, StringComparison.OrdinalIgnoreCase));
    }
}
