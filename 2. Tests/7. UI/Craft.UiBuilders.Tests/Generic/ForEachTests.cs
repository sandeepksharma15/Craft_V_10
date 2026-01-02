using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the ForEach component.
/// Tests collection iteration, empty states, separators, and index support.
/// </summary>
public class ForEachTests : ComponentTestBase
{
    [Fact]
    public void ForEach_WithSimpleCollection_ShouldRenderAllItems()
    {
        // Arrange
        var items = new[] { "Apple", "Banana", "Cherry" };

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, context.Item);
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("Apple", cut.Markup);
        Assert.Contains("Banana", cut.Markup);
        Assert.Contains("Cherry", cut.Markup);
    }

    [Fact]
    public void ForEach_WithEmptyCollection_ShouldRenderEmptyTemplate()
    {
        // Arrange
        var items = Array.Empty<string>();

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder => builder.AddContent(0, context.Item))
            .Add(p => p.Empty, builder => builder.AddMarkupContent(0, "<div>No items found</div>"))
        );

        // Assert
        Assert.Contains("No items found", cut.Markup);
    }

    [Fact]
    public void ForEach_WithNullCollection_ShouldRenderEmptyTemplate()
    {
        // Arrange & Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, (IEnumerable<string>?)null)
            .Add(p => p.ItemContent, context => builder => builder.AddContent(0, context.Item))
            .Add(p => p.Empty, builder => builder.AddMarkupContent(0, "<div>No items</div>"))
        );

        // Assert
        Assert.Contains("No items", cut.Markup);
    }

    [Fact]
    public void ForEach_WithEmptyCollectionAndNoEmptyTemplate_ShouldRenderNothing()
    {
        // Arrange
        var items = Array.Empty<string>();

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder => builder.AddContent(0, context.Item))
        );

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void ForEach_WithItemIndex_ShouldProvideCorrectIndex()
    {
        // Arrange
        var items = new[] { "First", "Second", "Third" };

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, $"{context.Index}: {context.Item}");
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("0: First", cut.Markup);
        Assert.Contains("1: Second", cut.Markup);
        Assert.Contains("2: Third", cut.Markup);
    }

    [Fact]
    public void ForEach_WithSeparator_ShouldRenderSeparatorBetweenItems()
    {
        // Arrange
        var items = new[] { "A", "B", "C" };

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder => builder.AddContent(0, context.Item))
            .Add(p => p.Separator, builder => builder.AddMarkupContent(0, "<hr/>"))
        );

        // Assert
        var markup = cut.Markup;
        var hrCount = markup.Split("<hr", StringSplitOptions.None).Length - 1;
        Assert.Equal(2, hrCount); // Should have 2 separators for 3 items
        Assert.Contains("A", markup);
        Assert.Contains("B", markup);
        Assert.Contains("C", markup);
    }

    [Fact]
    public void ForEach_WithSeparator_ShouldNotRenderSeparatorForSingleItem()
    {
        // Arrange
        var items = new[] { "OnlyOne" };

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder => builder.AddContent(0, context.Item))
            .Add(p => p.Separator, builder => builder.AddMarkupContent(0, "<hr/>"))
        );

        // Assert
        Assert.Contains("OnlyOne", cut.Markup);
        Assert.DoesNotContain("<hr", cut.Markup);
    }

    [Fact]
    public void ForEach_WithComplexObjects_ShouldRenderCorrectly()
    {
        // Arrange
        var items = new[]
        {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 }
        };

        // Act
        var cut = Render<ForEach<Person>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "person");
                builder.AddContent(2, $"{context.Item.Name} - {context.Item.Age}");
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("Alice - 30", cut.Markup);
        Assert.Contains("Bob - 25", cut.Markup);
        Assert.Contains("class=\"person\"", cut.Markup);
    }

    [Fact]
    public void ForEach_WithIsFirstProperty_ShouldIdentifyFirstItem()
    {
        // Arrange
        var items = new[] { "First", "Second", "Third" };

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", context.IsFirst ? "first-item" : "other-item");
                builder.AddContent(2, context.Item);
                builder.CloseElement();
            })
        );

        // Assert
        var markup = cut.Markup;
        Assert.Contains("class=\"first-item\"", markup);
        var firstItemCount = markup.Split("first-item", StringSplitOptions.None).Length - 1;
        Assert.Equal(1, firstItemCount); // Only one item should be marked as first
    }

    [Fact]
    public void ForEach_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new ForEach<string>();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public void ForEach_WithNestedForEach_ShouldRenderCorrectly()
    {
        // Arrange
        var categories = new[]
        {
            new Category { Name = "Fruits", Items = new[] { "Apple", "Banana" } },
            new Category { Name = "Vegetables", Items = new[] { "Carrot" } }
        };

        // Act
        var cut = Render<ForEach<Category>>(parameters => parameters
            .Add(p => p.Collection, categories)
            .Add(p => p.ItemContent, categoryContext => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "category");
                builder.AddContent(2, categoryContext.Item.Name);
                
                builder.OpenComponent<ForEach<string>>(3);
                builder.AddComponentParameter(4, "Collection", categoryContext.Item.Items);
                builder.AddComponentParameter(5, "ItemContent", (RenderFragment<ForEach<string>.ItemContext>)(itemContext => 
                    itemBuilder => itemBuilder.AddContent(0, $" - {itemContext.Item}")));
                builder.CloseComponent();
                
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("Fruits", cut.Markup);
        Assert.Contains("Apple", cut.Markup);
        Assert.Contains("Banana", cut.Markup);
        Assert.Contains("Vegetables", cut.Markup);
        Assert.Contains("Carrot", cut.Markup);
    }

    [Fact]
    public void ForEach_WithLargeCollection_ShouldHandleEfficiently()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).Select(i => $"Item {i}").ToArray();

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder => builder.AddContent(0, context.Item))
        );

        // Assert
        Assert.Contains("Item 1", cut.Markup);
        Assert.Contains("Item 50", cut.Markup);
        Assert.Contains("Item 100", cut.Markup);
    }

    [Fact]
    public void ForEach_WithComplexSeparator_ShouldRenderCorrectly()
    {
        // Arrange
        var items = new[] { "One", "Two", "Three" };

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddContent(1, context.Item);
                builder.CloseElement();
            })
            .Add(p => p.Separator, builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "separator");
                builder.AddContent(2, " | ");
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("One", cut.Markup);
        Assert.Contains("Two", cut.Markup);
        Assert.Contains("Three", cut.Markup);
        Assert.Contains("class=\"separator\"", cut.Markup);
        Assert.Contains("|", cut.Markup);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(5, false)]
    public void ForEach_ItemContext_IsFirstProperty_ShouldBeCorrect(int index, bool expectedIsFirst)
    {
        // Arrange
        var items = Enumerable.Range(0, 10).ToArray();

        // Act
        var cut = Render<ForEach<int>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder =>
            {
                if (context.Index == index)
                {
                    builder.OpenElement(0, "div");
                    builder.AddAttribute(1, "data-is-first", context.IsFirst.ToString().ToLower());
                    builder.AddContent(2, context.Item);
                    builder.CloseElement();
                }
            })
        );

        // Assert
        if (index < items.Length)
        {
            Assert.Contains($"data-is-first=\"{expectedIsFirst.ToString().ToLower()}\"", cut.Markup);
        }
    }

    [Fact]
    public void ForEach_WithEmptyStringItems_ShouldRenderCorrectly()
    {
        // Arrange
        var items = new[] { "First", "", "Third" };

        // Act
        var cut = Render<ForEach<string>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, string.IsNullOrEmpty(context.Item) ? "[Empty]" : context.Item);
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("First", cut.Markup);
        Assert.Contains("[Empty]", cut.Markup);
        Assert.Contains("Third", cut.Markup);
    }

    [Fact]
    public void ForEach_WithNullableItems_ShouldHandleCorrectly()
    {
        // Arrange
        var items = new int?[] { 1, null, 3 };

        // Act
        var cut = Render<ForEach<int?>>(parameters => parameters
            .Add(p => p.Collection, items)
            .Add(p => p.ItemContent, context => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, context.Item?.ToString() ?? "NULL");
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("1", cut.Markup);
        Assert.Contains("NULL", cut.Markup);
        Assert.Contains("3", cut.Markup);
    }

    [Fact]
    public void ForEach_ItemContext_ShouldBeRecord()
    {
        // Assert
        var itemContext = new ForEach<string>.ItemContext("test", 0);
        Assert.NotNull(itemContext);
        Assert.Equal("test", itemContext.Item);
        Assert.Equal(0, itemContext.Index);
        Assert.True(itemContext.IsFirst);
    }

    [Fact]
    public void ForEach_WithDifferentItemContexts_ShouldNotBeEqual()
    {
        // Arrange
        var context1 = new ForEach<string>.ItemContext("test", 0);
        var context2 = new ForEach<string>.ItemContext("test", 1);

        // Assert
        Assert.NotEqual(context1, context2);
    }

    [Fact]
    public void ForEach_WithSameItemContexts_ShouldBeEqual()
    {
        // Arrange
        var context1 = new ForEach<string>.ItemContext("test", 0);
        var context2 = new ForEach<string>.ItemContext("test", 0);

        // Assert
        Assert.Equal(context1, context2);
    }

    // Helper classes for testing
    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class Category
    {
        public string Name { get; set; } = string.Empty;
        public string[] Items { get; set; } = Array.Empty<string>();
    }
}
