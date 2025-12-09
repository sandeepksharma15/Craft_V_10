using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Cache;

/// <summary>
/// Null cache provider that performs no caching (useful for testing/disabling cache).
/// </summary>
public class NullCacheProvider : ICacheProvider
{
    private readonly ILogger<NullCacheProvider> _logger;
    private readonly CacheStats _stats = new();

    public string Name => "null";

    public NullCacheProvider(ILogger<NullCacheProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsConfigured() => true;

    public Task<CacheResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NULL CACHE: Get called for key: {Key}", key);
        return Task.FromResult(CacheResult<T>.Success());
    }

    public Task<CacheResult> SetAsync<T>(string key, T? value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NULL CACHE: Set called for key: {Key}", key);
        return Task.FromResult(CacheResult.Success());
    }

    public Task<CacheResult> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NULL CACHE: Remove called for key: {Key}", key);
        return Task.FromResult(CacheResult.Success());
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IDictionary<string, T?>>(new Dictionary<string, T?>());
    }

    public Task<CacheResult> SetManyAsync<T>(IDictionary<string, T?> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NULL CACHE: SetMany called for {Count} items", items.Count);
        return Task.FromResult(CacheResult.Success());
    }

    public Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NULL CACHE: RemoveByPattern called for pattern: {Pattern}", pattern);
        return Task.FromResult(0);
    }

    public Task<CacheStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_stats);
    }

    public Task<CacheResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NULL CACHE: Clear called");
        return Task.FromResult(CacheResult.Success());
    }

    public Task<CacheResult> RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NULL CACHE: Refresh called for key: {Key}", key);
        return Task.FromResult(CacheResult.Success());
    }
}
