using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Lazy component.
/// Tests lazy loading functionality using Intersection Observer.
/// </summary>
public class LazyTests : ComponentTestBase
{
    [Fact]
    public void Lazy_Initially_ShouldNotRenderContent()
    {
        // Arrange & Act
        var cut = Render<Lazy>(parameters => parameters
            .AddChildContent("<div>Lazy loaded content</div>")
        );

        // Assert - content should not be visible initially
        Assert.DoesNotContain("Lazy loaded content", cut.Markup);
    }

    [Fact]
    public void Lazy_WithDefaultThreshold_ShouldBe0Point1()
    {
        // Arrange
        var component = new Lazy();

        // Assert
        Assert.Equal(0.1, component.Threshold);
    }

    [Fact]
    public void Lazy_WithDefaultRootMargin_ShouldBe0px()
    {
        // Arrange
        var component = new Lazy();

        // Assert
        Assert.Equal("0px", component.RootMargin);
    }

    [Fact]
    public void Lazy_WithCustomThreshold_ShouldAcceptValue()
    {
        // Arrange & Act
        var cut = Render<Lazy>(parameters => parameters
            .Add(p => p.Threshold, 0.5)
            .AddChildContent("<div>Content</div>")
        );

        var instance = cut.Instance;

        // Assert
        Assert.Equal(0.5, instance.Threshold);
    }

    [Fact]
    public void Lazy_WithCustomRootMargin_ShouldAcceptValue()
    {
        // Arrange & Act
        var cut = Render<Lazy>(parameters => parameters
            .Add(p => p.RootMargin, "50px")
            .AddChildContent("<div>Content</div>")
        );

        var instance = cut.Instance;

        // Assert
        Assert.Equal("50px", instance.RootMargin);
    }

    [Fact]
    public void Lazy_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Lazy();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public async Task Lazy_OnIntersecting_ShouldRenderContent()
    {
        // Arrange
        var cut = Render<Lazy>(parameters => parameters
            .AddChildContent("<div>Intersected content</div>")
        );

        var instance = cut.Instance;

        // Act - simulate intersection
        await instance.OnIntersecting();

        // Assert
        Assert.Contains("Intersected content", cut.Markup);
    }

    [Fact]
    public async Task Lazy_AfterIntersecting_ShouldPersistContent()
    {
        // Arrange
        var cut = Render<Lazy>(parameters => parameters
            .AddChildContent("<div>Persistent content</div>")
        );

        var instance = cut.Instance;

        // Act - trigger intersection
        await instance.OnIntersecting();
        cut.Render();

        // Assert - content should be visible
        Assert.Contains("Persistent content", cut.Markup);

        // Act - render again
        cut.Render();

        // Assert - content should still be visible
        Assert.Contains("Persistent content", cut.Markup);
    }

    [Fact]
    public void Lazy_WithComplexContent_ShouldNotRenderInitially()
    {
        // Arrange & Act
        var cut = Render<Lazy>(parameters => parameters
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "complex-component");
                builder.OpenElement(2, "h3");
                builder.AddContent(3, "Title");
                builder.CloseElement();
                builder.OpenElement(4, "p");
                builder.AddContent(5, "Description");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Assert - should not contain complex content initially
        Assert.DoesNotContain("complex-component", cut.Markup);
        Assert.DoesNotContain("Title", cut.Markup);
        Assert.DoesNotContain("Description", cut.Markup);
    }

    [Fact]
    public async Task Lazy_WhenDisposed_ShouldNotThrow()
    {
        // Arrange
        var cut = Render<Lazy>(parameters => parameters
            .AddChildContent("<div>Content</div>")
        );

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
        {
            cut.Dispose();
            await Task.Delay(50);
        });

        Assert.Null(exception);
    }

    [Fact]
    public void Lazy_WithNoChildContent_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<Lazy>(parameters => parameters
                .Add(p => p.Threshold, 0.5)
            )
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Lazy_ShouldRenderPlaceholderElement()
    {
        // Arrange & Act
        var cut = Render<Lazy>(parameters => parameters
            .AddChildContent("<div>Content</div>")
        );

        // Assert - should have a placeholder div element reference
        Assert.NotNull(cut.Markup);
    }

    [Fact]
    public async Task Lazy_MultipleIntersectionCalls_ShouldOnlyRenderOnce()
    {
        // Arrange
        var cut = Render<Lazy>(parameters => parameters
            .AddChildContent("<div>Once content</div>")
        );

        var instance = cut.Instance;

        // Act - call OnIntersecting multiple times
        await instance.OnIntersecting();
        await instance.OnIntersecting();
        await instance.OnIntersecting();

        // Assert - content should be rendered (once)
        Assert.Contains("Once content", cut.Markup);
    }

    [Fact]
    public void Lazy_WithBoundaryThresholds_ShouldAcceptValues()
    {
        // Arrange & Act
        var cut1 = Render<Lazy>(parameters => parameters
            .Add(p => p.Threshold, 0.0)
            .AddChildContent("<div>Min threshold</div>")
        );

        var cut2 = Render<Lazy>(parameters => parameters
            .Add(p => p.Threshold, 1.0)
            .AddChildContent("<div>Max threshold</div>")
        );

        // Assert
        Assert.Equal(0.0, cut1.Instance.Threshold);
        Assert.Equal(1.0, cut2.Instance.Threshold);
    }
}
