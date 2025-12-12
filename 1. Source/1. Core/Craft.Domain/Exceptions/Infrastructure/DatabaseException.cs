using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when a database operation fails.
/// This includes connection failures, query execution errors,
/// transaction failures, and database constraint violations.
/// </summary>
public class DatabaseException : CraftException
{
    public DatabaseException()
        : base("A database error occurred", [], HttpStatusCode.InternalServerError) { }

    public DatabaseException(string message)
        : base(message, [], HttpStatusCode.InternalServerError) { }

    public DatabaseException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.InternalServerError) { }

    public DatabaseException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.InternalServerError) { }

    public DatabaseException(string operation, string details)
        : base($"Database error during {operation}: {details}", [], HttpStatusCode.InternalServerError) { }
}
