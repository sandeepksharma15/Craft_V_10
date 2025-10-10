using Craft.Utilities.Builders;

namespace Craft.Utilities.Tests.Builders;

public class CssBuilderTests
{
    [Fact]
    public void ShouldConstructWithDefaultValue()
    {
        // Arrange
        var classToRender = CssBuilder.Default("item-one").Build();

        // Assert
        Assert.Equal("item-one", classToRender);
    }

    [Fact]
    public void Build_ShouldReturnTrimmedString()
    {
        // Arrange
        var cssBuilder = new CssBuilder("  test  ");

        // Act
        var result = cssBuilder.Build();

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void ToString_ShouldReturnSameResultAsBuild()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var buildResult = cssBuilder.Build();
        var toStringResult = cssBuilder.ToString();

        // Assert
        Assert.Equal(buildResult, toStringResult);
    }

    [Fact]
    public void AddValue_ShouldAppendToBuffer()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddValue(" appended").Build();

        // Assert
        Assert.Equal("value appended", result);
    }

    [Fact]
    public void AddClass_ShouldAddSpaceAndClass()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClass("newClass").Build();

        // Assert
        Assert.Equal("value newClass", result);
    }

    [Fact]
    public void AddClass_WithCondition_ShouldAddClassIfConditionIsTrue()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClass("newClass", true).Build();

        // Assert
        Assert.Equal("value newClass", result);
    }

    [Fact]
    public void AddClass_WithCondition_ShouldNotAddClassIfConditionIsFalse()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClass("newClass", false).Build();

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void AddClass_WithFuncCondition_ShouldAddClassIfFuncConditionIsTrue()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClass("newClass", () => true).Build();

        // Assert
        Assert.Equal("value newClass", result);
    }

    [Fact]
    public void AddClass_WithFuncCondition_ShouldNotAddClassIfFuncConditionIsFalse()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClass("newClass", () => false).Build();

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void AddClass_WithFuncCondition_ShouldNotAddClassIfFuncConditionIsNull()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClass("newClass", null!).Build();

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void AddClassFromAttributes_ShouldAddClassFromAttributes()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");
        var attributes = new Dictionary<string, object> { { "class", "additionalClass" } };

        // Act
        var result = cssBuilder.AddClassFromAttributes(attributes).Build();

        // Assert
        Assert.Equal("value additionalClass", result);
    }

    [Fact]
    public void AddClassFromAttributes_WithNullAttributes_ShouldNotAddClass()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClassFromAttributes(null!).Build();

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void ShouldBuildConditionalCssClasses()
    {
        static bool hasFive() => false;

        // Arrange
        const bool hasTwo = false;
        const bool hasThree = true;

        // Act
        var classToRender = new CssBuilder("item-one")
                        .AddClass("item-two", when: hasTwo)
                        .AddClass("item-three", when: hasThree)
                        .AddClass("item-four")
                        .AddClass("item-five", when: hasFive)
                        .Build();
        // Assert
        Assert.Equal("item-one item-three item-four", classToRender);
    }

    [Fact]
    public void ShouldBuildConditionalCssBuilderClasses()
    {
        // Arrange
        const bool hasTwo = false;
        const bool hasThree = true;
        static bool hasFive() => false;

        // Act
        var classToRender = new CssBuilder("item-one")
                        .AddClass("item-two", when: hasTwo)
                        .AddClass(new CssBuilder("item-three")
                                        .AddClass("item-foo", false)
                                        .AddClass("item-sub-three"),
                                        when: hasThree)
                        .AddClass("item-four")
                        .AddClass("item-five", when: hasFive)
                        .Build();
        // Assert
        Assert.Equal("item-one item-three item-sub-three item-four", classToRender);
    }

    [Fact]
    public void ShouldBuildEmptyClasses()
    {
        // Arrange
        const bool shouldShow = false;

        // Act
        var classToRender = new CssBuilder()
                        .AddClass("some-class", shouldShow)
                        .Build();
        // Assert
        Assert.Equal(string.Empty, classToRender);
    }

    [Fact]
    public void ShouldBuildClassesWithFunc()
    {
        // Arrange
        IReadOnlyDictionary<string, object> attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

        // Act
        var classToRender = new CssBuilder("item-one")
                        .AddClass(() => attributes["class"].ToString()!, when: attributes.ContainsKey("class"))
                        .Build();
        // Assert
        Assert.Equal("item-one my-custom-class-1", classToRender);
    }

    [Fact]
    public void ShouldNotThrowWhenNullFor_BuildClassesFromAttributes()
    {
        // Arrange
        IReadOnlyDictionary<string, object>? attributes = null;

        // Act
        var classToRender = new CssBuilder("item-one")
                        .AddClassFromAttributes(attributes!)
                        .Build();

        // Assert
        Assert.Equal("item-one", classToRender);
    }

    [Fact]
    public void AddClass_WithFuncValueAndFuncCondition_ShouldAddClassIfConditionIsTrue()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClass(() => "newClass", () => true).Build();

        // Assert
        Assert.Equal("value newClass", result);
    }

    [Fact]
    public void AddClass_WithFuncValueAndFuncCondition_ShouldNotAddClassIfConditionIsFalse()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClass(() => "newClass", () => false).Build();

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void AddClass_WithFuncValueAndFuncCondition_ShouldNotAddClassIfConditionIsNull()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");

        // Act
        var result = cssBuilder.AddClass(() => "newClass", null!).Build();

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void AddClass_WithCssBuilderAndFuncCondition_ShouldAddClassIfConditionIsTrue()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");
        var otherBuilder = new CssBuilder("otherClass");

        // Act
        var result = cssBuilder.AddClass(otherBuilder, () => true).Build();

        // Assert
        Assert.Equal("value otherClass", result);
    }

    [Fact]
    public void AddClass_WithCssBuilderAndFuncCondition_ShouldNotAddClassIfConditionIsFalse()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");
        var otherBuilder = new CssBuilder("otherClass");

        // Act
        var result = cssBuilder.AddClass(otherBuilder, () => false).Build();

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void AddClass_WithCssBuilderAndFuncCondition_ShouldNotAddClassIfConditionIsNull()
    {
        // Arrange
        var cssBuilder = new CssBuilder("value");
        var otherBuilder = new CssBuilder("otherClass");

        // Act
        var result = cssBuilder.AddClass(otherBuilder, null!).Build();

        // Assert
        Assert.Equal("value", result);
    }
}
