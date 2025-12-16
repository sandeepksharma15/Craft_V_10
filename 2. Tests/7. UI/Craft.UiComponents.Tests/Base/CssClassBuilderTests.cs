using Craft.UiConponents;

namespace Craft.UiComponents.Tests.Base;

public class CssClassBuilderTests
{
    [Fact]
    public void Add_WithValidClass_ShouldAddClass()
    {
        // Arrange
        var builder = new CssClassBuilder();

        // Act
        builder.Add("test-class");
        var result = builder.Build();

        // Assert
        Assert.Equal("test-class", result);
    }

    [Fact]
    public void Add_WithMultipleClasses_ShouldJoinWithSpace()
    {
        // Arrange
        var builder = new CssClassBuilder();

        // Act
        builder.Add("class1").Add("class2").Add("class3");
        var result = builder.Build();

        // Assert
        Assert.Equal("class1 class2 class3", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_WithNullOrEmptyClass_ShouldNotAddClass(string? cssClass)
    {
        // Arrange
        var builder = new CssClassBuilder();

        // Act
        builder.Add("existing").Add(cssClass);
        var result = builder.Build();

        // Assert
        Assert.Equal("existing", result);
    }

    [Fact]
    public void Add_WithConditionTrue_ShouldAddClass()
    {
        // Arrange
        var builder = new CssClassBuilder();

        // Act
        builder.Add("conditional-class", true);
        var result = builder.Build();

        // Assert
        Assert.Equal("conditional-class", result);
    }

    [Fact]
    public void Add_WithConditionFalse_ShouldNotAddClass()
    {
        // Arrange
        var builder = new CssClassBuilder();

        // Act
        builder.Add("conditional-class", false);
        var result = builder.Build();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Add_WithWhitespace_ShouldTrimClass()
    {
        // Arrange
        var builder = new CssClassBuilder();

        // Act
        builder.Add("  trimmed-class  ");
        var result = builder.Build();

        // Assert
        Assert.Equal("trimmed-class", result);
    }

    [Fact]
    public void Build_WithNoClasses_ShouldReturnEmptyString()
    {
        // Arrange
        var builder = new CssClassBuilder();

        // Act
        var result = builder.Build();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToString_ShouldReturnSameAsBuild()
    {
        // Arrange
        var builder = new CssClassBuilder();
        builder.Add("class1").Add("class2");

        // Act
        var buildResult = builder.Build();
        var toStringResult = builder.ToString();

        // Assert
        Assert.Equal(buildResult, toStringResult);
    }
}
