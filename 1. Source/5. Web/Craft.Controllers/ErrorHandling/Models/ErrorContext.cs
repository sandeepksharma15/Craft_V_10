namespace Craft.Controllers.ErrorHandling.Models;

/// <summary>
/// Encapsulates error information for database error handling strategies.
/// </summary>
public sealed class ErrorContext
{
    /// <summary>
    /// The original exception that occurred.
    /// </summary>
    public required Exception Exception { get; init; }

    /// <summary>
    /// The humanized entity name (e.g., "Location", "Customer Order").
    /// </summary>
    public required string EntityName { get; init; }

    /// <summary>
    /// The extracted field name from constraint, if available.
    /// </summary>
    public string? FieldName { get; init; }

    /// <summary>
    /// PostgreSQL SQL state code (e.g., "23505").
    /// </summary>
    public string? SqlState { get; init; }

    /// <summary>
    /// SQL Server error number (e.g., 2601).
    /// </summary>
    public int? ErrorNumber { get; init; }

    /// <summary>
    /// Database constraint name (e.g., "IX_Locations_Name").
    /// </summary>
    public string? ConstraintName { get; init; }

    /// <summary>
    /// The full error message text from the database.
    /// </summary>
    public required string MessageText { get; init; }

    /// <summary>
    /// The exception type name for logging purposes.
    /// </summary>
    public string ExceptionTypeName => Exception.GetType().Name;
}
