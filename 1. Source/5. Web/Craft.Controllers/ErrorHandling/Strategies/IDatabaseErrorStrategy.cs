using Craft.Controllers.ErrorHandling.Models;

namespace Craft.Controllers.ErrorHandling.Strategies;

/// <summary>
/// Defines the contract for database-specific error handling strategies.
/// Implementations provide error messages tailored to specific database providers.
/// </summary>
public interface IDatabaseErrorStrategy
{
    /// <summary>
    /// Determines whether this strategy can handle the given error context.
    /// </summary>
    /// <param name="context">The error context containing exception and metadata.</param>
    /// <returns>True if this strategy can handle the error; otherwise, false.</returns>
    bool CanHandle(ErrorContext context);

    /// <summary>
    /// Gets a user-friendly error message based on the error context.
    /// </summary>
    /// <param name="context">The error context containing exception and metadata.</param>
    /// <returns>A user-friendly error message.</returns>
    string GetErrorMessage(ErrorContext context);

    /// <summary>
    /// Gets the priority of this strategy. Lower values have higher priority.
    /// Used to determine the order in which strategies are evaluated.
    /// </summary>
    int Priority { get; }
}
