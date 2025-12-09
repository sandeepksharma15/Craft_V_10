using Microsoft.Extensions.Logging;

namespace Craft.Cache;

/// <summary>
/// Main cache service implementation that delegates to configured cache providers.
/// </summary>
public class CacheService : ICacheService
{
    private readonly ICacheProviderFactory _providerFactory;
    private readonly ILogger<CacheService> _logger;
    private readonly ICacheProvider _defaultProvider;

    public CacheService(ICacheProviderFactory providerFactory, ILogger<CacheService> logger)
    {
        _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultProvider = _providerFactory.GetDefaultProvider();
    }

    public Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory)
    {
        return GetOrSetAsync(cacheKey, valueFactory, null, CancellationToken.None);
    }

    // ========================================
    // Enhanced Methods
    // ========================================

    public T? Set<T>(string cacheKey, T? value, TimeSpan? expiration)
    {
        var options = expiration.HasValue 
            ? CacheEntryOptions.WithExpiration(expiration.Value) 
            : null;
        
        SetAsync(cacheKey, value, options).GetAwaiter().GetResult();
        return value;
    }

    public T? Set<T>(string cacheKey, T? value, CacheEntryOptions options)
    {
        SetAsync(cacheKey, value, options).GetAwaiter().GetResult();
        return value;
    }

    // ========================================
    // Async Operations
    // ========================================

    public async Task<CacheResult<T>> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.GetAsync<T>(cacheKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", cacheKey);
            return CacheResult<T>.Failure($"Failed to get cache entry: {ex.Message}", ex);
        }
    }

    public async Task<CacheResult> SetAsync<T>(string cacheKey, T? value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.SetAsync(cacheKey, value, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", cacheKey);
            return CacheResult.Failure($"Failed to set cache entry: {ex.Message}", ex);
        }
    }

    public async Task<CacheResult> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.RemoveAsync(cacheKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", cacheKey);
            return CacheResult.Failure($"Failed to remove cache entry: {ex.Message}", ex);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory, CacheEntryOptions? options, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _defaultProvider.GetAsync<T>(cacheKey, cancellationToken);
            
            if (result.HasValue)
            {
                _logger.LogDebug("Cache hit for key: {Key}", cacheKey);
                return result.Value;
            }

            _logger.LogDebug("Cache miss for key: {Key}, executing value factory", cacheKey);
            var value = await valueFactory();
            
            await _defaultProvider.SetAsync(cacheKey, value, options, cancellationToken);
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrSetAsync for key: {Key}", cacheKey);
            throw;
        }
    }

    // ========================================
    // Bulk Operations
    // ========================================

    public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.GetManyAsync<T>(keys, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple cache values");
            return new Dictionary<string, T?>();
        }
    }

    public async Task<CacheResult> SetManyAsync<T>(IDictionary<string, T?> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.SetManyAsync(items, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multiple cache values");
            return CacheResult.Failure($"Failed to set multiple cache entries: {ex.Message}", ex);
        }
    }

    // ========================================
    // Pattern Operations
    // ========================================

    public async Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.RemoveByPatternAsync(pattern, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
            return 0;
        }
    }

    // ========================================
    // Cache Information
    // ========================================

    public async Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.ExistsAsync(cacheKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache key existence: {Key}", cacheKey);
            return false;
        }
    }

    public async Task<CacheStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.GetStatsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new CacheStats();
        }
    }

    public async Task<CacheResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.ClearAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return CacheResult.Failure($"Failed to clear cache: {ex.Message}", ex);
        }
    }

    public async Task<CacheResult> RefreshAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _defaultProvider.RefreshAsync(cacheKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache entry: {Key}", cacheKey);
            return CacheResult.Failure($"Failed to refresh cache entry: {ex.Message}", ex);
        }
    }
}
