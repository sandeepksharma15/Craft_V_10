using Microsoft.Extensions.Logging;

namespace Craft.Cache;

/// <summary>
/// Default implementation of cache invalidator.
/// </summary>
public class CacheInvalidator : ICacheInvalidator
{
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly ILogger<CacheInvalidator> _logger;

    public CacheInvalidator(
        ICacheService cacheService,
        ICacheKeyGenerator keyGenerator,
        ILogger<CacheInvalidator> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> InvalidateEntityAsync<TEntity>(object id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            var key = _keyGenerator.GenerateEntityKey<TEntity>(id);
            var result = await _cacheService.RemoveAsync(key, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Invalidated cache for {EntityType}:{Id}", typeof(TEntity).Name, id);
                return 1;
            }

            _logger.LogWarning("Failed to invalidate cache for {EntityType}:{Id}: {Error}",
                typeof(TEntity).Name, id, result.ErrorMessage);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for {EntityType}:{Id}", typeof(TEntity).Name, id);
            return 0;
        }
    }

    public async Task<int> InvalidateEntityTypeAsync<TEntity>(CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = _keyGenerator.GenerateEntityPattern<TEntity>();
            var count = await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);

            _logger.LogDebug("Invalidated {Count} cache entries for {EntityType}", count, typeof(TEntity).Name);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for {EntityType}", typeof(TEntity).Name);
            return 0;
        }
    }

    public async Task<int> InvalidateKeysAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var count = 0;
        foreach (var key in keys)
        {
            try
            {
                var result = await _cacheService.RemoveAsync(key, cancellationToken);
                if (result.IsSuccess)
                    count++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache key: {Key}", key);
            }
        }

        _logger.LogDebug("Invalidated {Count} cache entries by keys", count);
        return count;
    }

    public async Task<int> InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        try
        {
            var count = await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
            _logger.LogDebug("Invalidated {Count} cache entries matching pattern: {Pattern}", count, pattern);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by pattern: {Pattern}", pattern);
            return 0;
        }
    }

    public async Task<int> InvalidateAsync(ICacheInvalidationStrategy strategy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strategy);

        try
        {
            var patterns = strategy.GetPatternsToInvalidate();
            var totalCount = 0;

            foreach (var pattern in patterns)
            {
                // Check if it's a pattern or exact key
                if (pattern.Contains('*') || pattern.Contains('?'))
                {
                    var count = await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
                    totalCount += count;
                }
                else
                {
                    var result = await _cacheService.RemoveAsync(pattern, cancellationToken);
                    if (result.IsSuccess)
                        totalCount++;
                }
            }

            _logger.LogDebug("Invalidated {Count} cache entries using strategy {StrategyType}",
                totalCount, strategy.GetType().Name);
            return totalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache using strategy {StrategyType}", strategy.GetType().Name);
            return 0;
        }
    }
}
