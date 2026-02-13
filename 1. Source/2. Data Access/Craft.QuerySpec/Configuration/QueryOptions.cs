namespace Craft.QuerySpec;

/// <summary>
/// Configuration options for query execution and validation.
/// </summary>
public class QueryOptions
{
    /// <summary>
    /// Maximum time in seconds allowed for query execution. Default is 30 seconds.
    /// </summary>
    /// <remarks>
    /// Set to 0 to disable timeout (not recommended for production).
    /// </remarks>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of results that can be returned from a single query. Default is 10,000.
    /// </summary>
    /// <remarks>
    /// Set to 0 to disable limit (not recommended for production).
    /// Helps prevent memory exhaustion from large result sets.
    /// </remarks>
    public int MaxResultSize { get; set; } = 10000;

    /// <summary>
    /// Maximum number of filters allowed in a single query. Default is 50.
    /// </summary>
    /// <remarks>
    /// Helps prevent overly complex queries that could impact performance.
    /// </remarks>
    public int MaxFilterCount { get; set; } = 50;

    /// <summary>
    /// Maximum number of navigation properties that can be included. Default is 10.
    /// </summary>
    /// <remarks>
    /// Deep includes can cause cartesian explosion and performance issues.
    /// </remarks>
    public int MaxIncludeCount { get; set; } = 10;

    /// <summary>
    /// Maximum page size for paginated queries. Default is 1,000.
    /// </summary>
    /// <remarks>
    /// Prevents clients from requesting excessively large pages.
    /// </remarks>
    public int MaxPageSize { get; set; } = 1000;

    /// <summary>
    /// Maximum number of order by fields allowed. Default is 5.
    /// </summary>
    /// <remarks>
    /// Complex sorting can impact query performance.
    /// </remarks>
    public int MaxOrderByFields { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether query result caching is enabled. Default is false.
    /// </summary>
    /// <remarks>
    /// When enabled, query results can be cached based on query specifications.
    /// Requires cache implementation to be registered in DI.
    /// </remarks>
    public bool EnableQueryCaching { get; set; }

    /// <summary>
    /// Default cache expiration time for query results. Default is 5 minutes.
    /// </summary>
    /// <remarks>
    /// Only used when EnableQueryCaching is true.
    /// </remarks>
    public TimeSpan DefaultCacheExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether query metrics collection is enabled. Default is true.
    /// </summary>
    /// <remarks>
    /// Collects execution time, result counts, and error rates for monitoring.
    /// </remarks>
    public bool EnableQueryMetrics { get; set; } = true;

    /// <summary>
    /// Threshold in milliseconds for logging slow queries. Default is 5,000ms (5 seconds).
    /// </summary>
    /// <remarks>
    /// Queries exceeding this threshold will be logged as warnings.
    /// </remarks>
    public int SlowQueryThresholdMs { get; set; } = 5000;
}
