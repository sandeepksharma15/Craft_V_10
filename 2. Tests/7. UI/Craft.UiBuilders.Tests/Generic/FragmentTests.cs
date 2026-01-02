using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Fragment component.
/// Tests wrapper-free content rendering.
/// </summary>
public class FragmentTests : ComponentTestBase
{
    [Fact]
    public void Fragment_ShouldRenderChildContent()
    {
        // Arrange & Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent("<div>Fragment content</div>")
        );

        // Assert
        Assert.Contains("Fragment content", cut.Markup);
    }

    [Fact]
    public void Fragment_ShouldNotAddWrapperElement()
    {
        // Arrange & Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent("<div class=\"test\">Content</div>")
        );

        // Assert - should contain the content but no extra wrapper
        Assert.Contains("test", cut.Markup);
        Assert.Contains("Content", cut.Markup);
        
        // The markup should be minimal (just the content and Blazor comments)
        var trimmedMarkup = cut.Markup.Trim();
        Assert.DoesNotContain("<fragment", trimmedMarkup.ToLower());
    }

    [Fact]
    public void Fragment_WithMultipleElements_ShouldRenderAll()
    {
        // Arrange & Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent(
                "<div>First</div>" +
                "<div>Second</div>" +
                "<div>Third</div>"
            )
        );

        // Assert
        Assert.Contains("First", cut.Markup);
        Assert.Contains("Second", cut.Markup);
        Assert.Contains("Third", cut.Markup);
    }

    [Fact]
    public void Fragment_WithComplexContent_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "p");
                builder.AddContent(1, "Paragraph 1");
                builder.CloseElement();
                
                builder.OpenElement(2, "p");
                builder.AddContent(3, "Paragraph 2");
                builder.CloseElement();
                
                builder.OpenElement(4, "span");
                builder.AddAttribute(5, "class", "highlight");
                builder.AddContent(6, "Highlighted text");
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("Paragraph 1", cut.Markup);
        Assert.Contains("Paragraph 2", cut.Markup);
        Assert.Contains("highlight", cut.Markup);
        Assert.Contains("Highlighted text", cut.Markup);
    }

    [Fact]
    public void Fragment_WithNoContent_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<Fragment>(parameters => parameters.AddChildContent(""))
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Fragment_WithEmptyContent_ShouldRenderNothing()
    {
        // Arrange & Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent("")
        );

        // Assert
        var markup = cut.Markup.Trim();
        Assert.True(string.IsNullOrEmpty(markup) || markup == "<!--!-->");
    }

    [Fact]
    public void Fragment_ShouldInheritFromComponentBase()
    {
        // Arrange
        var component = new Fragment();

        // Assert
        Assert.IsAssignableFrom<Microsoft.AspNetCore.Components.ComponentBase>(component);
    }

    [Fact]
    public void Fragment_WithNestedComponents_ShouldRender()
    {
        // Arrange & Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "outer");
                
                builder.OpenElement(2, "div");
                builder.AddAttribute(3, "class", "inner");
                builder.AddContent(4, "Nested content");
                builder.CloseElement();
                
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("outer", cut.Markup);
        Assert.Contains("inner", cut.Markup);
        Assert.Contains("Nested content", cut.Markup);
    }

    [Fact]
    public void Fragment_WithTextOnly_ShouldRenderText()
    {
        // Arrange & Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent("Just plain text")
        );

        // Assert
        Assert.Contains("Just plain text", cut.Markup);
    }

    [Fact]
    public void Fragment_WithMixedContent_ShouldRenderAll()
    {
        // Arrange & Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent(builder =>
            {
                builder.AddContent(0, "Text before");
                
                builder.OpenElement(1, "strong");
                builder.AddContent(2, "Bold text");
                builder.CloseElement();
                
                builder.AddContent(3, "Text after");
            })
        );

        // Assert
        Assert.Contains("Text before", cut.Markup);
        Assert.Contains("Bold text", cut.Markup);
        Assert.Contains("Text after", cut.Markup);
    }

    [Fact]
    public void Fragment_AsGroupingMechanism_ShouldWork()
    {
        // Arrange & Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent(builder =>
            {
                // Simulate grouping multiple elements without a wrapper
                for (int i = 1; i <= 3; i++)
                {
                    builder.OpenElement(i * 2, "div");
                    builder.AddAttribute(i * 2 + 1, "data-index", i);
                    builder.AddContent(i * 2 + 2, $"Item {i}");
                    builder.CloseElement();
                }
            })
        );

        // Assert
        Assert.Contains("Item 1", cut.Markup);
        Assert.Contains("Item 2", cut.Markup);
        Assert.Contains("Item 3", cut.Markup);
        Assert.Contains("data-index=\"1\"", cut.Markup);
        Assert.Contains("data-index=\"2\"", cut.Markup);
        Assert.Contains("data-index=\"3\"", cut.Markup);
    }

    [Fact]
    public void Fragment_ShouldNotInheritFromCraftComponent()
    {
        // Arrange
        var component = new Fragment();

        // Assert - Fragment should be lightweight, not inherit from CraftComponent
        Assert.IsNotAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public void Fragment_WithConditionalContent_ShouldRender()
    {
        // Arrange
        var showContent = true;

        // Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent(builder =>
            {
                if (showContent)
                {
                    builder.OpenElement(0, "div");
                    builder.AddContent(1, "Conditional content");
                    builder.CloseElement();
                }
            })
        );

        // Assert
        Assert.Contains("Conditional content", cut.Markup);
    }

    [Fact]
    public void Fragment_WithListItems_ShouldRenderAllItems()
    {
        // Arrange
        var items = new[] { "Alpha", "Beta", "Gamma" };

        // Act
        var cut = Render<Fragment>(parameters => parameters
            .AddChildContent(builder =>
            {
                int sequence = 0;
                foreach (var item in items)
                {
                    builder.OpenElement(sequence++, "li");
                    builder.AddContent(sequence++, item);
                    builder.CloseElement();
                }
            })
        );

        // Assert
        Assert.Contains("<li>Alpha</li>", cut.Markup);
        Assert.Contains("<li>Beta</li>", cut.Markup);
        Assert.Contains("<li>Gamma</li>", cut.Markup);
    }
}
