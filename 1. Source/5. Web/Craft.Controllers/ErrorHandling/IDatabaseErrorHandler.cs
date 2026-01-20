namespace Craft.Controllers.ErrorHandling;

/// <summary>
/// Service for handling database exceptions and converting them to user-friendly error messages.
/// </summary>
public interface IDatabaseErrorHandler
{
    /// <summary>
    /// Handles a database exception and returns a user-friendly error message.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="entityType">The entity type being operated on.</param>
    /// <returns>A user-friendly error message.</returns>
    string HandleException(Exception exception, Type entityType);
}
