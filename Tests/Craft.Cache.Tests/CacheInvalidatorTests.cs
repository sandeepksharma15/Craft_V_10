using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Craft.Cache.Tests;

public class CacheInvalidatorTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ICacheKeyGenerator> _keyGeneratorMock;
    private readonly Mock<ILogger<CacheInvalidator>> _loggerMock;
    private readonly CacheInvalidator _invalidator;

    public CacheInvalidatorTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _keyGeneratorMock = new Mock<ICacheKeyGenerator>();
        _loggerMock = new Mock<ILogger<CacheInvalidator>>();

        _invalidator = new CacheInvalidator(
            _cacheServiceMock.Object,
            _keyGeneratorMock.Object,
            _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange, Act & Assert
        Assert.NotNull(_invalidator);
    }

    [Fact]
    public void Constructor_WithNullCacheService_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CacheInvalidator(null!, _keyGeneratorMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullKeyGenerator_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CacheInvalidator(_cacheServiceMock.Object, null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CacheInvalidator(_cacheServiceMock.Object, _keyGeneratorMock.Object, null!));
    }

    #endregion

    #region InvalidateEntityAsync Tests

    [Fact]
    public async Task InvalidateEntityAsync_WithValidId_InvalidatesCache()
    {
        // Arrange
        _keyGeneratorMock.Setup(k => k.GenerateEntityKey<TestEntity>(123))
            .Returns("test:TestEntity:123");
        _cacheServiceMock.Setup(c => c.RemoveAsync("test:TestEntity:123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var count = await _invalidator.InvalidateEntityAsync<TestEntity>(123);

        // Assert
        Assert.Equal(1, count);
        _cacheServiceMock.Verify(c => c.RemoveAsync("test:TestEntity:123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateEntityAsync_WithNullId_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _invalidator.InvalidateEntityAsync<TestEntity>(null!));
    }

    [Fact]
    public async Task InvalidateEntityAsync_WithFailedRemoval_ReturnsZero()
    {
        // Arrange
        _keyGeneratorMock.Setup(k => k.GenerateEntityKey<TestEntity>(123))
            .Returns("test:TestEntity:123");
        _cacheServiceMock.Setup(c => c.RemoveAsync("test:TestEntity:123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Failure("Cache error"));

        // Act
        var count = await _invalidator.InvalidateEntityAsync<TestEntity>(123);

        // Assert
        Assert.Equal(0, count);
    }

    #endregion

    #region InvalidateEntityTypeAsync Tests

    [Fact]
    public async Task InvalidateEntityTypeAsync_RemovesAllEntitiesOfType()
    {
        // Arrange
        _keyGeneratorMock.Setup(k => k.GenerateEntityPattern<TestEntity>(null))
            .Returns("test:TestEntity:*");
        _cacheServiceMock.Setup(c => c.RemoveByPatternAsync("test:TestEntity:*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var count = await _invalidator.InvalidateEntityTypeAsync<TestEntity>();

        // Assert
        Assert.Equal(5, count);
        _cacheServiceMock.Verify(c => c.RemoveByPatternAsync("test:TestEntity:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateEntityTypeAsync_WithNoMatches_ReturnsZero()
    {
        // Arrange
        _keyGeneratorMock.Setup(k => k.GenerateEntityPattern<TestEntity>(null))
            .Returns("test:TestEntity:*");
        _cacheServiceMock.Setup(c => c.RemoveByPatternAsync("test:TestEntity:*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var count = await _invalidator.InvalidateEntityTypeAsync<TestEntity>();

        // Assert
        Assert.Equal(0, count);
    }

    #endregion

    #region InvalidateKeysAsync Tests

    [Fact]
    public async Task InvalidateKeysAsync_WithMultipleKeys_InvalidatesAll()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var count = await _invalidator.InvalidateKeysAsync(keys);

        // Assert
        Assert.Equal(3, count);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task InvalidateKeysAsync_WithSomeFailures_ReturnsSuccessCount()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        _cacheServiceMock.SetupSequence(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success())
            .ReturnsAsync(CacheResult.Failure("Error"))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var count = await _invalidator.InvalidateKeysAsync(keys);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task InvalidateKeysAsync_WithNullKeys_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _invalidator.InvalidateKeysAsync(null!));
    }

    [Fact]
    public async Task InvalidateKeysAsync_WithEmptyKeys_ReturnsZero()
    {
        // Arrange
        var keys = Array.Empty<string>();

        // Act
        var count = await _invalidator.InvalidateKeysAsync(keys);

        // Assert
        Assert.Equal(0, count);
    }

    #endregion

    #region InvalidatePatternAsync Tests

    [Fact]
    public async Task InvalidatePatternAsync_WithPattern_InvalidatesMatches()
    {
        // Arrange
        _cacheServiceMock.Setup(c => c.RemoveByPatternAsync("user:*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        // Act
        var count = await _invalidator.InvalidatePatternAsync("user:*");

        // Assert
        Assert.Equal(10, count);
        _cacheServiceMock.Verify(c => c.RemoveByPatternAsync("user:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InvalidatePatternAsync_WithEmptyPattern_ThrowsArgumentException(string? pattern)
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _invalidator.InvalidatePatternAsync(pattern!));
    }

    [Fact]
    public async Task InvalidatePatternAsync_WithNullPattern_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _invalidator.InvalidatePatternAsync(null!));
    }

    #endregion

    #region InvalidateAsync with Strategy Tests

    [Fact]
    public async Task InvalidateAsync_WithPatternStrategy_InvalidatesCorrectly()
    {
        // Arrange
        var strategy = new PatternInvalidationStrategy("pattern1:*", "pattern2:*");
        _cacheServiceMock.Setup(c => c.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var count = await _invalidator.InvalidateAsync(strategy);

        // Assert
        Assert.Equal(10, count); // 5 per pattern × 2 patterns
        _cacheServiceMock.Verify(c => c.RemoveByPatternAsync("pattern1:*", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPatternAsync("pattern2:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateAsync_WithEntityTypeStrategy_InvalidatesCorrectly()
    {
        // Arrange
        _keyGeneratorMock.Setup(k => k.GenerateEntityPattern<TestEntity>(null))
            .Returns("test:TestEntity:*");
        var strategy = new EntityTypeInvalidationStrategy<TestEntity>(_keyGeneratorMock.Object);
        _cacheServiceMock.Setup(c => c.RemoveByPatternAsync("test:TestEntity:*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        // Act
        var count = await _invalidator.InvalidateAsync(strategy);

        // Assert
        Assert.Equal(15, count);
    }

    [Fact]
    public async Task InvalidateAsync_WithEntityIdStrategy_InvalidatesSpecificEntities()
    {
        // Arrange
        _keyGeneratorMock.Setup(k => k.GenerateEntityKey<TestEntity>(1))
            .Returns("test:TestEntity:1");
        _keyGeneratorMock.Setup(k => k.GenerateEntityKey<TestEntity>(2))
            .Returns("test:TestEntity:2");
        var strategy = new EntityIdInvalidationStrategy<TestEntity>(_keyGeneratorMock.Object, 1, 2);
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());

        // Act
        var count = await _invalidator.InvalidateAsync(strategy);

        // Assert
        Assert.Equal(2, count);
        _cacheServiceMock.Verify(c => c.RemoveAsync("test:TestEntity:1", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("test:TestEntity:2", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateAsync_WithNullStrategy_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _invalidator.InvalidateAsync(null!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task InvalidateAsync_WithDependentEntityStrategy_InvalidatesRelatedEntities()
    {
        // Arrange
        _keyGeneratorMock.Setup(k => k.GenerateEntityKey<TestEntity>(1))
            .Returns("test:TestEntity:1");
        _keyGeneratorMock.Setup(k => k.GenerateEntityPattern<RelatedEntity>(null))
            .Returns("test:RelatedEntity:*");

        var strategy = new DependentEntityInvalidationStrategy(_keyGeneratorMock.Object)
            .AddDependency<TestEntity>(1)
            .AddTypeDependency<RelatedEntity>();

        _cacheServiceMock.Setup(c => c.RemoveAsync("test:TestEntity:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CacheResult.Success());
        _cacheServiceMock.Setup(c => c.RemoveByPatternAsync("test:RelatedEntity:*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var count = await _invalidator.InvalidateAsync(strategy);

        // Assert
        Assert.Equal(6, count); // 1 specific entity + 5 related entities
    }

    #endregion
}

public class TestEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class RelatedEntity
{
    public int Id { get; set; }
    public int TestEntityId { get; set; }
}
