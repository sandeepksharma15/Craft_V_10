namespace Craft.Cache;

/// <summary>
/// Factory for creating and managing cache providers.
/// </summary>
public interface ICacheProviderFactory
{
    /// <summary>
    /// Gets the default cache provider based on configuration.
    /// </summary>
    ICacheProvider GetDefaultProvider();

    /// <summary>
    /// Gets a cache provider by name.
    /// </summary>
    /// <param name="providerName">The name of the provider (e.g., "memory", "redis").</param>
    ICacheProvider GetProvider(string providerName);

    /// <summary>
    /// Gets all registered cache providers.
    /// </summary>
    IEnumerable<ICacheProvider> GetAllProviders();
}
