using Craft.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.IntegrationTests.Cache;

/// <summary>
/// Integration tests for cache service and providers.
/// Tests complete cache loop: Set ? Get ? Update ? Remove ? Verify.
/// </summary>
public class CacheServiceIntegrationTests
{
    private readonly ICacheService _cacheService;
    private readonly ICacheProvider _memoryProvider;

    public CacheServiceIntegrationTests()
    {
        var options = Options.Create(new CacheOptions
        {
            DefaultExpiration = TimeSpan.FromMinutes(30),
            KeyPrefix = "test",
            EnableStatistics = true
        });

        var loggerFactory = new LoggerFactory();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        _memoryProvider = new MemoryCacheProvider(
            memoryCache,
            options,
            loggerFactory.CreateLogger<MemoryCacheProvider>());

        var providerFactoryMock = new Mock<ICacheProviderFactory>();
        providerFactoryMock.Setup(f => f.GetDefaultProvider()).Returns(_memoryProvider);
        providerFactoryMock.Setup(f => f.GetProvider("memory")).Returns(_memoryProvider);

        _cacheService = new CacheService(
            providerFactoryMock.Object,
            loggerFactory.CreateLogger<CacheService>());
    }

    #region Complete Cache Loop Tests

    [Fact]
    public async Task CompleteCacheLoop_SetGetUpdateRemove_WorksCorrectly()
    {
        // Arrange
        const string key = "complete-loop-key";
        var initialValue = new TestCacheItem { Id = 1, Name = "Initial Value" };
        var updatedValue = new TestCacheItem { Id = 1, Name = "Updated Value" };

        // Step 1: Set initial value
        var setResult = await _cacheService.SetAsync(key, initialValue);
        Assert.True(setResult.IsSuccess);

        // Step 2: Get value
        var getResult = await _cacheService.GetAsync<TestCacheItem>(key);
        Assert.True(getResult.IsSuccess);
        Assert.True(getResult.HasValue);
        Assert.Equal("Initial Value", getResult.Value?.Name);

        // Step 3: Update value (set with new value)
        var updateResult = await _cacheService.SetAsync(key, updatedValue);
        Assert.True(updateResult.IsSuccess);

        // Verify update
        var afterUpdate = await _cacheService.GetAsync<TestCacheItem>(key);
        Assert.True(afterUpdate.HasValue);
        Assert.Equal("Updated Value", afterUpdate.Value?.Name);

        // Step 4: Remove value
        var removeResult = await _cacheService.RemoveAsync(key);
        Assert.True(removeResult.IsSuccess);

        // Step 5: Verify removal
        var afterRemove = await _cacheService.GetAsync<TestCacheItem>(key);
        Assert.False(afterRemove.HasValue);
    }

    #endregion

    #region Set Operations Tests

    [Fact]
    public async Task SetAsync_WithSimpleValue_StoresCorrectly()
    {
        // Arrange
        const string key = "simple-string";
        const string value = "Hello, Cache!";

        // Act
        var result = await _cacheService.SetAsync(key, value);

        // Assert
        Assert.True(result.IsSuccess);

        var retrieved = await _cacheService.GetAsync<string>(key);
        Assert.True(retrieved.HasValue);
        Assert.Equal(value, retrieved.Value);
    }

    [Fact]
    public async Task SetAsync_WithComplexObject_StoresCorrectly()
    {
        // Arrange
        const string key = "complex-object";
        var value = new TestCacheItem
        {
            Id = 42,
            Name = "Complex Object",
            Tags = ["tag1", "tag2", "tag3"],
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _cacheService.SetAsync(key, value);

        // Assert
        Assert.True(result.IsSuccess);

        var retrieved = await _cacheService.GetAsync<TestCacheItem>(key);
        Assert.True(retrieved.HasValue);
        Assert.Equal(42, retrieved.Value?.Id);
        Assert.Equal("Complex Object", retrieved.Value?.Name);
        Assert.Equal(3, retrieved.Value?.Tags?.Count);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ExpiresCorrectly()
    {
        // Arrange
        const string key = "expiring-key";
        const string value = "Will expire";
        var options = CacheEntryOptions.WithExpiration(TimeSpan.FromMilliseconds(100));

        // Act
        await _cacheService.SetAsync(key, value, options);

        // Verify exists before expiration
        var beforeExpire = await _cacheService.GetAsync<string>(key);
        Assert.True(beforeExpire.HasValue);

        // Wait for expiration
        await Task.Delay(150);

        // Verify expired
        var afterExpire = await _cacheService.GetAsync<string>(key);
        Assert.False(afterExpire.HasValue);
    }

    [Fact]
    public async Task SetAsync_WithNullValue_StoresNull()
    {
        // Arrange
        const string key = "null-value-key";

        // Act
        var result = await _cacheService.SetAsync<string?>(key, null);

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Get Operations Tests

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNoValue()
    {
        // Act
        var result = await _cacheService.GetAsync<string>("non-existent-key");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.HasValue);
    }

    [Fact]
    public async Task GetOrSetAsync_NonExistentKey_CallsFactory()
    {
        // Arrange
        const string key = $"get-or-set-{nameof(GetOrSetAsync_NonExistentKey_CallsFactory)}";
        var factoryCallCount = 0;

        // Act
        var result = await _cacheService.GetOrSetAsync(key, async () =>
        {
            factoryCallCount++;
            await Task.Delay(1);
            return "Factory Value";
        });

        // Assert
        Assert.Equal("Factory Value", result);
        Assert.Equal(1, factoryCallCount);

        // Second call should use cache
        var secondResult = await _cacheService.GetOrSetAsync(key, async () =>
        {
            factoryCallCount++;
            await Task.Delay(1);
            return "Should Not Be Called";
        });

        Assert.Equal("Factory Value", secondResult);
        Assert.Equal(1, factoryCallCount);
    }

    [Fact]
    public async Task GetOrSetAsync_ExistingKey_DoesNotCallFactory()
    {
        // Arrange
        const string key = $"get-or-set-existing-{nameof(GetOrSetAsync_ExistingKey_DoesNotCallFactory)}";
        await _cacheService.SetAsync(key, "Existing Value");
        var factoryCalled = false;

        // Act
        var result = await _cacheService.GetOrSetAsync(key, async () =>
        {
            factoryCalled = true;
            await Task.Delay(1);
            return "Factory Value";
        });

        // Assert
        Assert.Equal("Existing Value", result);
        Assert.False(factoryCalled);
    }

    #endregion

    #region Remove Operations Tests

    [Fact]
    public async Task RemoveAsync_ExistingKey_RemovesSuccessfully()
    {
        // Arrange
        const string key = "to-remove";
        await _cacheService.SetAsync(key, "Value to remove");

        // Verify exists
        var beforeRemove = await _cacheService.GetAsync<string>(key);
        Assert.True(beforeRemove.HasValue);

        // Act
        var result = await _cacheService.RemoveAsync(key);

        // Assert
        Assert.True(result.IsSuccess);

        var afterRemove = await _cacheService.GetAsync<string>(key);
        Assert.False(afterRemove.HasValue);
    }

    [Fact]
    public async Task RemoveAsync_NonExistentKey_ReturnsSuccess()
    {
        // Act
        var result = await _cacheService.RemoveAsync("non-existent-key-to-remove");

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region MemoryCacheProvider Direct Tests

    [Fact]
    public async Task MemoryCacheProvider_ExistsAsync_ReturnsTrueForExistingKey()
    {
        // Arrange
        const string key = "exists-check";
        await _memoryProvider.SetAsync(key, "value");

        // Act
        var exists = await _memoryProvider.ExistsAsync(key);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task MemoryCacheProvider_ExistsAsync_ReturnsFalseForMissingKey()
    {
        // Act
        var exists = await _memoryProvider.ExistsAsync("missing-key");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task MemoryCacheProvider_GetManyAsync_ReturnsMultipleValues()
    {
        // Arrange
        await _memoryProvider.SetAsync("many-1", "value1");
        await _memoryProvider.SetAsync("many-2", "value2");
        await _memoryProvider.SetAsync("many-3", "value3");

        // Act
        var results = await _memoryProvider.GetManyAsync<string>(["many-1", "many-2", "many-3"]);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("value1", results["many-1"]);
        Assert.Equal("value2", results["many-2"]);
        Assert.Equal("value3", results["many-3"]);
    }

    [Fact]
    public void MemoryCacheProvider_IsConfigured_ReturnsTrue()
    {
        // Act & Assert
        Assert.True(_memoryProvider.IsConfigured());
    }

    [Fact]
    public void MemoryCacheProvider_Name_ReturnsMemory()
    {
        // Act & Assert
        Assert.Equal("memory", _memoryProvider.Name);
    }

    #endregion

    #region Test Models

    private class TestCacheItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string>? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion
}
