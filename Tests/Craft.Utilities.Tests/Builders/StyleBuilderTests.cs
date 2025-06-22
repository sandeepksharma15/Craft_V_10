using Craft.Utilities.Builders;

namespace Craft.Utilities.Tests.Builders;

public class StyleBuilderTests
{
    [Fact]
    public void ShouldBulidConditionalInlineStyles()
    {
        // Arrange
        var hasBorder = true;
        var isOnTop = false;
        var top = 2;
        var bottom = 10;
        var left = 4;
        var right = 20;

        // Act
        var ClassToRender = new StyleBuilder("background-color", "DodgerBlue")
                        .AddStyle("border-width", $"{top}px {right}px {bottom}px {left}px", when: hasBorder)
                        .AddStyle("z-index", "999", when: isOnTop)
                        .AddStyle("z-index", "-1", when: !isOnTop)
                        .AddStyle("padding", "35px")
                        .Build();
        // Assert
        Assert.Equal("background-color:DodgerBlue;border-width:2px 20px 10px 4px;z-index:-1;padding:35px;", ClassToRender);
    }

    [Fact]
    public void ShouldBulidConditionalInlineStylesFromAttributes()
    {

        // Arrange
        var hasBorder = true;
        var isOnTop = false;
        var top = 2;
        var bottom = 10;
        var left = 4;
        var right = 20;

        // Act
        var StyleToRender = new StyleBuilder("background-color", "DodgerBlue")
                        .AddStyle("border-width", $"{top}px {right}px {bottom}px {left}px", when: hasBorder)
                        .AddStyle("z-index", "999", when: isOnTop)
                        .AddStyle("z-index", "-1", when: !isOnTop)
                        .AddStyle("padding", "35px")
                        .Build();

        IReadOnlyDictionary<string, object> attributes = new Dictionary<string, object> { { "style", StyleToRender } };

        var ClassToRender = new StyleBuilder().AddStyleFromAttributes(attributes).Build();

        // Assert
        Assert.Equal("background-color:DodgerBlue;border-width:2px 20px 10px 4px;z-index:-1;padding:35px;", ClassToRender);
    }

    [Fact]
    public void ShouldAddExistingStyle()
    {
        var StyleToRender = StyleBuilder.Empty()
            .AddStyle("background-color:DodgerBlue;")
            .AddStyle("padding", "35px")
            .Build();

        var StyleToRenderFromDefaultConstructor = StyleBuilder.Default(StyleToRender).Build();

        /// Double ;; is valid HTML.
        /// The CSS syntax allows for empty declarations, which means that you can add leading and trailing semicolons as you like. For instance, this is valid CSS
        /// .foo { ;;;display:none;;;color:black;;; }
        /// Trimming is possible, but is it worth the operations for a non-issue?
        Assert.Equal("background-color:DodgerBlue;;padding:35px;", StyleToRender);
        Assert.Equal("background-color:DodgerBlue;;padding:35px;;", StyleToRenderFromDefaultConstructor);

    }

    [Fact]
    public void ShouldNotAddEmptyStyle()
    {
        // Arrange & Act
        var StyleToRender = StyleBuilder.Empty().AddStyle("");

        Assert.Null(StyleToRender.NullIfEmpty());
    }

    [Fact]
    public void ShouldAddNestedStyles()
    {


        var Child = StyleBuilder.Empty()
            .AddStyle("background-color", "DodgerBlue")
            .AddStyle("padding", "35px");

        var StyleToRender = StyleBuilder.Empty()
            .AddStyle(Child)
            .AddStyle("z-index", "-1")
            .Build();

        /// Double ;; is valid HTML.
        /// The CSS syntax allows for empty declarations, which means that you can add leading and trailing semicolons as you like. For instance, this is valid CSS
        /// .foo { ;;;display:none;;;color:black;;; }
        /// Trimming is possible, but is it worth the operations for a non-issue?
        Assert.Equal("background-color:DodgerBlue;padding:35px;z-index:-1;", StyleToRender);
    }

    [Fact]
    public void ShouldAddComplexStyles()
    {
        var StyleToRender = StyleBuilder.Empty()
            .AddStyle("text-decoration", v => v
                        .AddValue("underline", true)
                        .AddValue("overline", false)
                        .AddValue("line-through", true),
                        when: true)
            .AddStyle("z-index", "-1")
            .Build();

        /// Double ;; is valid HTML.
        /// The CSS syntax allows for empty declarations, which means that you can add leading and trailing semicolons as you like. For instance, this is valid CSS
        /// .foo { ;;;display:none;;;color:black;;; }
        /// Trimming is possible, but is it worth the operations for a non-issue?
        Assert.Equal("text-decoration:underline line-through;z-index:-1;", StyleToRender);

    }

    [Fact]
    public void ShouldBuildStyleWithFunc()
    {
        {
            // Arrange
            // Simulates Razor Components attribute splatting feature
            IReadOnlyDictionary<string, object> attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

            // Act
            var StyleToRender = StyleBuilder.Empty()
                            .AddStyle("background-color", () => attributes["style"].ToString()!, when: attributes.ContainsKey("style"))
                            .AddStyle("background-color", "black")
                            .Build();
            // Assert
            Assert.Equal("background-color:black;", StyleToRender);
        }
    }

    [Fact]
    public void AddStyle_WithValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var builder = StyleBuilder.Default("color", "red");

        // Act
        builder.AddStyle("background-color", "blue");
        var result = builder.Build();

        // Assert
        Assert.Equal("color:red;background-color:blue;", result);
    }

    [Fact]
    public void AddStyle_Conditional_WhenConditionTrue_AddsStyle()
    {
        // Arrange
        var builder = StyleBuilder.Empty();

        // Act
        builder.AddStyle("margin", "10px", true);
        var result = builder.Build();

        // Assert
        Assert.Equal("margin:10px;", result);
    }

    [Fact]
    public void AddStyle_Conditional_WhenConditionFalse_DoesNotAddStyle()
    {
        // Arrange
        var builder = StyleBuilder.Empty();

        // Act
        builder.AddStyle("padding", "5px", false);
        var result = builder.Build();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Build_WithNoStyles_ReturnsEmptyString()
    {
        // Arrange
        var builder = StyleBuilder.Empty();

        // Act
        var result = builder.Build();

        // Assert
        Assert.Equal(string.Empty, result);
    }
}
