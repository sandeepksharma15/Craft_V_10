using Craft.HashIdKeys;

namespace Craft.Domain.Tests.Keys;

public class HashKeyOptionsTests
{
    [Fact]
    public void Alphabet_DefaultValue_ShouldBe_DefaultAlphabet()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act & Assert
        Assert.Equal(HashidsNet.Hashids.DEFAULT_ALPHABET, options.Alphabet);
    }

    [Fact]
    public void MinHashLength_DefaultValue_ShouldBe_10()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act & Assert
        Assert.Equal(10, options.MinHashLength);
    }

    [Fact]
    public void Salt_DefaultValue_ShouldBe_CraftDomainKeySalt()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act & Assert
        Assert.Equal("CraftDomainKeySalt", options.Salt);
    }

    [Fact]
    public void Steps_DefaultValue_ShouldBe_DefaultSeps()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act & Assert
        Assert.Equal(HashidsNet.Hashids.DEFAULT_SEPS, options.Steps);
    }

    // Write a test to test all getter and setter methods
    [Fact]
    public void AllProperties_ShouldBeSet()
    {
        // Arrange
        var options = new HashKeyOptions
        {
            Alphabet = "abcdefghijklmnopqrstuvwxyz",
            MinHashLength = 5,
            Salt = "CraftDomainKeySalt",
            Steps = "1234567890"
        };

        // Act & Assert
        Assert.Equal("abcdefghijklmnopqrstuvwxyz", options.Alphabet);
        Assert.Equal(5, options.MinHashLength);
        Assert.Equal("CraftDomainKeySalt", options.Salt);
        Assert.Equal("1234567890", options.Steps);
    }
}
