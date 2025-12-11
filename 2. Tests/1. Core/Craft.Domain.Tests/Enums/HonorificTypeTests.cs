using System.ComponentModel;

namespace Craft.Domain.Tests.Enums;

/// <summary>
/// Unit tests for the HonorificType enum.
/// </summary>
public class HonorificTypeTests
{
    [Fact]
    public void HonorificType_ShouldHaveFiveValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<HonorificType>();

        // Assert
        Assert.Equal(5, values.Length);
    }

    [Fact]
    public void HonorificType_Dr_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var value = HonorificType.Dr;

        // Assert
        Assert.Equal(0, (int)value);
    }

    [Fact]
    public void HonorificType_Prof_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var value = HonorificType.Prof;

        // Assert
        Assert.Equal(1, (int)value);
    }

    [Fact]
    public void HonorificType_Mr_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var value = HonorificType.Mr;

        // Assert
        Assert.Equal(2, (int)value);
    }

    [Fact]
    public void HonorificType_Ms_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var value = HonorificType.Ms;

        // Assert
        Assert.Equal(3, (int)value);
    }

    [Fact]
    public void HonorificType_Mrs_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var value = HonorificType.Mrs;

        // Assert
        Assert.Equal(4, (int)value);
    }

    [Fact]
    public void HonorificType_Dr_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var fieldInfo = typeof(HonorificType).GetField(nameof(HonorificType.Dr));

        // Act
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        var description = (attributes?.FirstOrDefault() as DescriptionAttribute)?.Description;

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Equal("Dr", description);
    }

    [Fact]
    public void HonorificType_Prof_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var fieldInfo = typeof(HonorificType).GetField(nameof(HonorificType.Prof));

        // Act
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        var description = (attributes?.FirstOrDefault() as DescriptionAttribute)?.Description;

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Equal("Prof", description);
    }

    [Fact]
    public void HonorificType_Mr_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var fieldInfo = typeof(HonorificType).GetField(nameof(HonorificType.Mr));

        // Act
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        var description = (attributes?.FirstOrDefault() as DescriptionAttribute)?.Description;

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Equal("Mr", description);
    }

    [Fact]
    public void HonorificType_Ms_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var fieldInfo = typeof(HonorificType).GetField(nameof(HonorificType.Ms));

        // Act
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        var description = (attributes?.FirstOrDefault() as DescriptionAttribute)?.Description;

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Equal("Ms", description);
    }

    [Fact]
    public void HonorificType_Mrs_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var fieldInfo = typeof(HonorificType).GetField(nameof(HonorificType.Mrs));

        // Act
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        var description = (attributes?.FirstOrDefault() as DescriptionAttribute)?.Description;

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Equal("Mrs", description);
    }

    [Theory]
    [InlineData(HonorificType.Dr, "Dr")]
    [InlineData(HonorificType.Prof, "Prof")]
    [InlineData(HonorificType.Mr, "Mr")]
    [InlineData(HonorificType.Ms, "Ms")]
    [InlineData(HonorificType.Mrs, "Mrs")]
    public void HonorificType_ToString_ShouldReturnEnumName(HonorificType honorific, string expectedName)
    {
        // Act
        var result = honorific.ToString();

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void HonorificType_CanParseFromString_Dr()
    {
        // Arrange
        const string input = "Dr";

        // Act
        var result = Enum.Parse<HonorificType>(input);

        // Assert
        Assert.Equal(HonorificType.Dr, result);
    }

    [Fact]
    public void HonorificType_CanParseFromString_Prof()
    {
        // Arrange
        const string input = "Prof";

        // Act
        var result = Enum.Parse<HonorificType>(input);

        // Assert
        Assert.Equal(HonorificType.Prof, result);
    }

    [Fact]
    public void HonorificType_CanParseFromString_Mr()
    {
        // Arrange
        const string input = "Mr";

        // Act
        var result = Enum.Parse<HonorificType>(input);

        // Assert
        Assert.Equal(HonorificType.Mr, result);
    }

    [Fact]
    public void HonorificType_CanParseFromString_Ms()
    {
        // Arrange
        const string input = "Ms";

        // Act
        var result = Enum.Parse<HonorificType>(input);

        // Assert
        Assert.Equal(HonorificType.Ms, result);
    }

    [Fact]
    public void HonorificType_CanParseFromString_Mrs()
    {
        // Arrange
        const string input = "Mrs";

        // Act
        var result = Enum.Parse<HonorificType>(input);

        // Assert
        Assert.Equal(HonorificType.Mrs, result);
    }

    [Fact]
    public void HonorificType_AllValues_ShouldBeUnique()
    {
        // Arrange
        var values = Enum.GetValues<HonorificType>();

        // Act
        var distinctValues = values.Distinct();

        // Assert
        Assert.Equal(values.Length, distinctValues.Count());
    }

    [Fact]
    public void HonorificType_IsDefined_Dr_ShouldReturnTrue()
    {
        // Act
        var result = Enum.IsDefined(HonorificType.Dr);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HonorificType_IsDefined_InvalidValue_ShouldReturnFalse()
    {
        // Act
        var result = Enum.IsDefined(typeof(HonorificType), 999);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(HonorificType.Dr)]
    [InlineData(HonorificType.Prof)]
    [InlineData(HonorificType.Mr)]
    [InlineData(HonorificType.Ms)]
    [InlineData(HonorificType.Mrs)]
    public void HonorificType_AllValues_AreValid(HonorificType honorific)
    {
        // Act
        var result = Enum.IsDefined(honorific);

        // Assert
        Assert.True(result);
    }
}
