using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Craft.Cache;

/// <summary>
/// In-memory cache provider implementation using IMemoryCache.
/// </summary>
public class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheProvider> _logger;
    private readonly CacheOptions _options;
    private readonly CacheStats _stats;
    private readonly ConcurrentDictionary<string, byte> _keys;

    public string Name => "memory";

    public MemoryCacheProvider(
        IMemoryCache memoryCache,
        IOptions<CacheOptions> options,
        ILogger<MemoryCacheProvider> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _stats = new CacheStats();
        _keys = new ConcurrentDictionary<string, byte>();
    }

    public bool IsConfigured() => true;

    public Task<CacheResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullKey = GetFullKey(key);

            if (_memoryCache.TryGetValue(fullKey, out T? value))
            {
                if (_options.EnableStatistics)
                    Interlocked.Increment(ref _stats.HitsRef);

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return Task.FromResult(CacheResult<T>.Success(value));
            }

            if (_options.EnableStatistics)
                Interlocked.Increment(ref _stats.MissesRef);

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return Task.FromResult(CacheResult<T>.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from memory cache for key: {Key}", key);
            return Task.FromResult(CacheResult<T>.Failure($"Failed to get cache entry: {ex.Message}", ex));
        }
    }

    public Task<CacheResult> SetAsync<T>(string key, T? value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var entryOptions = CreateMemoryCacheEntryOptions(options);

            _memoryCache.Set(fullKey, value, entryOptions);
            _keys.TryAdd(fullKey, 0);

            if (_options.EnableStatistics)
                Interlocked.Increment(ref _stats.SetsRef);

            _logger.LogDebug("Set cache entry for key: {Key}", key);
            return Task.FromResult(CacheResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in memory cache for key: {Key}", key);
            return Task.FromResult(CacheResult.Failure($"Failed to set cache entry: {ex.Message}", ex));
        }
    }

    public Task<CacheResult> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullKey = GetFullKey(key);
            _memoryCache.Remove(fullKey);
            _keys.TryRemove(fullKey, out _);

            if (_options.EnableStatistics)
                Interlocked.Increment(ref _stats.RemovesRef);

            _logger.LogDebug("Removed cache entry for key: {Key}", key);
            return Task.FromResult(CacheResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from memory cache for key: {Key}", key);
            return Task.FromResult(CacheResult.Failure($"Failed to remove cache entry: {ex.Message}", ex));
        }
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return Task.FromResult(_memoryCache.TryGetValue(fullKey, out _));
    }

    public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, T?>();

        foreach (var key in keys)
        {
            var cacheResult = await GetAsync<T>(key, cancellationToken);
            if (cacheResult.HasValue)
                result[key] = cacheResult.Value;
        }

        return result;
    }

    public async Task<CacheResult> SetManyAsync<T>(IDictionary<string, T?> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var item in items)
            {
                var result = await SetAsync(item.Key, item.Value, options, cancellationToken);
                if (!result.IsSuccess)
                    return result;
            }

            return CacheResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multiple values in memory cache");
            return CacheResult.Failure($"Failed to set multiple cache entries: {ex.Message}", ex);
        }
    }

    public Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            var matchingKeys = _keys.Keys.Where(k => regex.IsMatch(k)).ToList();
            
            foreach (var key in matchingKeys)
            {
                _memoryCache.Remove(key);
                _keys.TryRemove(key, out _);
            }

            if (_options.EnableStatistics)
                Interlocked.Add(ref _stats.RemovesRef, matchingKeys.Count);

            _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", matchingKeys.Count, pattern);
            return Task.FromResult(matchingKeys.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
            return Task.FromResult(0);
        }
    }

    public Task<CacheStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        _stats.EntryCount = _keys.Count;
        _stats.Timestamp = DateTimeOffset.UtcNow;
        return Task.FromResult(_stats);
    }

    public Task<CacheResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var keys = _keys.Keys.ToList();
            foreach (var key in keys)
            {
                _memoryCache.Remove(key);
                _keys.TryRemove(key, out _);
            }

            _logger.LogInformation("Cleared {Count} entries from memory cache", keys.Count);
            return Task.FromResult(CacheResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing memory cache");
            return Task.FromResult(CacheResult.Failure($"Failed to clear cache: {ex.Message}", ex));
        }
    }

    public Task<CacheResult> RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        // Memory cache doesn't support explicit refresh, but we can get and set
        try
        {
            var fullKey = GetFullKey(key);
            
            if (_memoryCache.TryGetValue(fullKey, out object? value))
            {
                // This will reset the sliding expiration if configured
                _memoryCache.TryGetValue(fullKey, out _);
                _logger.LogDebug("Refreshed cache entry for key: {Key}", key);
                return Task.FromResult(CacheResult.Success());
            }

            _logger.LogDebug("Cache entry not found for refresh: {Key}", key);
            return Task.FromResult(CacheResult.Failure("Cache entry not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache entry for key: {Key}", key);
            return Task.FromResult(CacheResult.Failure($"Failed to refresh cache entry: {ex.Message}", ex));
        }
    }

    private string GetFullKey(string key)
    {
        return string.IsNullOrWhiteSpace(_options.KeyPrefix) 
            ? key 
            : $"{_options.KeyPrefix}{key}";
    }

    private MemoryCacheEntryOptions CreateMemoryCacheEntryOptions(CacheEntryOptions? options)
    {
        if (options != null)
            return options.ToMemoryCacheEntryOptions();

        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _options.DefaultExpiration,
            SlidingExpiration = _options.DefaultSlidingExpiration,
            Priority = CacheItemPriority.Normal
        };
    }
}
