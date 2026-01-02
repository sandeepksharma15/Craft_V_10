using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Show component.
/// Tests conditional rendering when condition is true.
/// </summary>
public class ShowTests : ComponentTestBase
{
    [Fact]
    public void Show_WhenConditionIsTrue_ShouldRenderChildContent()
    {
        // Arrange & Act
        var cut = Render<Show>(parameters => parameters
            .Add(p => p.When, true)
            .AddChildContent("<div>Content to show</div>")
        );

        // Assert
        Assert.Contains("Content to show", cut.Markup);
    }

    [Fact]
    public void Show_WhenConditionIsFalse_ShouldNotRenderChildContent()
    {
        // Arrange & Act
        var cut = Render<Show>(parameters => parameters
            .Add(p => p.When, false)
            .AddChildContent("<div>Content to show</div>")
        );

        // Assert
        Assert.DoesNotContain("Content to show", cut.Markup);
    }

    [Fact]
    public void Show_WithNoChildContent_ShouldNotThrowException()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<Show>(parameters => parameters
                .Add(p => p.When, true)
            )
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Show_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Show();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public void Show_WithComplexContent_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<Show>(parameters => parameters
            .Add(p => p.When, true)
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "alert alert-success");
                builder.OpenElement(2, "h4");
                builder.AddContent(3, "Success!");
                builder.CloseElement();
                builder.OpenElement(4, "p");
                builder.AddContent(5, "Operation completed successfully.");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("alert alert-success", cut.Markup);
        Assert.Contains("Success!", cut.Markup);
        Assert.Contains("Operation completed successfully.", cut.Markup);
    }

    [Fact]
    public void Show_WithMultipleElements_ShouldRenderAll()
    {
        // Arrange & Act
        var cut = Render<Show>(parameters => parameters
            .Add(p => p.When, true)
            .AddChildContent("<div>First</div><div>Second</div><div>Third</div>")
        );

        // Assert
        Assert.Contains("First", cut.Markup);
        Assert.Contains("Second", cut.Markup);
        Assert.Contains("Third", cut.Markup);
    }

    [Fact]
    public void Show_WhenTrueWithNestedComponents_ShouldRenderNested()
    {
        // Arrange & Act
        var cut = Render<Show>(parameters => parameters
            .Add(p => p.When, true)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<Show>(0);
                builder.AddComponentParameter(1, "When", true);
                builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(b =>
                {
                    b.AddContent(0, "Nested content");
                }));
                builder.CloseComponent();
            })
        );

        // Assert
        Assert.Contains("Nested content", cut.Markup);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Show_WithDifferentConditions_ShouldBehaveConsistently(bool condition, bool shouldShow)
    {
        // Arrange & Act
        var cut = Render<Show>(parameters => parameters
            .Add(p => p.When, condition)
            .AddChildContent("<div>Test Content</div>")
        );

        // Assert
        if (shouldShow)
        {
            Assert.Contains("Test Content", cut.Markup);
        }
        else
        {
            Assert.DoesNotContain("Test Content", cut.Markup);
        }
    }

    [Fact]
    public void Show_WithMarkupString_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<Show>(parameters => parameters
            .Add(p => p.When, true)
            .AddChildContent("<strong>Bold Text</strong>")
        );

        // Assert
        Assert.Contains("Bold Text", cut.Markup);
        Assert.Contains("strong", cut.Markup);
    }

    [Fact]
    public void Show_WhenFalseWithComplexContent_ShouldNotRenderAnything()
    {
        // Arrange & Act
        var cut = Render<Show>(parameters => parameters
            .Add(p => p.When, false)
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "complex");
                builder.AddContent(2, "Complex content");
                builder.CloseElement();
            })
        );

        // Assert
        Assert.DoesNotContain("complex", cut.Markup);
        Assert.DoesNotContain("Complex content", cut.Markup);
    }
}
