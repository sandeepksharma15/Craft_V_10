using System.ComponentModel;

namespace Craft.Domain.Tests.Enums;

/// <summary>
/// Unit tests for the GenderType enum.
/// </summary>
public class GenderTypeTests
{
    [Fact]
    public void GenderType_ShouldHaveThreeValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<GenderType>();

        // Assert
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void GenderType_Male_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var value = GenderType.Male;

        // Assert
        Assert.Equal(0, (int)value);
    }

    [Fact]
    public void GenderType_Female_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var value = GenderType.Female;

        // Assert
        Assert.Equal(1, (int)value);
    }

    [Fact]
    public void GenderType_Other_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var value = GenderType.Other;

        // Assert
        Assert.Equal(2, (int)value);
    }

    [Fact]
    public void GenderType_Male_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var fieldInfo = typeof(GenderType).GetField(nameof(GenderType.Male));

        // Act
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        var description = (attributes?.FirstOrDefault() as DescriptionAttribute)?.Description;

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Equal("Male", description);
    }

    [Fact]
    public void GenderType_Female_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var fieldInfo = typeof(GenderType).GetField(nameof(GenderType.Female));

        // Act
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        var description = (attributes?.FirstOrDefault() as DescriptionAttribute)?.Description;

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Equal("Female", description);
    }

    [Fact]
    public void GenderType_Other_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var fieldInfo = typeof(GenderType).GetField(nameof(GenderType.Other));

        // Act
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        var description = (attributes?.FirstOrDefault() as DescriptionAttribute)?.Description;

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Equal("Other", description);
    }

    [Theory]
    [InlineData(GenderType.Male, "Male")]
    [InlineData(GenderType.Female, "Female")]
    [InlineData(GenderType.Other, "Other")]
    public void GenderType_ToString_ShouldReturnEnumName(GenderType gender, string expectedName)
    {
        // Act
        var result = gender.ToString();

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void GenderType_CanParseFromString_Male()
    {
        // Arrange
        const string input = "Male";

        // Act
        var result = Enum.Parse<GenderType>(input);

        // Assert
        Assert.Equal(GenderType.Male, result);
    }

    [Fact]
    public void GenderType_CanParseFromString_Female()
    {
        // Arrange
        const string input = "Female";

        // Act
        var result = Enum.Parse<GenderType>(input);

        // Assert
        Assert.Equal(GenderType.Female, result);
    }

    [Fact]
    public void GenderType_CanParseFromString_Other()
    {
        // Arrange
        const string input = "Other";

        // Act
        var result = Enum.Parse<GenderType>(input);

        // Assert
        Assert.Equal(GenderType.Other, result);
    }

    [Fact]
    public void GenderType_AllValues_ShouldBeUnique()
    {
        // Arrange
        var values = Enum.GetValues<GenderType>();

        // Act
        var distinctValues = values.Distinct();

        // Assert
        Assert.Equal(values.Length, distinctValues.Count());
    }

    [Fact]
    public void GenderType_IsDefined_Male_ShouldReturnTrue()
    {
        // Act
        var result = Enum.IsDefined(GenderType.Male);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GenderType_IsDefined_InvalidValue_ShouldReturnFalse()
    {
        // Act
        var result = Enum.IsDefined(typeof(GenderType), 999);

        // Assert
        Assert.False(result);
    }
}
