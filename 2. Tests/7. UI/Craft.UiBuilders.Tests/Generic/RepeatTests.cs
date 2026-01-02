using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Repeat component.
/// Tests collection rendering with item templates and empty state handling.
/// </summary>
public class RepeatTests : ComponentTestBase
{
    [Fact]
    public void Repeat_WithItems_ShouldRenderAllItems()
    {
        // Arrange
        var items = new[] { "Apple", "Banana", "Cherry" };

        // Act
        var cut = Render<Repeat<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, item);
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("Apple", cut.Markup);
        Assert.Contains("Banana", cut.Markup);
        Assert.Contains("Cherry", cut.Markup);
    }

    [Fact]
    public void Repeat_WithEmptyCollection_ShouldRenderEmptyTemplate()
    {
        // Arrange
        var items = Array.Empty<string>();

        // Act
        var cut = Render<Repeat<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.EmptyTemplate, builder => builder.AddMarkupContent(0, "<div>No items found</div>"))
        );

        // Assert
        Assert.Contains("No items found", cut.Markup);
    }

    [Fact]
    public void Repeat_WithNullCollection_ShouldRenderEmptyTemplate()
    {
        // Arrange & Act
        var cut = Render<Repeat<string>>(parameters => parameters
            .Add(p => p.Items, (IEnumerable<string>?)null)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.EmptyTemplate, builder => builder.AddMarkupContent(0, "<div>No items</div>"))
        );

        // Assert
        Assert.Contains("No items", cut.Markup);
    }

    [Fact]
    public void Repeat_WithEmptyCollectionAndNoEmptyTemplate_ShouldRenderNothing()
    {
        // Arrange
        var items = Array.Empty<string>();

        // Act
        var cut = Render<Repeat<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
        );

        // Assert
        var markup = cut.Markup.Trim();
        Assert.True(string.IsNullOrEmpty(markup) || markup == "<!--!--><!--!-->");
    }

    [Fact]
    public void Repeat_WithComplexObjects_ShouldRenderCorrectly()
    {
        // Arrange
        var items = new[]
        {
            new { Id = 1, Name = "John", Age = 30 },
            new { Id = 2, Name = "Jane", Age = 25 },
            new { Id = 3, Name = "Bob", Age = 35 }
        };

        // Act
        var cut = Render<Repeat<dynamic>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "data-id", item.Id);
                builder.AddContent(2, $"{item.Name} is {item.Age} years old");
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("John is 30 years old", cut.Markup);
        Assert.Contains("Jane is 25 years old", cut.Markup);
        Assert.Contains("Bob is 35 years old", cut.Markup);
        Assert.Contains("data-id=\"1\"", cut.Markup);
        Assert.Contains("data-id=\"2\"", cut.Markup);
        Assert.Contains("data-id=\"3\"", cut.Markup);
    }

    [Fact]
    public void Repeat_WithSingleItem_ShouldRenderCorrectly()
    {
        // Arrange
        var items = new[] { "OnlyItem" };

        // Act
        var cut = Render<Repeat<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
        );

        // Assert
        Assert.Contains("OnlyItem", cut.Markup);
    }

    [Fact]
    public void Repeat_WithListOfIntegers_ShouldRenderCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var cut = Render<Repeat<int>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddContent(1, item.ToString());
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("<span>1</span>", cut.Markup);
        Assert.Contains("<span>2</span>", cut.Markup);
        Assert.Contains("<span>3</span>", cut.Markup);
        Assert.Contains("<span>4</span>", cut.Markup);
        Assert.Contains("<span>5</span>", cut.Markup);
    }

    [Fact]
    public void Repeat_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Repeat<string>();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public void Repeat_WithEmptyString_ShouldStillRender()
    {
        // Arrange
        var items = new[] { "First", "", "Third" };

        // Act
        var cut = Render<Repeat<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, string.IsNullOrEmpty(item) ? "(empty)" : item);
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("First", cut.Markup);
        Assert.Contains("(empty)", cut.Markup);
        Assert.Contains("Third", cut.Markup);
    }
}
