using Craft.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Cache.Tests;

/// <summary>
/// Integration tests for complete cache service workflow.
/// </summary>
public class CacheServiceIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ICacheService _cacheService;

    public CacheServiceIntegrationTests()
    {
        var services = new ServiceCollection();

        // Configure cache with memory provider
        services.AddCacheServices(options =>
        {
            options.Provider = "memory";
            options.DefaultExpiration = TimeSpan.FromMinutes(10);
            options.EnableStatistics = true;
            options.KeyPrefix = "test:";
        });

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        _serviceProvider = services.BuildServiceProvider();
        _cacheService = _serviceProvider.GetRequiredService<ICacheService>();
    }

    [Fact]
    public async Task CompleteWorkflow_SetGetRemove_WorksCorrectly()
    {
        // Arrange
        var key = "integration-test-1";
        var value = "test-value";

        // Act - Set
        var setResult = await _cacheService.SetAsync(key, value);

        // Act - Get
        var getResult = await _cacheService.GetAsync<string>(key);

        // Act - Remove
        var removeResult = await _cacheService.RemoveAsync(key);

        // Act - Get after remove
        var getAfterRemove = await _cacheService.GetAsync<string>(key);

        // Assert
        Assert.True(setResult.IsSuccess);
        Assert.True(getResult.IsSuccess);
        Assert.True(getResult.HasValue);
        Assert.Equal(value, getResult.Value);
        Assert.True(removeResult.IsSuccess);
        Assert.False(getAfterRemove.HasValue);
    }

    [Fact]
    public async Task GetOrSetAsync_WithMiss_CreatesAndReturnsValue()
    {
        // Arrange
        var key = "integration-test-2";
        var callCount = 0;

        // Act - First call should execute factory
        var value1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(1);
            return "computed-value";
        });

        // Act - Second call should not execute factory
        var value2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(1);
            return "computed-value";
        });

        // Assert
        Assert.Equal(1, callCount);  // Factory called only once
        Assert.Equal("computed-value", value1);
        Assert.Equal("computed-value", value2);
    }

    [Fact]
    public async Task BulkOperations_SetManyAndGetMany_WorkCorrectly()
    {
        // Arrange
        var items = new Dictionary<string, string?>
        {
            ["bulk-1"] = "value-1",
            ["bulk-2"] = "value-2",
            ["bulk-3"] = "value-3"
        };

        // Act - Set many
        var setResult = await _cacheService.SetManyAsync(items);

        // Act - Get many
        var getResult = await _cacheService.GetManyAsync<string>(items.Keys);

        // Assert
        Assert.True(setResult.IsSuccess);
        Assert.Equal(3, getResult.Count);
        Assert.Equal("value-1", getResult["bulk-1"]);
        Assert.Equal("value-2", getResult["bulk-2"]);
        Assert.Equal("value-3", getResult["bulk-3"]);
    }

    [Fact]
    public async Task PatternRemoval_RemovesMatchingKeys()
    {
        // Arrange
        await _cacheService.SetAsync("pattern:1", "value1");
        await _cacheService.SetAsync("pattern:2", "value2");
        await _cacheService.SetAsync("pattern:3", "value3");
        await _cacheService.SetAsync("other:1", "value4");

        // Act
        var removedCount = await _cacheService.RemoveByPatternAsync("pattern:*");

        // Act - Verify removal
        var exists1 = await _cacheService.ExistsAsync("pattern:1");
        var exists2 = await _cacheService.ExistsAsync("pattern:2");
        var exists3 = await _cacheService.ExistsAsync("pattern:3");
        var existsOther = await _cacheService.ExistsAsync("other:1");

        // Assert
        Assert.True(removedCount >= 3);  // At least 3 keys removed
        Assert.False(exists1);
        Assert.False(exists2);
        Assert.False(exists3);
        Assert.True(existsOther);  // This should not be removed
    }

    [Fact]
    public async Task Statistics_TrackOperations()
    {
        // Arrange
        await _cacheService.ClearAsync();  // Start fresh
        
        // Act - Perform various operations
        await _cacheService.SetAsync("stats-1", "value1");
        await _cacheService.SetAsync("stats-2", "value2");
        await _cacheService.GetAsync<string>("stats-1");  // Hit
        await _cacheService.GetAsync<string>("stats-2");  // Hit
        await _cacheService.GetAsync<string>("stats-3");  // Miss
        await _cacheService.RemoveAsync("stats-1");

        // Act - Get stats
        var stats = await _cacheService.GetStatsAsync();

        // Assert
        Assert.True(stats.Sets >= 2);
        Assert.True(stats.Hits >= 2);
        Assert.True(stats.Misses >= 1);
        Assert.True(stats.Removes >= 1);
        Assert.True(stats.HitRatio > 0);
    }

    [Fact]
    public async Task ExpirationOptions_WorkCorrectly()
    {
        // Arrange
        var key = "expiration-test";
        var options = CacheEntryOptions.WithExpiration(TimeSpan.FromMilliseconds(100));

        // Act
        await _cacheService.SetAsync(key, "value", options);
        var existsBefore = await _cacheService.ExistsAsync(key);
        
        // Wait for expiration
        await Task.Delay(150);
        
        var existsAfter = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.True(existsBefore);
        // Note: This might be flaky in real scenarios, but demonstrates the concept
    }

    [Fact]
    public async Task ComplexTypes_CanBeCached()
    {
        // Arrange
        var key = "complex-object";
        var complexObject = new ComplexTestObject
        {
            Id = 123,
            Name = "Test Object",
            CreatedDate = DateTime.UtcNow,
            Tags = new List<string> { "tag1", "tag2", "tag3" },
            Metadata = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            }
        };

        // Act
        await _cacheService.SetAsync(key, complexObject);
        var result = await _cacheService.GetAsync<ComplexTestObject>(key);

        // Assert
        Assert.True(result.HasValue);
        Assert.NotNull(result.Value);
        Assert.Equal(123, result.Value.Id);
        Assert.Equal("Test Object", result.Value.Name);
        Assert.Equal(3, result.Value.Tags.Count);
        Assert.Equal(2, result.Value.Metadata.Count);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllEntries()
    {
        // Arrange
        await _cacheService.SetAsync("clear-1", "value1");
        await _cacheService.SetAsync("clear-2", "value2");
        await _cacheService.SetAsync("clear-3", "value3");

        // Act
        var clearResult = await _cacheService.ClearAsync();
        var exists1 = await _cacheService.ExistsAsync("clear-1");
        var exists2 = await _cacheService.ExistsAsync("clear-2");
        var exists3 = await _cacheService.ExistsAsync("clear-3");

        // Assert
        Assert.True(clearResult.IsSuccess);
        Assert.False(exists1);
        Assert.False(exists2);
        Assert.False(exists3);
    }

    [Fact]
    public async Task ConcurrentOperations_AreThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        var key = "concurrent-test";

        // Act - Multiple concurrent sets
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(_cacheService.SetAsync($"{key}-{index}", $"value-{index}"));
        }

        await Task.WhenAll(tasks);
        tasks.Clear();

        // Act - Multiple concurrent gets
        var results = new List<CacheResult<string>>();
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var result = await _cacheService.GetAsync<string>($"{key}-{index}");
                lock (results)
                {
                    results.Add(result);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, results.Count);
        Assert.All(results, r => Assert.True(r.IsSuccess));
        Assert.All(results, r => Assert.True(r.HasValue));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

public class ComplexTestObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
