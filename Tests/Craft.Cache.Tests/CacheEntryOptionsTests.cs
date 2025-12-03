using Craft.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace Craft.Cache.Tests;

public class CacheEntryOptionsTests
{
    [Fact]
    public void DefaultOptions_CreatesOptionsWithDefaults()
    {
        // Arrange & Act
        var options = CacheEntryOptions.DefaultOptions;

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.AbsoluteExpirationRelativeToNow);
        Assert.Equal(TimeSpan.FromHours(1), options.AbsoluteExpirationRelativeToNow);
        Assert.NotNull(options.SlidingExpiration);
        Assert.Equal(TimeSpan.FromMinutes(30), options.SlidingExpiration);
        Assert.Equal(CacheItemPriority.Normal, options.Priority);
    }

    [Fact]
    public void WithExpiration_CreatesOptionsWithSpecifiedExpiration()
    {
        // Arrange
        var expiration = TimeSpan.FromMinutes(10);

        // Act
        var options = CacheEntryOptions.WithExpiration(expiration);

        // Assert
        Assert.Equal(expiration, options.AbsoluteExpirationRelativeToNow);
        Assert.Equal(CacheItemPriority.Normal, options.Priority);
    }

    [Fact]
    public void WithSlidingExpiration_CreatesOptionsWithSliding()
    {
        // Arrange
        var slidingExpiration = TimeSpan.FromMinutes(15);

        // Act
        var options = CacheEntryOptions.WithSlidingExpiration(slidingExpiration);

        // Assert
        Assert.Equal(slidingExpiration, options.SlidingExpiration);
        Assert.Equal(CacheItemPriority.Normal, options.Priority);
    }

    [Fact]
    public void ToMemoryCacheEntryOptions_ConvertsCorrectly()
    {
        // Arrange
        var options = new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.High,
            Size = 100
        };

        // Act
        var memoryOptions = options.ToMemoryCacheEntryOptions();

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(10), memoryOptions.AbsoluteExpirationRelativeToNow);
        Assert.Equal(TimeSpan.FromMinutes(5), memoryOptions.SlidingExpiration);
        Assert.Equal(CacheItemPriority.High, memoryOptions.Priority);
        Assert.Equal(100, memoryOptions.Size);
    }

    [Fact]
    public void ToMemoryCacheEntryOptions_WithAbsoluteExpiration_ConvertsCorrectly()
    {
        // Arrange
        var absoluteTime = DateTimeOffset.UtcNow.AddHours(1);
        var options = new CacheEntryOptions
        {
            AbsoluteExpiration = absoluteTime
        };

        // Act
        var memoryOptions = options.ToMemoryCacheEntryOptions();

        // Assert
        Assert.Equal(absoluteTime, memoryOptions.AbsoluteExpiration);
    }

    [Theory]
    [InlineData(CacheItemPriority.Low)]
    [InlineData(CacheItemPriority.Normal)]
    [InlineData(CacheItemPriority.High)]
    [InlineData(CacheItemPriority.NeverRemove)]
    public void Priority_CanBeSetToAnyValue(CacheItemPriority priority)
    {
        // Arrange & Act
        var options = new CacheEntryOptions
        {
            Priority = priority
        };

        // Assert
        Assert.Equal(priority, options.Priority);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Size_CanBeSet(long size)
    {
        // Arrange & Act
        var options = new CacheEntryOptions
        {
            Size = size
        };

        // Assert
        Assert.Equal(size, options.Size);
    }
}
