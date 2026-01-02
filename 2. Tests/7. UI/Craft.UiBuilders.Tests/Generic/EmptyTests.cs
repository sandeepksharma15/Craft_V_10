using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Empty component.
/// Tests empty state display for collections.
/// </summary>
public class EmptyTests : ComponentTestBase
{
    [Fact]
    public void Empty_WithEmptyCollection_ShouldRenderContent()
    {
        // Arrange
        var items = Array.Empty<string>();

        // Act
        var cut = Render<Empty<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent("<div>No items found</div>")
        );

        // Assert
        Assert.Contains("No items found", cut.Markup);
    }

    [Fact]
    public void Empty_WithNullCollection_ShouldRenderContent()
    {
        // Arrange & Act
        var cut = Render<Empty<string>>(parameters => parameters
            .Add(p => p.Items, (IEnumerable<string>?)null)
            .AddChildContent("<div>Collection is null</div>")
        );

        // Assert
        Assert.Contains("Collection is null", cut.Markup);
    }

    [Fact]
    public void Empty_WithNonEmptyCollection_ShouldNotRenderContent()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = Render<Empty<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent("<div>No items</div>")
        );

        // Assert
        Assert.DoesNotContain("No items", cut.Markup);
    }

    [Fact]
    public void Empty_WithSingleItem_ShouldNotRenderContent()
    {
        // Arrange
        var items = new[] { "OnlyItem" };

        // Act
        var cut = Render<Empty<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent("<div>Empty state</div>")
        );

        // Assert
        Assert.DoesNotContain("Empty state", cut.Markup);
    }

    [Fact]
    public void Empty_WithListOfIntegers_ShouldWork()
    {
        // Arrange
        var emptyList = new List<int>();

        // Act
        var cut = Render<Empty<int>>(parameters => parameters
            .Add(p => p.Items, emptyList)
            .AddChildContent("<div>No numbers</div>")
        );

        // Assert
        Assert.Contains("No numbers", cut.Markup);
    }

    [Fact]
    public void Empty_WithComplexObjects_ShouldWork()
    {
        // Arrange
        var emptyItems = Array.Empty<object>();

        // Act
        var cut = Render<Empty<object>>(parameters => parameters
            .Add(p => p.Items, emptyItems)
            .AddChildContent("<div>No objects available</div>")
        );

        // Assert
        Assert.Contains("No objects available", cut.Markup);
    }

    [Fact]
    public void Empty_WithComplexEmptyContent_ShouldRenderCorrectly()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var cut = Render<Empty<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "empty-state");
                builder.OpenElement(2, "h3");
                builder.AddContent(3, "No Results");
                builder.CloseElement();
                builder.OpenElement(4, "p");
                builder.AddContent(5, "Try adjusting your filters");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Assert
            Assert.Contains("empty-state", cut.Markup);
            Assert.Contains("No Results", cut.Markup);
            Assert.Contains("Try adjusting your filters", cut.Markup);
        }

        [Fact]
        public void Empty_WithNonEmptyList_ShouldNotShowContent()
        {
            // Arrange & Act
            var cut = Render<Empty<string>>(parameters => parameters
                .Add(p => p.Items, ["Item1", "Item2"])
                .AddChildContent("<div>Empty message</div>")
            );

            // Assert - should not show empty message
            Assert.DoesNotContain("Empty message", cut.Markup);
        }

    [Fact]
    public void Empty_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Empty<string>();

        // Assert
        Assert.IsType<Craft.UiComponents.CraftComponent>(component, exactMatch: false);
    }

    [Fact]
    public void Empty_WithNoChildContent_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<Empty<string>>(parameters => parameters
                .Add(p => p.Items, [])
            )
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Empty_WithEnumerableAsQueryable_ShouldWork()
    {
        // Arrange
        var queryable = new List<int>().AsQueryable();

        // Act
        var cut = Render<Empty<int>>(parameters => parameters
            .Add(p => p.Items, queryable)
            .AddChildContent("<div>No query results</div>")
        );

        // Assert
        Assert.Contains("No query results", cut.Markup);
    }

    [Fact]
    public void Empty_WithCollectionOfNullableTypes_ShouldWork()
    {
        // Arrange
        var items = new List<int?>();

        // Act
        var cut = Render<Empty<int?>>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent("<div>No nullable integers</div>")
        );

        // Assert
        Assert.Contains("No nullable integers", cut.Markup);
    }

    [Fact]
    public void Empty_WithIEnumerable_ShouldWork()
    {
        // Arrange
        static IEnumerable<string> GetEmptyEnumerable()
        {
            yield break;
        }

        // Act
        var cut = Render<Empty<string>>(parameters => parameters
            .Add(p => p.Items, GetEmptyEnumerable())
            .AddChildContent("<div>Generator empty</div>")
        );

        // Assert
        Assert.Contains("Generator empty", cut.Markup);
    }
}
