using Craft.Domain.HashIdentityKey;

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
}
