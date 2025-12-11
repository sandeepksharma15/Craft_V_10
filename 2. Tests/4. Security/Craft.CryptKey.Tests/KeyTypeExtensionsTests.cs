using Craft.CryptKey;
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

    [Fact]
    public void ToKeyType_NullString_ThrowsException()
    {
        // Arrange
        string? nullHash = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => nullHash!.ToKeyType());
    }

    [Fact]
    public void ToKeyType_With_IHashKeys_NullString_ThrowsException()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        string? nullHash = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => nullHash!.ToKeyType(hashKeys));
    }

    [Fact]
    public void ToKeyType_With_IServiceProvider_NullString_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();
        var provider = services.BuildServiceProvider();
        string? nullHash = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => nullHash!.ToKeyType(provider));
    }

    [Theory]
    [InlineData(long.MaxValue)]
    [InlineData(1000000)]
    [InlineData(999)]
    public void ToHashKey_And_ToKeyType_RoundTrip_Works_For_LargeValues(long value)
    {
        // Arrange
        KeyType keyType = value;

        // Act
        string hash = keyType.ToHashKey();
        KeyType decoded = hash.ToKeyType();

        // Assert
        Assert.Equal(keyType, decoded);
    }

    [Fact]
    public void ToHashKey_With_IHashKeys_SameValue_ProducesConsistentHash()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        KeyType keyType = 12345;

        // Act
        string hash1 = keyType.ToHashKey(hashKeys);
        string hash2 = keyType.ToHashKey(hashKeys);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ToHashKey_Without_IHashKeys_SameValue_ProducesConsistentHash()
    {
        // Arrange
        KeyType keyType = 54321;

        // Act
        string hash1 = keyType.ToHashKey();
        string hash2 = keyType.ToHashKey();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ToHashKey_With_IServiceProvider_CustomOptions_Works()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys(options =>
        {
            options.Salt = "CustomTestSalt";
            options.MinHashLength = 15;
        });
        var provider = services.BuildServiceProvider();
        KeyType keyType = 777;

        // Act
        string hash = keyType.ToHashKey(provider);
        KeyType decoded = hash.ToKeyType(provider);

        // Assert
        Assert.Equal(keyType, decoded);
        Assert.True(hash.Length >= 15);
    }

    [Fact]
    public void ToKeyType_With_WhiteSpaceString_ThrowsException()
    {
        // Arrange
        string whitespaceHash = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => whitespaceHash.ToKeyType());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(99)]
    [InlineData(999)]
    public void ToHashKey_MinimumHashLength_IsRespected(long value)
    {
        // Arrange
        var options = new HashKeyOptions { MinHashLength = 10 };
        var hashKeys = new HashKeys(options);
        KeyType keyType = value;

        // Act
        string hash = keyType.ToHashKey(hashKeys);

        // Assert
        Assert.True(hash.Length >= 10);
    }

    [Fact]
    public void ToHashKey_DifferentValues_ProduceDifferentHashes()
    {
        // Arrange
        KeyType keyType1 = 111;
        KeyType keyType2 = 222;

        // Act
        string hash1 = keyType1.ToHashKey();
        string hash2 = keyType2.ToHashKey();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
}
