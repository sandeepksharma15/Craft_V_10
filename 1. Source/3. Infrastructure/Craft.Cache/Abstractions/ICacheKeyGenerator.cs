namespace Craft.Cache;

/// <summary>
/// Defines the contract for generating cache keys.
/// </summary>
public interface ICacheKeyGenerator
{
    /// <summary>
    /// Generates a cache key from a type and parameters.
    /// </summary>
    /// <param name="type">The type for which to generate the key.</param>
    /// <param name="parameters">The parameters to include in the key.</param>
    /// <returns>The generated cache key.</returns>
    string Generate(Type type, params object[] parameters);

    /// <summary>
    /// Generates a cache key from a method and its arguments.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="arguments">The method arguments.</param>
    /// <returns>The generated cache key.</returns>
    string GenerateMethodKey(string typeName, string methodName, params object[] arguments);

    /// <summary>
    /// Generates a cache key for an entity by its ID.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="id">The entity ID.</param>
    /// <returns>The generated cache key.</returns>
    string GenerateEntityKey<TEntity>(object id);

    /// <summary>
    /// Generates a pattern for matching related cache keys.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="pattern">Optional pattern suffix.</param>
    /// <returns>The generated pattern.</returns>
    string GenerateEntityPattern<TEntity>(string? pattern = null);
}
