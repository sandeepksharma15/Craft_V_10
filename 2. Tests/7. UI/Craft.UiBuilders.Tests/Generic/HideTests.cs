using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Hide component.
/// Tests conditional rendering when condition is false (inverse of Show).
/// </summary>
public class HideTests : ComponentTestBase
{
    [Fact]
    public void Hide_WhenConditionIsTrue_ShouldNotRenderChildContent()
    {
        // Arrange & Act
        var cut = Render<Hide>(parameters => parameters
            .Add(p => p.When, true)
            .AddChildContent("<div>Content to hide</div>")
        );

        // Assert
        Assert.DoesNotContain("Content to hide", cut.Markup);
    }

    [Fact]
    public void Hide_WhenConditionIsFalse_ShouldRenderChildContent()
    {
        // Arrange & Act
        var cut = Render<Hide>(parameters => parameters
            .Add(p => p.When, false)
            .AddChildContent("<div>Content to hide</div>")
        );

        // Assert
        Assert.Contains("Content to hide", cut.Markup);
    }

    [Fact]
    public void Hide_WithNoChildContent_ShouldNotThrowException()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<Hide>(parameters => parameters
                .Add(p => p.When, false)
            )
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Hide_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Hide();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public void Hide_WithComplexContent_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<Hide>(parameters => parameters
            .Add(p => p.When, false)
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "alert alert-warning");
                builder.OpenElement(2, "h4");
                builder.AddContent(3, "Warning!");
                builder.CloseElement();
                builder.OpenElement(4, "p");
                builder.AddContent(5, "This content is shown when condition is false.");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("alert alert-warning", cut.Markup);
        Assert.Contains("Warning!", cut.Markup);
        Assert.Contains("This content is shown when condition is false.", cut.Markup);
    }

    [Fact]
    public void Hide_WithMultipleElements_ShouldRenderAll()
    {
        // Arrange & Act
        var cut = Render<Hide>(parameters => parameters
            .Add(p => p.When, false)
            .AddChildContent("<div>First</div><div>Second</div><div>Third</div>")
        );

        // Assert
        Assert.Contains("First", cut.Markup);
        Assert.Contains("Second", cut.Markup);
        Assert.Contains("Third", cut.Markup);
    }

    [Fact]
    public void Hide_WhenFalseWithNestedComponents_ShouldRenderNested()
    {
        // Arrange & Act
        var cut = Render<Hide>(parameters => parameters
            .Add(p => p.When, false)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<Hide>(0);
                builder.AddComponentParameter(1, "When", false);
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
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Hide_WithDifferentConditions_ShouldBehaveConsistently(bool condition, bool shouldShow)
    {
        // Arrange & Act
        var cut = Render<Hide>(parameters => parameters
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
    public void Hide_OppositeOfShow_ShouldBehaveAsExpected()
    {
        // This test verifies that Hide is the logical opposite of Show
        // Arrange
        var hideComponent = Render<Hide>(parameters => parameters
            .Add(p => p.When, true)
            .AddChildContent("<div>Content</div>")
        );

        var showComponent = Render<Show>(parameters => parameters
            .Add(p => p.When, true)
            .AddChildContent("<div>Content</div>")
        );

        // Assert - When condition is true, Hide should not show content, but Show should
        Assert.DoesNotContain("Content", hideComponent.Markup);
        Assert.Contains("Content", showComponent.Markup);
    }

    [Fact]
    public void Hide_WhenUsedForLoadingStates_ShouldHideContentDuringLoad()
    {
        // Arrange - Simulating hiding content during loading
        var isLoading = true;
        var cut = Render<Hide>(parameters => parameters
            .Add(p => p.When, isLoading)
            .AddChildContent("<div>Loaded Content</div>")
        );

        // Assert - Content should be hidden while loading
        Assert.DoesNotContain("Loaded Content", cut.Markup);
    }

    [Fact]
    public void Hide_WhenUsedForLoadingStates_ShouldShowContentAfterLoad()
    {
        // Arrange - Simulating showing content after loading
        var isLoading = false;
        var cut = Render<Hide>(parameters => parameters
            .Add(p => p.When, isLoading)
            .AddChildContent("<div>Loaded Content</div>")
        );

        // Assert - Content should be visible when not loading
        Assert.Contains("Loaded Content", cut.Markup);
    }

    [Fact]
    public void Hide_WithMarkupString_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<Hide>(parameters => parameters
            .Add(p => p.When, false)
            .AddChildContent("<em>Italic Text</em>")
        );

        // Assert
        Assert.Contains("Italic Text", cut.Markup);
        Assert.Contains("em", cut.Markup);
    }

    [Fact]
    public void Hide_WhenTrueWithComplexContent_ShouldNotRenderAnything()
    {
        // Arrange & Act
        var cut = Render<Hide>(parameters => parameters
            .Add(p => p.When, true)
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
