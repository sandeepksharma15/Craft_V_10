using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Cache;

/// <summary>
/// Factory for creating and managing cache providers.
/// </summary>
public class CacheProviderFactory : ICacheProviderFactory
{
    private readonly IEnumerable<ICacheProvider> _providers;
    private readonly ILogger<CacheProviderFactory> _logger;
    private readonly CacheOptions _options;

    public CacheProviderFactory(IEnumerable<ICacheProvider> providers, IOptions<CacheOptions> options,
        ILogger<CacheProviderFactory> logger)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public ICacheProvider GetDefaultProvider()
    {
        return GetProvider(_options.Provider);
    }

    public ICacheProvider GetProvider(string providerName)
    {
        var provider = _providers.FirstOrDefault(p => 
            p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            _logger.LogError("Cache provider '{Provider}' not found. Available providers: {Providers}",
                providerName, string.Join(", ", _providers.Select(p => p.Name)));
            throw new InvalidOperationException($"Cache provider '{providerName}' is not registered.");
        }

        if (!provider.IsConfigured())
        {
            _logger.LogWarning("Cache provider '{Provider}' is not configured properly", providerName);
        }

        return provider;
    }

    public IEnumerable<ICacheProvider> GetAllProviders()
    {
        return _providers;
    }
}
