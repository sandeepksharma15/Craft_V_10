namespace Craft.QuerySpec;

/// <summary>
/// Defines a contract for collecting and reporting query execution metrics.
/// </summary>
public interface IQueryMetrics
{
    /// <summary>
    /// Records a successful query execution.
    /// </summary>
    /// <param name="queryType">The type of query executed (e.g., "GetAll", "GetPaged").</param>
    /// <param name="entityType">The entity type being queried.</param>
    /// <param name="duration">The execution duration.</param>
    /// <param name="resultCount">The number of results returned.</param>
    void RecordQueryExecution(string queryType, string entityType, TimeSpan duration, int resultCount);

    /// <summary>
    /// Records a query execution error.
    /// </summary>
    /// <param name="queryType">The type of query that failed.</param>
    /// <param name="entityType">The entity type being queried.</param>
    /// <param name="exception">The exception that occurred.</param>
    void RecordQueryError(string queryType, string entityType, Exception exception);

    /// <summary>
    /// Records a query validation failure.
    /// </summary>
    /// <param name="entityType">The entity type being queried.</param>
    /// <param name="validationErrors">The validation errors.</param>
    void RecordValidationFailure(string entityType, IEnumerable<string> validationErrors);
}
