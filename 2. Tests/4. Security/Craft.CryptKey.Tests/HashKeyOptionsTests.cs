using Craft.CryptKey;

namespace Craft.CryptKey.Tests;

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

    [Fact]
    public void AllProperties_ShouldBeSet()
    {
        // Arrange
        var options = new HashKeyOptions
        {
            Alphabet = "abcdefghijklmnopqrstuvwxyz",
            MinHashLength = 5,
            Salt = "CustomSalt",
            Steps = "1234567890"
        };

        // Act & Assert
        Assert.Equal("abcdefghijklmnopqrstuvwxyz", options.Alphabet);
        Assert.Equal(5, options.MinHashLength);
        Assert.Equal("CustomSalt", options.Salt);
        Assert.Equal("1234567890", options.Steps);
    }

    [Fact]
    public void Alphabet_CanBeSetToCustomValue()
    {
        // Arrange
        var options = new HashKeyOptions();
        var customAlphabet = "0123456789ABCDEF";

        // Act
        options.Alphabet = customAlphabet;

        // Assert
        Assert.Equal(customAlphabet, options.Alphabet);
    }

    [Fact]
    public void MinHashLength_CanBeSetToCustomValue()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act
        options.MinHashLength = 25;

        // Assert
        Assert.Equal(25, options.MinHashLength);
    }

    [Fact]
    public void Salt_CanBeSetToCustomValue()
    {
        // Arrange
        var options = new HashKeyOptions();
        var customSalt = "MyCustomSalt123";

        // Act
        options.Salt = customSalt;

        // Assert
        Assert.Equal(customSalt, options.Salt);
    }

    [Fact]
    public void Steps_CanBeSetToCustomValue()
    {
        // Arrange
        var options = new HashKeyOptions();
        var customSteps = "cfhistuCFHISTU";

        // Act
        options.Steps = customSteps;

        // Assert
        Assert.Equal(customSteps, options.Steps);
    }

    [Fact]
    public void MinHashLength_CanBeSetToZero()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act
        options.MinHashLength = 0;

        // Assert
        Assert.Equal(0, options.MinHashLength);
    }

    [Fact]
    public void Properties_AreIndependent()
    {
        // Arrange
        var options1 = new HashKeyOptions { Salt = "Salt1" };
        var options2 = new HashKeyOptions { Salt = "Salt2" };

        // Act & Assert
        Assert.NotEqual(options1.Salt, options2.Salt);
        Assert.Equal(options1.MinHashLength, options2.MinHashLength);
    }

    [Fact]
    public void EmptySalt_CanBeSet()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act
        options.Salt = string.Empty;

        // Assert
        Assert.Equal(string.Empty, options.Salt);
    }

    [Fact]
    public void AllProperties_CanBeModifiedAfterConstruction()
    {
        // Arrange
        var options = new HashKeyOptions();

        // Act
        options.Alphabet = "abc";
        options.MinHashLength = 100;
        options.Salt = "NewSalt";
        options.Steps = "xyz";

        // Assert
        Assert.Equal("abc", options.Alphabet);
        Assert.Equal(100, options.MinHashLength);
        Assert.Equal("NewSalt", options.Salt);
        Assert.Equal("xyz", options.Steps);
    }
}
