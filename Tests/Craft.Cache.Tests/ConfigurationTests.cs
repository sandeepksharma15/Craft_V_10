using Craft.Cache;
using System.ComponentModel.DataAnnotations;

namespace Craft.Cache.Tests;

public class CacheOptionsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var options = new CacheOptions();

        // Assert
        Assert.Equal("memory", options.Provider);
        Assert.Equal(TimeSpan.FromHours(1), options.DefaultExpiration);
        Assert.Equal(TimeSpan.FromMinutes(30), options.DefaultSlidingExpiration);
        Assert.True(options.EnableStatistics);
        Assert.Equal("__CRAFT__", options.KeyPrefix);
    }

    [Fact]
    public void Validate_WithValidOptions_Passes()
    {
        // Arrange
        var options = new CacheOptions
        {
            Provider = "memory",
            DefaultExpiration = TimeSpan.FromHours(1)
        };
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WithRedisProviderAndNoSettings_Fails()
    {
        // Arrange
        var options = new CacheOptions
        {
            Provider = "redis",
            Redis = null
        };
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Redis settings are required"));
    }

    [Fact]
    public void Validate_WithHybridProviderAndNoSettings_Fails()
    {
        // Arrange
        var options = new CacheOptions
        {
            Provider = "hybrid",
            Hybrid = null
        };
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Hybrid settings are required"));
    }

    [Theory]
    [InlineData("memory")]
    [InlineData("redis")]
    [InlineData("hybrid")]
    [InlineData("null")]
    public void Provider_CanBeSetToVariousValues(string provider)
    {
        // Arrange & Act
        var options = new CacheOptions
        {
            Provider = provider
        };

        // Assert
        Assert.Equal(provider, options.Provider);
    }
}

public class MemoryCacheSettingsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var settings = new MemoryCacheSettings();

        // Assert
        Assert.Null(settings.SizeLimit);
        Assert.Equal(0.25, settings.CompactionPercentage);
        Assert.Equal(TimeSpan.FromMinutes(1), settings.ExpirationScanFrequency);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1024)]
    [InlineData(int.MaxValue)]
    public void SizeLimit_CanBeSetToValidValues(long sizeLimit)
    {
        // Arrange & Act
        var settings = new MemoryCacheSettings
        {
            SizeLimit = sizeLimit
        };

        // Assert
        Assert.Equal(sizeLimit, settings.SizeLimit);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void CompactionPercentage_CanBeSetToValidValues(double percentage)
    {
        // Arrange & Act
        var settings = new MemoryCacheSettings
        {
            CompactionPercentage = percentage
        };

        // Assert
        Assert.Equal(percentage, settings.CompactionPercentage);
    }
}

public class RedisCacheSettingsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var settings = new RedisCacheSettings();

        // Assert
        Assert.Equal(0, settings.Database);
        Assert.Equal("craft:", settings.InstanceName);
        Assert.Equal(5000, settings.ConnectTimeout);
        Assert.Equal(5000, settings.SyncTimeout);
        Assert.Equal(3, settings.RetryCount);
        Assert.False(settings.UseSsl);
    }

    [Fact]
    public void Validate_WithNoConnectionString_Fails()
    {
        // Arrange
        var settings = new RedisCacheSettings
        {
            ConnectionString = null
        };
        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithValidConnectionString_Passes()
    {
        // Arrange
        var settings = new RedisCacheSettings
        {
            ConnectionString = "localhost:6379"
        };
        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(15)]
    public void Database_CanBeSetToValidValues(int database)
    {
        // Arrange & Act
        var settings = new RedisCacheSettings
        {
            Database = database
        };

        // Assert
        Assert.Equal(database, settings.Database);
    }
}

public class HybridCacheSettingsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var settings = new HybridCacheSettings();

        // Assert
        Assert.Equal("memory", settings.L1Provider);
        Assert.Equal("redis", settings.L2Provider);
        Assert.Equal(TimeSpan.FromMinutes(5), settings.L1Expiration);
        Assert.True(settings.SyncL1OnL2Update);
    }

    [Fact]
    public void Validate_WithSameL1AndL2Providers_Fails()
    {
        // Arrange
        var settings = new HybridCacheSettings
        {
            L1Provider = "memory",
            L2Provider = "memory"
        };
        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("must be different"));
    }

    [Fact]
    public void Validate_WithDifferentProviders_Passes()
    {
        // Arrange
        var settings = new HybridCacheSettings
        {
            L1Provider = "memory",
            L2Provider = "redis"
        };
        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        // Assert
        Assert.True(isValid);
    }
}
