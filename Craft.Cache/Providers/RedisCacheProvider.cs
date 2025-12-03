using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Craft.Cache;

/// <summary>
/// Redis cache provider implementation using StackExchange.Redis.
/// </summary>
public class RedisCacheProvider : ICacheProvider, IDisposable
{
    private readonly ILogger<RedisCacheProvider> _logger;
    private readonly CacheOptions _options;
    private readonly CacheStats _stats;
    private IConnectionMultiplexer? _connection;
    private IDatabase? _database;
    private bool _disposed;

    public string Name => "redis";

    public RedisCacheProvider(IOptions<CacheOptions> options, ILogger<RedisCacheProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _stats = new CacheStats();

        InitializeConnection();
    }

    public bool IsConfigured() => _connection?.IsConnected ?? false;

    private void InitializeConnection()
    {
        try
        {
            if (_options.Redis == null)
            {
                _logger.LogWarning("Redis settings not configured");
                return;
            }

            var configOptions = ConfigurationOptions.Parse(_options.Redis.ConnectionString!);
            configOptions.ConnectTimeout = _options.Redis.ConnectTimeout;
            configOptions.SyncTimeout = _options.Redis.SyncTimeout;
            configOptions.Ssl = _options.Redis.UseSsl;
            configOptions.AbortOnConnectFail = false;

            _connection = ConnectionMultiplexer.Connect(configOptions);
            _database = _connection.GetDatabase(_options.Redis.Database);

            _logger.LogInformation("Redis cache provider initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Redis connection");
        }
    }

    public async Task<CacheResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return CacheResult<T>.Failure("Redis connection not available");

        try
        {
            var fullKey = GetFullKey(key);
            var value = await _database!.StringGetAsync(fullKey);

            if (value.IsNullOrEmpty)
            {
                if (_options.EnableStatistics)
                    Interlocked.Increment(ref _stats.MissesRef);

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return CacheResult<T>.Success();
            }

            if (_options.EnableStatistics)
                Interlocked.Increment(ref _stats.HitsRef);

            var deserializedValue = JsonSerializer.Deserialize<T>((string)value!);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return CacheResult<T>.Success(deserializedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from Redis for key: {Key}", key);
            return CacheResult<T>.Failure($"Failed to get cache entry: {ex.Message}", ex);
        }
    }

    public async Task<CacheResult> SetAsync<T>(string key, T? value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return CacheResult.Failure("Redis connection not available");

        try
        {
            var fullKey = GetFullKey(key);
            var serializedValue = JsonSerializer.Serialize(value);
            var expiry = GetExpiry(options);

            await _database!.StringSetAsync(fullKey, serializedValue, expiry);

            if (_options.EnableStatistics)
                Interlocked.Increment(ref _stats.SetsRef);

            _logger.LogDebug("Set cache entry for key: {Key} with expiry: {Expiry}", key, expiry);
            return CacheResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in Redis for key: {Key}", key);
            return CacheResult.Failure($"Failed to set cache entry: {ex.Message}", ex);
        }
    }

    public async Task<CacheResult> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return CacheResult.Failure("Redis connection not available");

        try
        {
            var fullKey = GetFullKey(key);
            await _database!.KeyDeleteAsync(fullKey);

            if (_options.EnableStatistics)
                Interlocked.Increment(ref _stats.RemovesRef);

            _logger.LogDebug("Removed cache entry for key: {Key}", key);
            return CacheResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from Redis for key: {Key}", key);
            return CacheResult.Failure($"Failed to remove cache entry: {ex.Message}", ex);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return false;

        try
        {
            var fullKey = GetFullKey(key);
            return await _database!.KeyExistsAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking key existence in Redis for key: {Key}", key);
            return false;
        }
    }

    public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return new Dictionary<string, T?>();

        var result = new Dictionary<string, T?>();

        try
        {
            var redisKeys = keys.Select(k => (RedisKey)GetFullKey(k)).ToArray();
            var values = await _database!.StringGetAsync(redisKeys);

            var keysList = keys.ToList();
            for (int i = 0; i < values.Length; i++)
            {
                if (!values[i].IsNullOrEmpty)
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>((string)values[i]!);
                    result[keysList[i]] = deserializedValue;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple values from Redis");
        }

        return result;
    }

    public async Task<CacheResult> SetManyAsync<T>(IDictionary<string, T?> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return CacheResult.Failure("Redis connection not available");

        try
        {
            var expiry = GetExpiry(options);
            var batch = _database!.CreateBatch();
            var tasks = new List<Task>();

            foreach (var item in items)
            {
                var fullKey = GetFullKey(item.Key);
                var serializedValue = JsonSerializer.Serialize(item.Value);
                tasks.Add(batch.StringSetAsync(fullKey, serializedValue, expiry));
            }

            batch.Execute();
            await Task.WhenAll(tasks);

            if (_options.EnableStatistics)
                Interlocked.Add(ref _stats.SetsRef, items.Count);

            _logger.LogDebug("Set {Count} cache entries in Redis", items.Count);
            return CacheResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multiple values in Redis");
            return CacheResult.Failure($"Failed to set multiple cache entries: {ex.Message}", ex);
        }
    }

    public async Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return 0;

        try
        {
            var fullPattern = GetFullKey(pattern);
            var server = _connection!.GetServer(_connection.GetEndPoints().First());
            var keys = server.Keys(pattern: fullPattern).ToArray();

            if (keys.Length > 0)
            {
                await _database!.KeyDeleteAsync(keys);

                if (_options.EnableStatistics)
                    Interlocked.Add(ref _stats.RemovesRef, keys.Length);

                _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keys.Length, pattern);
            }

            return keys.Length;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
            return 0;
        }
    }

    public async Task<CacheStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return _stats;

        try
        {
            var server = _connection!.GetServer(_connection.GetEndPoints().First());
            var info = await server.InfoAsync("stats");
            
            // Try to get approximate key count
            var keyCount = 0L;
            try
            {
                var pattern = string.IsNullOrWhiteSpace(_options.KeyPrefix) ? "*" : $"{_options.KeyPrefix}*";
                keyCount = server.Keys(pattern: pattern).LongCount();
            }
            catch
            {
                // Ignore errors getting key count
            }

            _stats.EntryCount = keyCount;
            _stats.Timestamp = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Redis statistics");
        }

        return _stats;
    }

    public async Task<CacheResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return CacheResult.Failure("Redis connection not available");

        try
        {
            var pattern = string.IsNullOrWhiteSpace(_options.KeyPrefix) ? "*" : $"{_options.KeyPrefix}*";
            var count = await RemoveByPatternAsync(pattern, cancellationToken);

            _logger.LogInformation("Cleared {Count} entries from Redis cache", count);
            return CacheResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing Redis cache");
            return CacheResult.Failure($"Failed to clear cache: {ex.Message}", ex);
        }
    }

    public async Task<CacheResult> RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!EnsureConnected())
            return CacheResult.Failure("Redis connection not available");

        try
        {
            var fullKey = GetFullKey(key);
            var exists = await _database!.KeyExistsAsync(fullKey);

            if (!exists)
            {
                _logger.LogDebug("Cache entry not found for refresh: {Key}", key);
                return CacheResult.Failure("Cache entry not found");
            }

            // Get current TTL and reset it
            var ttl = await _database.KeyTimeToLiveAsync(fullKey);
            if (ttl.HasValue)
            {
                await _database.KeyExpireAsync(fullKey, ttl.Value);
                _logger.LogDebug("Refreshed cache entry for key: {Key}", key);
                return CacheResult.Success();
            }

            _logger.LogDebug("Cache entry has no expiration: {Key}", key);
            return CacheResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache entry for key: {Key}", key);
            return CacheResult.Failure($"Failed to refresh cache entry: {ex.Message}", ex);
        }
    }

    private string GetFullKey(string key)
    {
        var prefix = _options.Redis?.InstanceName ?? _options.KeyPrefix;
        return string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}{key}";
    }

    private TimeSpan? GetExpiry(CacheEntryOptions? options)
    {
        if (options?.AbsoluteExpirationRelativeToNow.HasValue == true)
            return options.AbsoluteExpirationRelativeToNow.Value;

        if (options?.AbsoluteExpiration.HasValue == true)
            return options.AbsoluteExpiration.Value - DateTimeOffset.UtcNow;

        return _options.DefaultExpiration;
    }

    private bool EnsureConnected()
    {
        if (_database != null && _connection?.IsConnected == true)
            return true;

        _logger.LogWarning("Redis connection is not available");
        return false;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _connection?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
