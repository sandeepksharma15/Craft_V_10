using Craft.CryptKey;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.CryptKey.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHashKeys_ResolvesHashKeysInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var hashKeys = serviceProvider.GetService<IHashKeys>();

        // Assert
        Assert.NotNull(hashKeys);
        Assert.IsType<HashKeys>(hashKeys);
    }

    [Fact]
    public void AddHashKeys_WithDefaultOptions_UsesDefaultValues()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var hashKeys = serviceProvider.GetRequiredService<IHashKeys>();
        var options = serviceProvider.GetRequiredService<HashKeyOptions>();

        // Assert
        Assert.NotNull(hashKeys);
        Assert.NotNull(options);
        Assert.Equal("CraftDomainKeySalt", options.Salt);
        Assert.Equal(10, options.MinHashLength);
    }

    [Fact]
    public void AddHashKeys_WithCustomOptions_UsesCustomValues()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys(options =>
        {
            options.Salt = "CustomSalt";
            options.MinHashLength = 20;
            options.Alphabet = "abcdefghijklmnopqrstuvwxyz";
        });
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<HashKeyOptions>();

        // Assert
        Assert.Equal("CustomSalt", options.Salt);
        Assert.Equal(20, options.MinHashLength);
        Assert.Equal("abcdefghijklmnopqrstuvwxyz", options.Alphabet);
    }

    [Fact]
    public void AddHashKeys_RegistersSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var hashKeys1 = serviceProvider.GetRequiredService<IHashKeys>();
        var hashKeys2 = serviceProvider.GetRequiredService<IHashKeys>();

        // Assert
        Assert.Same(hashKeys1, hashKeys2);
    }

    [Fact]
    public void AddHashKeys_RegistersHashKeyOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetService<HashKeyOptions>();

        // Assert
        Assert.NotNull(options);
    }

    [Fact]
    public void AddHashKeys_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddHashKeys();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddHashKeys_CanEncodeAndDecode()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();
        var serviceProvider = services.BuildServiceProvider();
        var hashKeys = serviceProvider.GetRequiredService<IHashKeys>();
        long originalValue = 98765;

        // Act
        var encoded = hashKeys.EncodeLong(originalValue);
        var decoded = hashKeys.DecodeLong(encoded);

        // Assert
        Assert.Equal(originalValue, decoded[0]);
    }

    [Fact]
    public void AddHashKeys_WithPartialCustomOptions_MergesWithDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys(options =>
        {
            options.Salt = "OnlyCustomSalt";
        });
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<HashKeyOptions>();

        // Assert
        Assert.Equal("OnlyCustomSalt", options.Salt);
        Assert.Equal(10, options.MinHashLength);
        Assert.Equal(HashidsNet.Hashids.DEFAULT_ALPHABET, options.Alphabet);
        Assert.Equal(HashidsNet.Hashids.DEFAULT_SEPS, options.Steps);
    }

    [Fact]
    public void AddHashKeys_MultipleCalls_LastConfigurationWins()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys(options => options.Salt = "FirstSalt");
        services.AddHashKeys(options => options.Salt = "SecondSalt");
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<HashKeyOptions>();

        // Assert
        Assert.Equal("SecondSalt", options.Salt);
    }

    [Fact]
    public void AddHashKeys_WithNullConfiguration_UsesDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys(null!);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<HashKeyOptions>();

        // Assert
        Assert.Equal("CraftDomainKeySalt", options.Salt);
        Assert.Equal(10, options.MinHashLength);
    }
}
