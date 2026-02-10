using System.ComponentModel.DataAnnotations;

namespace Craft.Cache;

/// <summary>
/// Configuration settings for cache services.
/// </summary>
public class CacheOptions : IValidatableObject
{
    public const string SectionName = "CacheOptions";

    /// <summary>
    /// Gets or sets the default cache provider to use (e.g., "memory", "redis", "hybrid").
    /// </summary>
    [Required(ErrorMessage = "Cache provider is required")]
    public string Provider { get; set; } = "memory";

    /// <summary>
    /// Gets or sets the default expiration time for cache entries.
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the default sliding expiration time for cache entries.
    /// </summary>
    public TimeSpan? DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets a value indicating whether to enable cache statistics tracking.
    /// </summary>
    public bool EnableStatistics { get; set; } = true;

    /// <summary>
    /// Gets or sets the key prefix for all cache keys.
    /// </summary>
    public string KeyPrefix { get; set; } = "__CRAFT__";

    /// <summary>
    /// Gets or sets memory cache specific settings.
    /// </summary>
    public MemoryCacheSettings? Memory { get; set; }

    /// <summary>
    /// Gets or sets Redis cache specific settings.
    /// </summary>
    public RedisCacheSettings? Redis { get; set; }

    /// <summary>
    /// Gets or sets hybrid cache specific settings.
    /// </summary>
    public HybridCacheSettings? Hybrid { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Provider.Equals("redis", StringComparison.OrdinalIgnoreCase))
        {
            if (Redis == null)
                yield return new ValidationResult(
                    "Redis settings are required when using Redis provider",
                    [nameof(Redis)]);
            else
            {
                var redisValidationContext = new ValidationContext(Redis);
                var redisResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(Redis, redisValidationContext, redisResults, true))
                    foreach (var result in redisResults)
                        yield return result;
            }
        }

        if (Provider.Equals("hybrid", StringComparison.OrdinalIgnoreCase))
        {
            if (Hybrid == null)
                yield return new ValidationResult(
                    "Hybrid settings are required when using Hybrid provider",
                    [nameof(Hybrid)]);
            else
            {
                var hybridValidationContext = new ValidationContext(Hybrid);
                var hybridResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(Hybrid, hybridValidationContext, hybridResults, true))
                    foreach (var result in hybridResults)
                        yield return result;
            }
        }

        if (DefaultExpiration <= TimeSpan.Zero)
            yield return new ValidationResult(
                "DefaultExpiration must be greater than zero",
                [nameof(DefaultExpiration)]);

        if (DefaultSlidingExpiration.HasValue && DefaultSlidingExpiration.Value <= TimeSpan.Zero)
            yield return new ValidationResult(
                "DefaultSlidingExpiration must be greater than zero or null",
                [nameof(DefaultSlidingExpiration)]);
    }
}

/// <summary>
/// Memory cache specific configuration settings.
/// </summary>
public class MemoryCacheSettings
{
    /// <summary>
    /// Gets or sets the maximum size of the cache in MB (null for unlimited).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "SizeLimit must be greater than 0")]
    public long? SizeLimit { get; set; }

    /// <summary>
    /// Gets or sets the compaction percentage (0.0 to 1.0) when size limit is reached.
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "CompactionPercentage must be between 0.0 and 1.0")]
    public double CompactionPercentage { get; set; } = 0.25;

    /// <summary>
    /// Gets or sets the expiration scan frequency.
    /// </summary>
    public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1);
}

/// <summary>
/// Redis cache specific configuration settings.
/// </summary>
public class RedisCacheSettings : IValidatableObject
{
    /// <summary>
    /// Gets or sets the Redis connection string.
    /// </summary>
    [Required(ErrorMessage = "Redis connection string is required")]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the Redis database number.
    /// </summary>
    [Range(0, 15, ErrorMessage = "Database must be between 0 and 15")]
    public int Database { get; set; } = 0;

    /// <summary>
    /// Gets or sets the instance name prefix for Redis keys.
    /// </summary>
    public string InstanceName { get; set; } = "craft:";

    /// <summary>
    /// Gets or sets the connection timeout in milliseconds.
    /// </summary>
    [Range(100, 60000, ErrorMessage = "ConnectTimeout must be between 100 and 60000 milliseconds")]
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the sync timeout in milliseconds.
    /// </summary>
    [Range(100, 60000, ErrorMessage = "SyncTimeout must be between 100 and 60000 milliseconds")]
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the number of retry attempts for failed operations.
    /// </summary>
    [Range(0, 10, ErrorMessage = "RetryCount must be between 0 and 10")]
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL/TLS.
    /// </summary>
    public bool UseSsl { get; set; } = false;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            yield return new ValidationResult(
                "Redis connection string is required",
                [nameof(ConnectionString)]);
    }
}

/// <summary>
/// Hybrid cache specific configuration settings.
/// </summary>
public class HybridCacheSettings : IValidatableObject
{
    /// <summary>
    /// Gets or sets the L1 (local) cache provider.
    /// </summary>
    [Required(ErrorMessage = "L1Provider is required for hybrid caching")]
    public string L1Provider { get; set; } = "memory";

    /// <summary>
    /// Gets or sets the L2 (distributed) cache provider.
    /// </summary>
    [Required(ErrorMessage = "L2Provider is required for hybrid caching")]
    public string L2Provider { get; set; } = "redis";

    /// <summary>
    /// Gets or sets the L1 cache expiration time.
    /// </summary>
    public TimeSpan L1Expiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets a value indicating whether to sync L1 on L2 updates.
    /// </summary>
    public bool SyncL1OnL2Update { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (L1Provider.Equals(L2Provider, StringComparison.OrdinalIgnoreCase))
            yield return new ValidationResult(
                "L1Provider and L2Provider must be different for hybrid caching",
                [nameof(L1Provider), nameof(L2Provider)]);

        if (L1Expiration <= TimeSpan.Zero)
            yield return new ValidationResult(
                "L1Expiration must be greater than zero",
                [nameof(L1Expiration)]);
    }
}

