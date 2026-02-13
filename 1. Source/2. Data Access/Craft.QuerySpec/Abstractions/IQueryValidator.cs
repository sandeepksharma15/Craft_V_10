namespace Craft.QuerySpec;

/// <summary>
/// Defines a contract for validating query specifications.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
public interface IQueryValidator<T> where T : class
{
    /// <summary>
    /// Validates the specified query specification.
    /// </summary>
    /// <param name="query">The query to validate.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A validation result indicating success or failure with error messages.</returns>
    Task<ValidationResult> ValidateAsync(IQuery<T> query, CancellationToken cancellationToken = default);
}
