using Craft.Cache;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Cache.Tests;

public class CacheServiceTests
{
    private readonly Mock<ICacheProviderFactory> _factoryMock;
    private readonly Mock<ICacheProvider> _providerMock;
    private readonly Mock<ILogger<CacheService>> _loggerMock;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _factoryMock = new Mock<ICacheProviderFactory>();
        _providerMock = new Mock<ICacheProvider>();
        _loggerMock = new Mock<ILogger<CacheService>>();

        _factoryMock.Setup(f => f.GetDefaultProvider()).Returns(_providerMock.Object);
        _cacheService = new CacheService(_factoryMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var factoryMock = new Mock<ICacheProviderFactory>();
        var providerMock = new Mock<ICacheProvider>();
        var loggerMock = new Mock<ILogger<CacheService>>();
        
        factoryMock.Setup(f => f.GetDefaultProvider()).Returns(providerMock.Object);

        // Act
        var service = new CacheService(factoryMock.Object, loggerMock.Object);

        // Assert
        Assert.NotNull(service);
        factoryMock.Verify(f => f.GetDefaultProvider(), Times.Once);
    }

    [Fact]
    public void Constructor_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CacheService(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CacheService(_factoryMock.Object, null!));
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithExistingKey_ReturnsSuccessWithValue()
    {
        // Arrange
        var expectedValue = "test-value";
        _providerMock.Setup(p => p.GetAsync<string>("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult<string>.Success(expectedValue));

        // Act
        var result = await _cacheService.GetAsync<string>("key1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.HasValue);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public async Task GetAsync_WithNonExistingKey_ReturnsSuccessWithoutValue()
    {
        // Arrange
        _providerMock.Setup(p => p.GetAsync<string>("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult<string>.Success());

        // Act
        var result = await _cacheService.GetAsync<string>("key1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.HasValue);
    }

    [Fact]
    public async Task GetAsync_WithProviderException_ReturnsFailure()
    {
        // Arrange
        _providerMock.Setup(p => p.GetAsync<string>("key1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.GetAsync<string>("key1");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to get cache entry", result.ErrorMessage);
    }

    [Theory]
    [InlineData("key1")]
    [InlineData("key2")]
    [InlineData("user:123")]
    public async Task GetAsync_WithVariousKeys_CallsProvider(string key)
    {
        // Arrange
        _providerMock.Setup(p => p.GetAsync<int>(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult<int>.Success(42));

        // Act
        await _cacheService.GetAsync<int>(key);

        // Assert
        _providerMock.Verify(p => p.GetAsync<int>(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region SetAsync Tests

    [Fact]
    public async Task SetAsync_WithValidValue_ReturnsSuccess()
    {
        // Arrange
        _providerMock.Setup(p => p.SetAsync("key1", "value1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var result = await _cacheService.SetAsync("key1", "value1");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SetAsync_WithOptions_PassesOptionsToProvider()
    {
        // Arrange
        var options = CacheEntryOptions.WithExpiration(TimeSpan.FromMinutes(5));
        _providerMock.Setup(p => p.SetAsync("key1", "value1", options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var result = await _cacheService.SetAsync("key1", "value1", options);

        // Assert
        Assert.True(result.IsSuccess);
        _providerMock.Verify(p => p.SetAsync("key1", "value1", options, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithProviderException_ReturnsFailure()
    {
        // Arrange
        _providerMock.Setup(p => p.SetAsync("key1", "value1", null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.SetAsync("key1", "value1");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to set cache entry", result.ErrorMessage);
    }

    [Theory]
    [InlineData("key1", 42)]
    [InlineData("key2", 100)]
    [InlineData("key3", -1)]
    public async Task SetAsync_WithVariousValues_CallsProvider(string key, int value)
    {
        // Arrange
        _providerMock.Setup(p => p.SetAsync(key, value, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
        _providerMock.Verify(p => p.SetAsync(key, value, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_WithExistingKey_ReturnsSuccess()
    {
        // Arrange
        _providerMock.Setup(p => p.RemoveAsync("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var result = await _cacheService.RemoveAsync("key1");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RemoveAsync_WithProviderException_ReturnsFailure()
    {
        // Arrange
        _providerMock.Setup(p => p.RemoveAsync("key1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.RemoveAsync("key1");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to remove cache entry", result.ErrorMessage);
    }

    [Theory]
    [InlineData("key1")]
    [InlineData("key2")]
    [InlineData("user:123")]
    public async Task RemoveAsync_WithVariousKeys_CallsProvider(string key)
    {
        // Arrange
        _providerMock.Setup(p => p.RemoveAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _providerMock.Verify(p => p.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetOrSetAsync Tests

    [Fact]
    public async Task GetOrSetAsync_WithCacheHit_ReturnsValueWithoutCallingFactory()
    {
        // Arrange
        var factoryCalled = false;
        _providerMock.Setup(p => p.GetAsync<string>("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult<string>.Success("cached-value"));

        // Act
        var result = await _cacheService.GetOrSetAsync("key1", () =>
        {
            factoryCalled = true;
            return Task.FromResult("new-value");
        }, null);

        // Assert
        Assert.Equal("cached-value", result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public async Task GetOrSetAsync_WithCacheMiss_CallsFactoryAndSetsValue()
    {
        // Arrange
        var factoryCalled = false;
        _providerMock.Setup(p => p.GetAsync<string>("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult<string>.Success());
        _providerMock.Setup(p => p.SetAsync("key1", "new-value", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var result = await _cacheService.GetOrSetAsync("key1", () =>
        {
            factoryCalled = true;
            return Task.FromResult("new-value");
        }, null);

        // Assert
        Assert.Equal("new-value", result);
        Assert.True(factoryCalled);
        _providerMock.Verify(p => p.SetAsync("key1", "new-value", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrSetAsync_WithOptions_PassesOptionsToProvider()
    {
        // Arrange
        var options = CacheEntryOptions.WithExpiration(TimeSpan.FromMinutes(5));
        _providerMock.Setup(p => p.GetAsync<string>("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult<string>.Success());
        _providerMock.Setup(p => p.SetAsync("key1", "new-value", options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        await _cacheService.GetOrSetAsync("key1", () => Task.FromResult("new-value"), options);

        // Assert
        _providerMock.Verify(p => p.SetAsync("key1", "new-value", options, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrSetAsync_WithFactoryException_ThrowsException()
    {
        // Arrange
        _providerMock.Setup(p => p.GetAsync<string>("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult<string>.Success());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await _cacheService.GetOrSetAsync<string>("key1", () => throw new Exception("Factory error"), null));
    }

    #endregion

    #region GetManyAsync Tests

    [Fact]
    public async Task GetManyAsync_WithMultipleKeys_ReturnsAllValues()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        var expectedResult = new Dictionary<string, string?>
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
            ["key3"] = "value3"
        };
        _providerMock.Setup(p => p.GetManyAsync<string>(keys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _cacheService.GetManyAsync<string>(keys);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
        Assert.Equal("value3", result["key3"]);
    }

    [Fact]
    public async Task GetManyAsync_WithProviderException_ReturnsEmptyDictionary()
    {
        // Arrange
        var keys = new[] { "key1", "key2" };
        _providerMock.Setup(p => p.GetManyAsync<string>(keys, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.GetManyAsync<string>(keys);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region SetManyAsync Tests

    [Fact]
    public async Task SetManyAsync_WithMultipleItems_ReturnsSuccess()
    {
        // Arrange
        var items = new Dictionary<string, string?>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };
        _providerMock.Setup(p => p.SetManyAsync(items, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var result = await _cacheService.SetManyAsync(items);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SetManyAsync_WithProviderException_ReturnsFailure()
    {
        // Arrange
        var items = new Dictionary<string, string?> { ["key1"] = "value1" };
        _providerMock.Setup(p => p.SetManyAsync(items, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.SetManyAsync(items);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to set multiple cache entries", result.ErrorMessage);
    }

    #endregion

    #region RemoveByPatternAsync Tests

    [Fact]
    public async Task RemoveByPatternAsync_WithPattern_ReturnsCount()
    {
        // Arrange
        _providerMock.Setup(p => p.RemoveByPatternAsync("user:*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _cacheService.RemoveByPatternAsync("user:*");

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task RemoveByPatternAsync_WithProviderException_ReturnsZero()
    {
        // Arrange
        _providerMock.Setup(p => p.RemoveByPatternAsync("user:*", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.RemoveByPatternAsync("user:*");

        // Assert
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData("user:*")]
    [InlineData("product:*")]
    [InlineData("*:123")]
    public async Task RemoveByPatternAsync_WithVariousPatterns_CallsProvider(string pattern)
    {
        // Arrange
        _providerMock.Setup(p => p.RemoveByPatternAsync(pattern, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _cacheService.RemoveByPatternAsync(pattern);

        // Assert
        _providerMock.Verify(p => p.RemoveByPatternAsync(pattern, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        _providerMock.Setup(p => p.ExistsAsync("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _cacheService.ExistsAsync("key1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        _providerMock.Setup(p => p.ExistsAsync("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _cacheService.ExistsAsync("key1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WithProviderException_ReturnsFalse()
    {
        // Arrange
        _providerMock.Setup(p => p.ExistsAsync("key1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.ExistsAsync("key1");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetStatsAsync Tests

    [Fact]
    public async Task GetStatsAsync_ReturnsStats()
    {
        // Arrange
        var expectedStats = new CacheStats
        {
            Hits = 100,
            Misses = 20,
            Sets = 50,
            Removes = 10,
            EntryCount = 40
        };
        _providerMock.Setup(p => p.GetStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _cacheService.GetStatsAsync();

        // Assert
        Assert.Equal(100, result.Hits);
        Assert.Equal(20, result.Misses);
        Assert.Equal(50, result.Sets);
        Assert.Equal(10, result.Removes);
        Assert.Equal(40, result.EntryCount);
    }

    [Fact]
    public async Task GetStatsAsync_WithProviderException_ReturnsEmptyStats()
    {
        // Arrange
        _providerMock.Setup(p => p.GetStatsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.GetStatsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Hits);
        Assert.Equal(0, result.Misses);
    }

    #endregion

    #region ClearAsync Tests

    [Fact]
    public async Task ClearAsync_ReturnsSuccess()
    {
        // Arrange
        _providerMock.Setup(p => p.ClearAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var result = await _cacheService.ClearAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ClearAsync_WithProviderException_ReturnsFailure()
    {
        // Arrange
        _providerMock.Setup(p => p.ClearAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.ClearAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to clear cache", result.ErrorMessage);
    }

    #endregion

    #region RefreshAsync Tests

    [Fact]
    public async Task RefreshAsync_WithExistingKey_ReturnsSuccess()
    {
        // Arrange
        _providerMock.Setup(p => p.RefreshAsync("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var result = await _cacheService.RefreshAsync("key1");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RefreshAsync_WithProviderException_ReturnsFailure()
    {
        // Arrange
        _providerMock.Setup(p => p.RefreshAsync("key1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _cacheService.RefreshAsync("key1");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to refresh cache entry", result.ErrorMessage);
    }

    #endregion

    #region Complex Type Tests

    [Fact]
    public async Task GetAsync_WithComplexType_ReturnsValue()
    {
        // Arrange
        var testObj = new TestClass { Id = 1, Name = "Test" };
        _providerMock.Setup(p => p.GetAsync<TestClass>("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult<TestClass>.Success(testObj));

        // Act
        var result = await _cacheService.GetAsync<TestClass>("key1");

        // Assert
        Assert.True(result.HasValue);
        Assert.Equal(1, result.Value?.Id);
        Assert.Equal("Test", result.Value?.Name);
    }

    [Fact]
    public async Task SetAndGetAsync_CompleteFlow_WorksCorrectly()
    {
        // Arrange
        var testObj = new TestClass { Id = 1, Name = "Test" };
        _providerMock.Setup(p => p.SetAsync("key1", testObj, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());
        _providerMock.Setup(p => p.GetAsync<TestClass>("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult<TestClass>.Success(testObj));

        // Act
        var setResult = await _cacheService.SetAsync("key1", testObj);
        var getResult = await _cacheService.GetAsync<TestClass>("key1");

        // Assert
        Assert.True(setResult.IsSuccess);
        Assert.True(getResult.HasValue);
        Assert.Equal(testObj.Id, getResult.Value?.Id);
    }

    #endregion
}

public class TestClass
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
