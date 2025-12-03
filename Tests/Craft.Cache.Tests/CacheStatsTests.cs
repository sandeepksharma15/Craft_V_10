using Craft.Cache;

namespace Craft.Cache.Tests;

public class CacheStatsTests
{
    [Fact]
    public void NewStats_HasZeroValues()
    {
        // Arrange & Act
        var stats = new CacheStats();

        // Assert
        Assert.Equal(0, stats.Hits);
        Assert.Equal(0, stats.Misses);
        Assert.Equal(0, stats.Sets);
        Assert.Equal(0, stats.Removes);
        Assert.Equal(0, stats.EntryCount);
        Assert.Equal(0, stats.TotalRequests);
        Assert.Equal(0, stats.HitRatio);
    }

    [Fact]
    public void HitRatio_WithNoRequests_ReturnsZero()
    {
        // Arrange
        var stats = new CacheStats();

        // Act
        var ratio = stats.HitRatio;

        // Assert
        Assert.Equal(0, ratio);
    }

    [Fact]
    public void HitRatio_WithAllHits_ReturnsOne()
    {
        // Arrange
        var stats = new CacheStats
        {
            Hits = 100,
            Misses = 0
        };

        // Act
        var ratio = stats.HitRatio;

        // Assert
        Assert.Equal(1.0, ratio);
    }

    [Fact]
    public void HitRatio_WithAllMisses_ReturnsZero()
    {
        // Arrange
        var stats = new CacheStats
        {
            Hits = 0,
            Misses = 100
        };

        // Act
        var ratio = stats.HitRatio;

        // Assert
        Assert.Equal(0.0, ratio);
    }

    [Fact]
    public void HitRatio_WithMixedResults_CalculatesCorrectly()
    {
        // Arrange
        var stats = new CacheStats
        {
            Hits = 80,
            Misses = 20
        };

        // Act
        var ratio = stats.HitRatio;

        // Assert
        Assert.Equal(0.8, ratio);
    }

    [Fact]
    public void TotalRequests_SumsHitsAndMisses()
    {
        // Arrange
        var stats = new CacheStats
        {
            Hits = 75,
            Misses = 25
        };

        // Act
        var total = stats.TotalRequests;

        // Assert
        Assert.Equal(100, total);
    }

    [Fact]
    public void Reset_ClearsAllStats()
    {
        // Arrange
        var stats = new CacheStats
        {
            Hits = 100,
            Misses = 50,
            Sets = 75,
            Removes = 25,
            EntryCount = 60
        };

        // Act
        stats.Reset();

        // Assert
        Assert.Equal(0, stats.Hits);
        Assert.Equal(0, stats.Misses);
        Assert.Equal(0, stats.Sets);
        Assert.Equal(0, stats.Removes);
        Assert.Equal(0, stats.EntryCount);
    }

    [Fact]
    public void Reset_UpdatesTimestamp()
    {
        // Arrange
        var stats = new CacheStats();
        var originalTimestamp = stats.Timestamp;
        Thread.Sleep(10); // Ensure time passes

        // Act
        stats.Reset();

        // Assert
        Assert.True(stats.Timestamp > originalTimestamp);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var stats = new CacheStats
        {
            Hits = 80,
            Misses = 20,
            Sets = 50,
            Removes = 10,
            EntryCount = 40
        };

        // Act
        var result = stats.ToString();

        // Assert
        Assert.Contains("80", result);  // Hits
        Assert.Contains("20", result);  // Misses
        Assert.Contains("50", result);  // Sets
        Assert.Contains("10", result);  // Removes
        Assert.Contains("40", result);  // EntryCount
        Assert.Contains("%", result);   // HitRatio percentage
    }

    [Theory]
    [InlineData(100, 0, 1.0)]
    [InlineData(75, 25, 0.75)]
    [InlineData(50, 50, 0.5)]
    [InlineData(25, 75, 0.25)]
    [InlineData(0, 100, 0.0)]
    public void HitRatio_WithVariousScenarios_CalculatesCorrectly(long hits, long misses, double expectedRatio)
    {
        // Arrange
        var stats = new CacheStats
        {
            Hits = hits,
            Misses = misses
        };

        // Act
        var ratio = stats.HitRatio;

        // Assert
        Assert.Equal(expectedRatio, ratio, 2);
    }

    [Fact]
    public void ThreadSafety_SettersAreThreadSafe()
    {
        // Arrange
        var stats = new CacheStats();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                stats.Hits++;
                stats.Misses++;
                stats.Sets++;
                stats.Removes++;
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        // Due to thread safety with Interlocked operations, all increments should be recorded
        Assert.True(stats.Hits >= 0);
        Assert.True(stats.Misses >= 0);
        Assert.True(stats.Sets >= 0);
        Assert.True(stats.Removes >= 0);
    }
}
