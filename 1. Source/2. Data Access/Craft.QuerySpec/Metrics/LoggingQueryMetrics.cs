using Microsoft.Extensions.Logging;

namespace Craft.QuerySpec;

/// <summary>
/// Default implementation of query metrics that logs metrics using ILogger.
/// </summary>
/// <remarks>
/// For production use, consider implementing a custom IQueryMetrics that sends metrics
/// to your monitoring system (Application Insights, Prometheus, etc.).
/// </remarks>
public class LoggingQueryMetrics : IQueryMetrics
{
    private readonly ILogger<LoggingQueryMetrics> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingQueryMetrics"/> class.
    /// </summary>
    /// <param name="logger">Logger for metrics output.</param>
    public LoggingQueryMetrics(ILogger<LoggingQueryMetrics> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public void RecordQueryExecution(string queryType, string entityType, TimeSpan duration, int resultCount)
    {
        _logger.LogInformation(
            "[QueryMetrics] Query executed: Type={QueryType}, Entity={EntityType}, Duration={DurationMs}ms, Results={ResultCount}",
            queryType, entityType, duration.TotalMilliseconds, resultCount);
    }

    /// <inheritdoc />
    public void RecordQueryError(string queryType, string entityType, Exception exception)
    {
        _logger.LogError(
            exception,
            "[QueryMetrics] Query failed: Type={QueryType}, Entity={EntityType}, Error={ErrorMessage}",
            queryType, entityType, exception.Message);
    }

    /// <inheritdoc />
    public void RecordValidationFailure(string entityType, IEnumerable<string> validationErrors)
    {
        _logger.LogWarning(
            "[QueryMetrics] Query validation failed: Entity={EntityType}, Errors={ValidationErrors}",
            entityType, string.Join("; ", validationErrors));
    }
}
