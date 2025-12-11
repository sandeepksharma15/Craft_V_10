namespace Craft.CryptKey.Tests;

public class HashKeysTests
{
    [Fact]
    public void Constructor_WithDefaultOptions_CreatesInstance()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act
        var hashKeys = new HashKeys(options);

        // Assert
        Assert.NotNull(hashKeys);
        Assert.IsAssignableFrom<IHashKeys>(hashKeys);
    }

    [Fact]
    public void Constructor_WithCustomOptions_CreatesInstance()
    {
        // Arrange
        var options = new HashKeyOptions
        {
            Salt = "CustomSalt",
            MinHashLength = 15,
            Alphabet = "abcdefghijklmnopqrstuvwxyz1234567890",
            Steps = "cfhistu"
        };

        // Act
        var hashKeys = new HashKeys(options);

        // Assert
        Assert.NotNull(hashKeys);
    }

    [Fact]
    public void EncodeLong_WithSingleValue_ReturnsHashString()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        long value = 12345;

        // Act
        var hash = hashKeys.EncodeLong(value);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.True(hash.Length >= options.MinHashLength);
    }

    [Fact]
    public void DecodeLong_WithValidHash_ReturnsOriginalValue()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        long original = 98765;
        var hash = hashKeys.EncodeLong(original);

        // Act
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.NotNull(decoded);
        Assert.Single(decoded);
        Assert.Equal(original, decoded[0]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)]
    [InlineData(long.MaxValue)]
    public void EncodeDecode_RoundTrip_PreservesValue(long original)
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);

        // Act
        var encoded = hashKeys.EncodeLong(original);
        var decoded = hashKeys.DecodeLong(encoded);

        // Assert
        Assert.Equal(original, decoded[0]);
    }

    [Fact]
    public void EncodeLong_WithMultipleValues_ReturnsHashString()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        long[] values = [1, 2, 3, 4, 5];

        // Act
        var hash = hashKeys.EncodeLong(values);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void DecodeLong_WithMultipleValues_ReturnsAllOriginalValues()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        long[] original = [10, 20, 30, 40, 50];
        var hash = hashKeys.EncodeLong(original);

        // Act
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.Equal(original.Length, decoded.Length);
        Assert.Equal(original, decoded);
    }

    [Fact]
    public void DifferentSalts_ProduceDifferentHashes()
    {
        // Arrange
        var options1 = new HashKeyOptions { Salt = "Salt1" };
        var options2 = new HashKeyOptions { Salt = "Salt2" };
        var hashKeys1 = new HashKeys(options1);
        var hashKeys2 = new HashKeys(options2);
        long value = 12345;

        // Act
        var hash1 = hashKeys1.EncodeLong(value);
        var hash2 = hashKeys2.EncodeLong(value);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void DifferentAlphabets_ProduceDifferentHashes()
    {
        // Arrange
        var options1 = new HashKeyOptions { Alphabet = "abcdefghijklmnopqrstuvwxyz" };
        var options2 = new HashKeyOptions { Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" };
        var hashKeys1 = new HashKeys(options1);
        var hashKeys2 = new HashKeys(options2);
        long value = 12345;

        // Act
        var hash1 = hashKeys1.EncodeLong(value);
        var hash2 = hashKeys2.EncodeLong(value);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void MinHashLength_IsRespected()
    {
        // Arrange
        var options = new HashKeyOptions { MinHashLength = 20 };
        var hashKeys = new HashKeys(options);
        long value = 1;

        // Act
        var hash = hashKeys.EncodeLong(value);

        // Assert
        Assert.True(hash.Length >= 20);
    }

    [Fact]
    public void SameOptionsInstance_ProducesConsistentHashes()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        long value = 54321;

        // Act
        var hash1 = hashKeys.EncodeLong(value);
        var hash2 = hashKeys.EncodeLong(value);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void IHashKeys_Interface_ImplementedCorrectly()
    {
        // Arrange
        var options = new HashKeyOptions();
        IHashKeys hashKeys = new HashKeys(options);
        long value = 789;

        // Act
        var hash = hashKeys.EncodeLong(value);
        var decoded = hashKeys.DecodeLong(hash);

        // Assert
        Assert.Equal(value, decoded[0]);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 100)]
    [InlineData(100, 1000)]
    [InlineData(1000, 10000)]
    public void DifferentValues_ProduceDifferentHashes(long value1, long value2)
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);

        // Act
        var hash1 = hashKeys.EncodeLong(value1);
        var hash2 = hashKeys.EncodeLong(value2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Encode_EmptyArray_ReturnsEmptyString()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);
        long[] emptyArray = [];

        // Act
        var hash = hashKeys.EncodeLong(emptyArray);

        // Assert
        Assert.NotNull(hash);
        Assert.Empty(hash);
    }

    [Fact]
    public void Decode_EmptyString_ReturnsEmptyArray()
    {
        // Arrange
        var options = new HashKeyOptions();
        var hashKeys = new HashKeys(options);

        // Act
        var decoded = hashKeys.DecodeLong(string.Empty);

        // Assert
        Assert.NotNull(decoded);
        Assert.Empty(decoded);
    }

    [Fact]
    public void CustomSteps_AffectsEncoding()
    {
        // Arrange
        var options1 = new HashKeyOptions { Steps = "cfhistu" };
        var options2 = new HashKeyOptions { Steps = "abcdefg" };
        var hashKeys1 = new HashKeys(options1);
        var hashKeys2 = new HashKeys(options2);
        long value = 12345;

        // Act
        var hash1 = hashKeys1.EncodeLong(value);
        var hash2 = hashKeys2.EncodeLong(value);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void MultipleInstancesWithSameOptions_ProduceIdenticalResults()
    {
        // Arrange
        var options1 = new HashKeyOptions
        {
            Salt = "TestSalt",
            MinHashLength = 12,
            Alphabet = "abcdefghijklmnopqrstuvwxyz1234567890"
        };
        var options2 = new HashKeyOptions
        {
            Salt = "TestSalt",
            MinHashLength = 12,
            Alphabet = "abcdefghijklmnopqrstuvwxyz1234567890"
        };
        var hashKeys1 = new HashKeys(options1);
        var hashKeys2 = new HashKeys(options2);
        long value = 99999;

        // Act
        var hash1 = hashKeys1.EncodeLong(value);
        var hash2 = hashKeys2.EncodeLong(value);

        // Assert
        Assert.Equal(hash1, hash2);
    }
}
