using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Infrastructure.RequestMiddleware;

/// <summary>
/// Middleware for detailed request logging with configurable sensitivity filtering.
/// </summary>
public class RequestLoggingMiddleware(
    ILogger<RequestLoggingMiddleware> logger,
    IOptions<SystemSettings> settings) : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger = logger;
    private readonly LoggingSettings _loggingSettings = settings.Value.Logging;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (ShouldSkipLogging(context.Request.Path))
        {
            await next(context);
            return;
        }

        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        var startTime = Stopwatch.GetTimestamp();

        await LogRequestAsync(context, correlationId);

        await next(context);

        if (_loggingSettings.LogPerformanceMetrics)
        {
            var elapsedMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;

            _logger.LogInformation(
                "Request completed | Method: {Method} | Path: {Path} | StatusCode: {StatusCode} | Duration: {Duration}ms | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsedMs,
                correlationId);
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;

        var logData = new
        {
            CorrelationId = correlationId,
            Method = request.Method,
            Path = request.Path.ToString(),
            QueryString = request.QueryString.ToString(),
            Scheme = request.Scheme,
            Host = request.Host.ToString(),
            Protocol = request.Protocol,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            Headers = _loggingSettings.LogHeaders ? FilterHeaders(request.Headers) : null,
            Body = await GetRequestBodyAsync(context)
        };

        _logger.LogInformation(
            "Incoming request | {Method} {Path} | CorrelationId: {CorrelationId}",
            request.Method,
            request.Path,
            correlationId);

        _logger.LogDebug("Request details: {@RequestData}", logData);
    }

    private async Task<string?> GetRequestBodyAsync(HttpContext context)
    {
        if (!_loggingSettings.LogRequestBody)
            return null;

        var request = context.Request;

        if (string.IsNullOrEmpty(request.ContentType))
            return null;

        if (!request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) &&
            !request.ContentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase) &&
            !request.ContentType.Contains("text/", StringComparison.OrdinalIgnoreCase))
            return $"[Binary content: {request.ContentType}]";

        if (IsSensitivePath(request.Path))
            return "[REDACTED - Sensitive endpoint]";

        try
        {
            request.EnableBuffering();

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 4096,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();

            request.Body.Position = 0;

            if (body.Length > _loggingSettings.MaxRequestBodyLength)
                return $"{body[.._loggingSettings.MaxRequestBodyLength]}... [TRUNCATED]";

            return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read request body");
            return "[ERROR reading body]";
        }
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
