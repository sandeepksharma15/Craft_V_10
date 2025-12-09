using Craft.Domain.HashIdentityKey;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.CryptKey.Tests;

public class KeyTypeExtensionsTests
{
    [Fact]
    public void ToHashKey_And_ToKeyType_RoundTrip_Works_For_PositiveValue()
    {
        // Arrange
        long original = 123456789;
        KeyType keyType = original;

        // Act
        string hash = keyType.ToHashKey();
        KeyType decoded = hash.ToKeyType();

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.Equal(keyType, decoded);
    }

    [Fact]
    public void ToHashKey_And_ToKeyType_RoundTrip_Works_For_Zero()
    {
        // Arrange
        long original = 0;
        KeyType keyType = original;

        // Act
        string hash = keyType.ToHashKey();
        KeyType decoded = hash.ToKeyType();

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.Equal(keyType, decoded);
    }

    [Fact]
    public void ToHashKey_With_NegativeValue_ThrowsException()
    {
        // Arrange
        long original = -987654321;
        KeyType keyType = original;

        // Assert
        Assert.Throws<ArgumentException>(() => keyType.ToHashKey());
    }

    [Fact]
    public void ToKeyType_EmptyString_ThrowsException()
    {
        // Arrange
        string invalidHash = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => invalidHash.ToKeyType());
    }

    [Fact]
    public void ToKeyType_InvalidHash_ThrowsException()
    {
        // Arrange
        string invalidHash = "invalidhash";

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => invalidHash.ToKeyType());
    }

    [Fact]
    public void ToHashKey_And_ToKeyType_With_IHashKeys_RoundTrip_Works()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        long original = 42;
        KeyType keyType = original;

        // Act
        string hash = keyType.ToHashKey(hashKeys);
        KeyType decoded = hash.ToKeyType(hashKeys);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.Equal(keyType, decoded);
    }

    [Fact]
    public void ToHashKey_With_IHashKeys_NegativeValue_ThrowsException()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        long original = -1;
        KeyType keyType = original;

        // Assert
        Assert.Throws<ArgumentException>(() => keyType.ToHashKey(hashKeys));
    }

    [Fact]
    public void ToKeyType_With_IHashKeys_EmptyString_ThrowsException()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        string invalidHash = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => invalidHash.ToKeyType(hashKeys));
    }

    [Fact]
    public void ToKeyType_With_IHashKeys_InvalidHash_ThrowsException()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        string invalidHash = "invalidhash";

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => invalidHash.ToKeyType(hashKeys));
    }

    [Fact]
    public void ToHashKey_And_ToKeyType_With_IServiceProvider_RoundTrip_Works()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();
        var provider = services.BuildServiceProvider();
        long original = 123;
        KeyType keyType = original;

        // Act
        string hash = keyType.ToHashKey(provider);
        KeyType decoded = hash.ToKeyType(provider);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.Equal(keyType, decoded);
    }

    [Fact]
    public void ToHashKey_With_IServiceProvider_NegativeValue_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();
        var provider = services.BuildServiceProvider();
        long original = -2;
        KeyType keyType = original;

        // Assert
        Assert.Throws<ArgumentException>(() => keyType.ToHashKey(provider));
    }

    [Fact]
    public void ToKeyType_With_IServiceProvider_EmptyString_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();
        var provider = services.BuildServiceProvider();
        string invalidHash = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => invalidHash.ToKeyType(provider));
    }

    [Fact]
    public void ToKeyType_With_IServiceProvider_InvalidHash_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();
        var provider = services.BuildServiceProvider();
        string invalidHash = "invalidhash";

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => invalidHash.ToKeyType(provider));
    }
}
