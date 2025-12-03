namespace Craft.Cache;

/// <summary>
/// Defines the contract for cache invalidation strategies.
/// </summary>
public interface ICacheInvalidator
{
    /// <summary>
    /// Invalidates cache entries based on entity type and ID.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of invalidated entries.</returns>
    Task<int> InvalidateEntityAsync<TEntity>(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cache entries for an entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of invalidated entries.</returns>
    Task<int> InvalidateEntityTypeAsync<TEntity>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cache entries by keys.
    /// </summary>
    /// <param name="keys">The cache keys to invalidate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of invalidated entries.</returns>
    Task<int> InvalidateKeysAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cache entries by pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of invalidated entries.</returns>
    Task<int> InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cache entries using a custom strategy.
    /// </summary>
    /// <param name="strategy">The invalidation strategy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of invalidated entries.</returns>
    Task<int> InvalidateAsync(ICacheInvalidationStrategy strategy, CancellationToken cancellationToken = default);
}
