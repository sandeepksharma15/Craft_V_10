namespace Craft.Middleware.RequestMiddleware;

/// <summary>
/// Configuration settings for request/response middleware.
/// </summary>
public class RequestMiddlewareSettings
{
    /// <summary>
    /// Enables global exception handling using IExceptionHandler.
    /// </summary>
    public bool EnableExceptionHandler { get; set; } = true;

    /// <summary>
    /// Enables Serilog's built-in request logging with enrichment.
    /// </summary>
    public bool EnableSerilogRequestLogging { get; set; } = true;

    /// <summary>
    /// Enables custom request/response logging middleware.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// HTTP status code to return for model validation errors.
    /// Default is 422 (Unprocessable Entity) which is semantically correct for validation failures.
    /// Set to 400 for traditional Bad Request behavior.
    /// </summary>
    public int ModelValidationStatusCode { get; set; } = 422;

    /// <summary>
    /// Configuration for request/response logging behavior.
    /// </summary>
    public LoggingSettings Logging { get; set; } = new();

    /// <summary>
    /// Configuration for exception handling behavior.
    /// </summary>
    public ExceptionHandlingSettings ExceptionHandling { get; set; } = new();
}

/// <summary>
/// Configuration for logging middleware.
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// Enable logging of request bodies.
    /// </summary>
    public bool LogRequestBody { get; set; } = true;

    /// <summary>
    /// Enable logging of response bodies (has performance impact).
    /// </summary>
    public bool LogResponseBody { get; set; } = false;

    /// <summary>
    /// Enable logging of request/response headers.
    /// </summary>
    public bool LogHeaders { get; set; } = true;

    /// <summary>
    /// Enable performance metrics (request duration).
    /// </summary>
    public bool LogPerformanceMetrics { get; set; } = true;

    /// <summary>
    /// Paths to exclude from logging (e.g., health checks).
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = ["/health", "/metrics", "/favicon.ico"];

    /// <summary>
    /// Path patterns that contain sensitive data to redact.
    /// </summary>
    public List<string> SensitivePaths { get; set; } = ["/token", "/auth", "/password", "/secret"];

    /// <summary>
    /// Request headers to redact from logs.
    /// </summary>
    public List<string> SensitiveHeaders { get; set; } = ["Authorization", "Cookie", "X-API-Key", "X-Auth-Token"];

    /// <summary>
    /// Maximum request body size to log (in bytes). Larger bodies will be truncated.
    /// </summary>
    public int MaxRequestBodyLength { get; set; } = 4096;

    /// <summary>
    /// Maximum response body size to log (in bytes). Larger bodies will be truncated.
    /// </summary>
    public int MaxResponseBodyLength { get; set; } = 4096;
}

/// <summary>
/// Configuration for exception handling.
/// </summary>
public class ExceptionHandlingSettings
{
    /// <summary>
    /// Include exception stack trace in development environment.
    /// Only applies when environment is Development.
    /// </summary>
    public bool IncludeStackTrace { get; set; } = false;

    /// <summary>
    /// Include inner exception details when unwrapping exceptions.
    /// </summary>
    public bool IncludeInnerException { get; set; } = true;

    /// <summary>
    /// Use RFC 7807 ProblemDetails format for error responses.
    /// Should always be true for standards compliance.
    /// </summary>
    public bool UseProblemDetails { get; set; } = true;

    /// <summary>
    /// Include additional diagnostic information (errorId, correlationId, timestamp, user info).
    /// Recommended for production debugging.
    /// </summary>
    public bool IncludeDiagnostics { get; set; } = true;

    /// <summary>
    /// Include detailed validation errors in ValidationProblemDetails.
    /// When true, ModelValidationException will return structured validation errors.
    /// </summary>
    public bool IncludeValidationDetails { get; set; } = true;
}
