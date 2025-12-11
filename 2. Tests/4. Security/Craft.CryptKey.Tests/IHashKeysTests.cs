using Microsoft.Extensions.DependencyInjection;

namespace Craft.CryptKey.Tests;

public class IHashKeysTests
{
    [Fact]
    public void IHashKeys_Interface_IsImplementedByHashKeys()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act
        IHashKeys hashKeys = new HashKeys(options);

        // Assert
        Assert.NotNull(hashKeys);
        Assert.IsType<IHashKeys>(hashKeys, exactMatch: false);
    }

    [Fact]
    public void IHashKeys_CanEncodeLong()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        long value = 123456789;

        // Act
        var hash = hashKeys.EncodeLong(value);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void IHashKeys_CanDecodeLong()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        long original = 987654321;
        var hash = hashKeys.EncodeLong(original);

        // Act
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.NotNull(decoded);
        Assert.Single(decoded);
        Assert.Equal(original, decoded[0]);
    }

    [Fact]
    public void IHashKeys_CanEncodeMultipleLongs()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        long[] values = [1L, 2L, 3L, 4L, 5L];

        // Act
        var hash = hashKeys.EncodeLong(values);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void IHashKeys_CanDecodeMultipleLongs()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        long[] original = [10L, 20L, 30L, 40L, 50L];
        var hash = hashKeys.EncodeLong(original);

        // Act
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.Equal(original.Length, decoded.Length);
        Assert.Equal(original, decoded);
    }

    [Fact]
    public void IHashKeys_CanEncodeInt()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        int value = 12345;

        // Act
        var hash = hashKeys.Encode(value);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void IHashKeys_CanDecodeInt()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        int original = 98765;
        var hash = hashKeys.Encode(original);

        // Act
        var decoded = hashKeys.Decode(hash);

        // Assert
        Assert.NotNull(decoded);
        Assert.Single(decoded);
        Assert.Equal(original, decoded[0]);
    }

    [Fact]
    public void IHashKeys_CanEncodeHex()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        string hex = "1234567890ABCDEF";

        // Act
        var hash = hashKeys.EncodeHex(hex);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void IHashKeys_CanDecodeHex()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        string original = "DEADBEEF";
        var hash = hashKeys.EncodeHex(original);

        // Act
        var decoded = hashKeys.DecodeHex(hash);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(original.ToUpperInvariant(), decoded.ToUpperInvariant());
    }

    [Fact]
    public void IHashKeys_MultipleInstances_CanShareInterface()
    {
        // Arrange
        var options1 = new HashKeyOptions { Salt = "Salt1" };
        var options2 = new HashKeyOptions { Salt = "Salt2" };
        IHashKeys hashKeys1 = new HashKeys(options1);
        IHashKeys hashKeys2 = new HashKeys(options2);
        long value = 12345;

        // Act
        var hash1 = hashKeys1.EncodeLong(value);
        var hash2 = hashKeys2.EncodeLong(value);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void IHashKeys_FromDI_WorksCorrectly()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddHashKeys();
        var provider = services.BuildServiceProvider();
        IHashKeys hashKeys = provider.GetRequiredService<IHashKeys>();
        long value = 54321;

        // Act
        var hash = hashKeys.EncodeLong(value);
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.Equal(value, decoded[0]);
    }

    [Fact]
    public void IHashKeys_EmptyArray_EncodesAndDecodesCorrectly()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        long[] emptyArray = [];

        // Act
        var hash = hashKeys.EncodeLong(emptyArray);
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.NotNull(hash);
        Assert.Empty(hash);
        Assert.NotNull(decoded);
        Assert.Empty(decoded);
    }

    [Fact]
    public void IHashKeys_LargeValues_EncodeAndDecodeCorrectly()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        long largeValue = long.MaxValue;

        // Act
        var hash = hashKeys.EncodeLong(largeValue);
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.Equal(largeValue, decoded[0]);
    }

    [Fact]
    public void IHashKeys_ZeroValue_EncodeAndDecodeCorrectly()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        long zeroValue = 0L;

        // Act
        var hash = hashKeys.EncodeLong(zeroValue);
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.Equal(zeroValue, decoded[0]);
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    [InlineData(10000L)]
    [InlineData(100000L)]
    [InlineData(1000000L)]
    public void IHashKeys_VariousValues_RoundTripSuccessfully(long value)
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);

        // Act
        var hash = hashKeys.EncodeLong(value);
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.Equal(value, decoded[0]);
    }
}
