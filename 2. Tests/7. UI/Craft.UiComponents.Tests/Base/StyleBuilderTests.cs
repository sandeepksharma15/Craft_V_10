using Craft.UiConponents;

namespace Craft.UiComponents.Tests.Base;

public class StyleBuilderTests
{
    [Fact]
    public void Add_WithPropertyAndValue_ShouldAddStyle()
    {
        // Arrange
        var builder = new StyleBuilder();

        // Act
        builder.Add("color", "red");
        var result = builder.Build();

        // Assert
        Assert.Equal("color: red", result);
    }

    [Fact]
    public void Add_WithMultipleProperties_ShouldJoinWithSemicolon()
    {
        // Arrange
        var builder = new StyleBuilder();

        // Act
        builder.Add("color", "red").Add("font-size", "14px");
        var result = builder.Build();

        // Assert
        Assert.Equal("color: red; font-size: 14px", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_WithNullOrEmptyValue_ShouldNotAddStyle(string? value)
    {
        // Arrange
        var builder = new StyleBuilder();

        // Act
        builder.Add("color", value);
        var result = builder.Build();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Add_WithRawStyleString_ShouldAddStyle()
    {
        // Arrange
        var builder = new StyleBuilder();

        // Act
        builder.Add("color: blue; font-weight: bold");
        var result = builder.Build();

        // Assert
        Assert.Equal("color: blue; font-weight: bold", result);
    }

    [Fact]
    public void Add_WithConditionTrue_ShouldAddStyle()
    {
        // Arrange
        var builder = new StyleBuilder();

        // Act
        builder.Add("display", "none", true);
        var result = builder.Build();

        // Assert
        Assert.Equal("display: none", result);
    }

    [Fact]
    public void Add_WithConditionFalse_ShouldNotAddStyle()
    {
        // Arrange
        var builder = new StyleBuilder();

        // Act
        builder.Add("display", "none", false);
        var result = builder.Build();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Add_WithTrailingSemicolon_ShouldHandleCorrectly()
    {
        // Arrange
        var builder = new StyleBuilder();

        // Act
        builder.Add("color: red;").Add("font-size: 14px;");
        var result = builder.Build();

        // Assert
        Assert.Equal("color: red; font-size: 14px", result);
    }

    [Fact]
    public void Build_WithNoStyles_ShouldReturnNull()
    {
        // Arrange
        var builder = new StyleBuilder();

        // Act
        var result = builder.Build();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToString_WithNoStyles_ShouldReturnEmptyString()
    {
        // Arrange
        var builder = new StyleBuilder();

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToString_WithStyles_ShouldReturnStyleString()
    {
        // Arrange
        var builder = new StyleBuilder();
        builder.Add("color", "red");

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Equal("color: red", result);
    }
}
