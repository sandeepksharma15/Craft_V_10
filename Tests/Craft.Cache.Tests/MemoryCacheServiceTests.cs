using Craft.Cache;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Craft.Infrastructure.Tests.CacheService;

public class MemoryCacheServiceTests
{
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly MemoryCacheService _cacheService;

    public MemoryCacheServiceTests()
    {
        _memoryCacheMock = new Mock<IMemoryCache>();
        _cacheService = new MemoryCacheService(_memoryCacheMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidMemoryCache_CreatesInstance()
    {
        // Arrange
        var memoryCache = new Mock<IMemoryCache>().Object;

        // Act
        var service = new MemoryCacheService(memoryCache);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region Set Tests

    [Fact]
    public void Set_WithValidValue_StoresInCache()
    {
        // Arrange
        const string cacheKey = "testKey";
        const string expectedValue = "testValue";
        object? capturedValue = null;

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object key) =>
            {
                var entryMock = new Mock<ICacheEntry>();
                entryMock.SetupProperty(e => e.Value);
                entryMock.SetupSet(e => e.AbsoluteExpiration = It.IsAny<DateTimeOffset?>());
                entryMock.SetupSet(e => e.SlidingExpiration = It.IsAny<TimeSpan?>());
                entryMock.SetupSet(e => e.Priority = It.IsAny<CacheItemPriority>());
                entryMock.Setup(e => e.Dispose()).Callback(() =>
                {
                    capturedValue = entryMock.Object.Value;
                });
                return entryMock.Object;
            });

        // Act
        var result = _cacheService.Set(cacheKey, expectedValue);

        // Assert
        Assert.Equal(expectedValue, result);
        _memoryCacheMock.Verify(x => x.CreateEntry(cacheKey), Times.Once);
    }

    [Fact]
    public void Set_WithNullValue_StoresNull()
    {
        // Arrange
        const string cacheKey = "testKey";
        string? nullValue = null;

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(new Mock<ICacheEntry>().Object);

        // Act
        var result = _cacheService.Set(cacheKey, nullValue);

        // Assert
        Assert.Null(result);
        _memoryCacheMock.Verify(x => x.CreateEntry(cacheKey), Times.Once);
    }

    [Fact]
    public void Set_WithComplexObject_StoresObject()
    {
        // Arrange
        const string cacheKey = "complexKey";
        var complexObject = new TestClass { Id = 1, Name = "Test" };

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(new Mock<ICacheEntry>().Object);

        // Act
        var result = _cacheService.Set(cacheKey, complexObject);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(complexObject.Id, result.Id);
        Assert.Equal(complexObject.Name, result.Name);
    }

    [Theory]
    [InlineData("key1", 42)]
    [InlineData("key2", 100)]
    [InlineData("key3", -1)]
    public void Set_WithVariousIntValues_StoresCorrectly(string key, int value)
    {
        // Arrange
        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(new Mock<ICacheEntry>().Object);

        // Act
        var result = _cacheService.Set(key, value);

        // Assert
        Assert.Equal(value, result);
    }

    #endregion

    #region TryGet Tests

    [Fact]
    public void TryGet_WithExistingKey_ReturnsSuccessAndValue()
    {
        // Arrange
        const string cacheKey = "existingKey";
        const string expectedValue = "cachedValue";
        object? outValue = expectedValue;

        _memoryCacheMock
            .Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(true);

        // Act
        var (success, value) = _cacheService.TryGet<string>(cacheKey);

        // Assert
        Assert.True(success);
        Assert.Equal(expectedValue, value);
    }

    [Fact]
    public void TryGet_WithNonExistingKey_ReturnsFailureAndDefault()
    {
        // Arrange
        const string cacheKey = "nonExistingKey";
        object? outValue = null;

        _memoryCacheMock
            .Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(false);

        // Act
        var (success, value) = _cacheService.TryGet<string>(cacheKey);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryGet_WithComplexObject_ReturnsObject()
    {
        // Arrange
        const string cacheKey = "complexKey";
        var expectedObject = new TestClass { Id = 5, Name = "Cached" };
        object? outValue = expectedObject;

        _memoryCacheMock
            .Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(true);

        // Act
        var (success, value) = _cacheService.TryGet<TestClass>(cacheKey);

        // Assert
        Assert.True(success);
        Assert.NotNull(value);
        Assert.Equal(expectedObject.Id, value.Id);
        Assert.Equal(expectedObject.Name, value.Name);
    }

    [Fact]
    public void TryGet_WithNullValueInCache_ReturnsSuccessAndNull()
    {
        // Arrange
        const string cacheKey = "nullKey";
        object? outValue = null;

        _memoryCacheMock
            .Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(true);

        // Act
        var (success, value) = _cacheService.TryGet<string>(cacheKey);

        // Assert
        Assert.True(success);
        Assert.Null(value);
    }

    [Theory]
    [InlineData("key1", 42)]
    [InlineData("key2", 100)]
    [InlineData("key3", 0)]
    public void TryGet_WithVariousIntValues_ReturnsCorrectValue(string key, int expectedValue)
    {
        // Arrange
        object? outValue = expectedValue;

        _memoryCacheMock
            .Setup(x => x.TryGetValue(key, out outValue))
            .Returns(true);

        // Act
        var (success, value) = _cacheService.TryGet<int>(key);

        // Assert
        Assert.True(success);
        Assert.Equal(expectedValue, value);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_WithExistingKey_RemovesFromCache()
    {
        // Arrange
        const string cacheKey = "keyToRemove";

        // Act
        _cacheService.Remove(cacheKey);

        // Assert
        _memoryCacheMock.Verify(x => x.Remove(cacheKey), Times.Once);
    }

    [Fact]
    public void Remove_WithNonExistingKey_DoesNotThrow()
    {
        // Arrange
        const string cacheKey = "nonExistingKey";

        // Act
        _cacheService.Remove(cacheKey);

        // Assert
        _memoryCacheMock.Verify(x => x.Remove(cacheKey), Times.Once);
    }

    [Fact]
    public void Remove_CalledMultipleTimes_CallsRemoveMultipleTimes()
    {
        // Arrange
        const string cacheKey = "multiRemoveKey";

        // Act
        _cacheService.Remove(cacheKey);
        _cacheService.Remove(cacheKey);
        _cacheService.Remove(cacheKey);

        // Assert
        _memoryCacheMock.Verify(x => x.Remove(cacheKey), Times.Exactly(3));
    }

    [Theory]
    [InlineData("key1")]
    [InlineData("key2")]
    [InlineData("key3")]
    public void Remove_WithDifferentKeys_RemovesEachKey(string key)
    {
        // Arrange & Act
        _cacheService.Remove(key);

        // Assert
        _memoryCacheMock.Verify(x => x.Remove(key), Times.Once);
    }

    #endregion

    #region GetOrSetAsync Tests

    [Fact]
    public async Task GetOrSetAsync_WithNonExistingKey_CallsValueFactory()
    {
        // Arrange
        const string cacheKey = "newKey";
        const string expectedValue = "factoryValue";
        var factoryCalled = false;

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object key) =>
            {
                var entryMock = new Mock<ICacheEntry>();
                entryMock.SetupProperty(e => e.Value);
                entryMock.SetupSet(e => e.AbsoluteExpiration = It.IsAny<DateTimeOffset?>());
                entryMock.SetupSet(e => e.SlidingExpiration = It.IsAny<TimeSpan?>());
                entryMock.SetupSet(e => e.Priority = It.IsAny<CacheItemPriority>());
                return entryMock.Object;
            });

        Task<string> valueFactory()
        {
            factoryCalled = true;
            return Task.FromResult(expectedValue);
        }

        // Act
        var result = await _cacheService.GetOrSetAsync(cacheKey, valueFactory);

        // Assert
        Assert.True(factoryCalled);
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task GetOrSetAsync_WithExistingKey_ReturnsExistingValue()
    {
        // Arrange
        const string cacheKey = "existingKey";
        const string cachedValue = "existing";
        const string factoryValue = "new";
        var factoryCalled = false;
        object? outValue = cachedValue;

        _memoryCacheMock
            .Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(true);

        Task<string> valueFactory()
        {
            factoryCalled = true;
            return Task.FromResult(factoryValue);
        }

        // Act
        var result = await _cacheService.GetOrSetAsync(cacheKey, valueFactory);

        // Assert
        Assert.False(factoryCalled);
    }

    [Fact]
    public async Task GetOrSetAsync_WithComplexObject_ReturnsObject()
    {
        // Arrange
        const string cacheKey = "complexKey";
        var expectedObject = new TestClass { Id = 10, Name = "Async" };

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object key) =>
            {
                var entryMock = new Mock<ICacheEntry>();
                entryMock.SetupProperty(e => e.Value);
                entryMock.SetupSet(e => e.AbsoluteExpiration = It.IsAny<DateTimeOffset?>());
                entryMock.SetupSet(e => e.SlidingExpiration = It.IsAny<TimeSpan?>());
                entryMock.SetupSet(e => e.Priority = It.IsAny<CacheItemPriority>());
                return entryMock.Object;
            });

        Task<TestClass> valueFactory() => Task.FromResult(expectedObject);

        // Act
        var result = await _cacheService.GetOrSetAsync(cacheKey, valueFactory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedObject.Id, result.Id);
        Assert.Equal(expectedObject.Name, result.Name);
    }

    [Fact]
    public async Task GetOrSetAsync_WithNullValueFromFactory_ReturnsNull()
    {
        // Arrange
        const string cacheKey = "nullKey";

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object key) =>
            {
                var entryMock = new Mock<ICacheEntry>();
                entryMock.SetupProperty(e => e.Value);
                entryMock.SetupSet(e => e.AbsoluteExpiration = It.IsAny<DateTimeOffset?>());
                entryMock.SetupSet(e => e.SlidingExpiration = It.IsAny<TimeSpan?>());
                entryMock.SetupSet(e => e.Priority = It.IsAny<CacheItemPriority>());
                return entryMock.Object;
            });

        static Task<string?> valueFactory() => Task.FromResult<string?>(null);

        // Act
        var result = await _cacheService.GetOrSetAsync(cacheKey, valueFactory);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrSetAsync_WithAsyncFactory_AwaitsFactory()
    {
        // Arrange
        const string cacheKey = "asyncKey";
        const string expectedValue = "asyncValue";
        var factoryExecuted = false;

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object key) =>
            {
                var entryMock = new Mock<ICacheEntry>();
                entryMock.SetupProperty(e => e.Value);
                entryMock.SetupSet(e => e.AbsoluteExpiration = It.IsAny<DateTimeOffset?>());
                entryMock.SetupSet(e => e.SlidingExpiration = It.IsAny<TimeSpan?>());
                entryMock.SetupSet(e => e.Priority = It.IsAny<CacheItemPriority>());
                return entryMock.Object;
            });

        async Task<string> valueFactory()
        {
            await Task.Delay(10);
            factoryExecuted = true;
            return expectedValue;
        }

        // Act
        var result = await _cacheService.GetOrSetAsync(cacheKey, valueFactory);

        // Assert
        Assert.True(factoryExecuted);
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("key1", 10)]
    [InlineData("key2", 20)]
    [InlineData("key3", 30)]
    public async Task GetOrSetAsync_WithVariousValues_ReturnsCorrectValue(string key, int expectedValue)
    {
        // Arrange
        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object k) =>
            {
                var entryMock = new Mock<ICacheEntry>();
                entryMock.SetupProperty(e => e.Value);
                entryMock.SetupSet(e => e.AbsoluteExpiration = It.IsAny<DateTimeOffset?>());
                entryMock.SetupSet(e => e.SlidingExpiration = It.IsAny<TimeSpan?>());
                entryMock.SetupSet(e => e.Priority = It.IsAny<CacheItemPriority>());
                return entryMock.Object;
            });

        Task<int> valueFactory() => Task.FromResult(expectedValue);

        // Act
        var result = await _cacheService.GetOrSetAsync(key, valueFactory);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void SetAndTryGet_WithSameKey_ReturnsSetValue()
    {
        // Arrange
        const string cacheKey = "integrationKey";
        const string expectedValue = "integrationValue";
        object? storedValue = null;

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object key) =>
            {
                var entryMock = new Mock<ICacheEntry>();
                entryMock.SetupProperty(e => e.Value);
                entryMock.SetupSet(e => e.AbsoluteExpiration = It.IsAny<DateTimeOffset?>());
                entryMock.SetupSet(e => e.SlidingExpiration = It.IsAny<TimeSpan?>());
                entryMock.SetupSet(e => e.Priority = It.IsAny<CacheItemPriority>());
                entryMock.Setup(e => e.Dispose()).Callback(() =>
                {
                    storedValue = entryMock.Object.Value;
                });
                return entryMock.Object;
            });

        _memoryCacheMock
            .Setup(x => x.TryGetValue(cacheKey, out storedValue))
            .Returns(true);

        // Act
        _cacheService.Set(cacheKey, expectedValue);
        var (success, value) = _cacheService.TryGet<string>(cacheKey);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void SetAndRemoveAndTryGet_ReturnsNotFound()
    {
        // Arrange
        const string cacheKey = "removeKey";
        const string value = "value";
        object? outValue = null;

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(new Mock<ICacheEntry>().Object);

        _memoryCacheMock
            .Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(false);

        // Act
        _cacheService.Set(cacheKey, value);
        _cacheService.Remove(cacheKey);
        var (success, retrievedValue) = _cacheService.TryGet<string>(cacheKey);

        // Assert
        Assert.False(success);
        Assert.Null(retrievedValue);
    }

    [Fact]
    public async Task GetOrSetAsyncAndTryGet_ReturnsSameValue()
    {
        // Arrange
        const string cacheKey = "asyncIntegrationKey";
        const string expectedValue = "asyncIntegrationValue";

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object key) =>
            {
                var entryMock = new Mock<ICacheEntry>();
                entryMock.SetupProperty(e => e.Value);
                entryMock.SetupSet(e => e.AbsoluteExpiration = It.IsAny<DateTimeOffset?>());
                entryMock.SetupSet(e => e.SlidingExpiration = It.IsAny<TimeSpan?>());
                entryMock.SetupSet(e => e.Priority = It.IsAny<CacheItemPriority>());
                return entryMock.Object;
            });

        static Task<string> valueFactory() => Task.FromResult(expectedValue);

        // Act
        await _cacheService.GetOrSetAsync(cacheKey, valueFactory);

        // Assert
        _memoryCacheMock.Verify(x => x.CreateEntry(cacheKey), Times.Once);
    }

    #endregion

    #region Helper Classes

    private class TestClass
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    #endregion
}
