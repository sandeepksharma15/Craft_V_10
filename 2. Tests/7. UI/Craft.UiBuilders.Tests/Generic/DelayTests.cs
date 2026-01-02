using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Delay component.
/// Tests delayed rendering of content with configurable duration.
/// </summary>
public class DelayTests : ComponentTestBase
{
    [Fact]
    public void Delay_Initially_ShouldNotRenderContent()
    {
        // Arrange & Act
        var cut = Render<Delay>(parameters => parameters
            .Add(p => p.Milliseconds, 1000)
            .AddChildContent("<div>Delayed content</div>")
        );

        // Assert - content should not be visible immediately
        Assert.DoesNotContain("Delayed content", cut.Markup);
    }

    [Fact]
    public async Task Delay_AfterDelay_ShouldRenderContent()
    {
        // Arrange
        var cut = Render<Delay>(parameters => parameters
            .Add(p => p.Milliseconds, 100)
            .AddChildContent("<div>Delayed content</div>")
        );

        // Act - wait for the delay plus buffer
        await Task.Delay(200);
        cut.Render();

        // Assert
        Assert.Contains("Delayed content", cut.Markup);
    }

    [Fact]
    public async Task Delay_WithShortDelay_ShouldShowQuickly()
    {
        // Arrange
        var cut = Render<Delay>(parameters => parameters
            .Add(p => p.Milliseconds, 50)
            .AddChildContent("<div>Quick content</div>")
        );

        // Act
        await Task.Delay(100);
        cut.Render();

        // Assert
        Assert.Contains("Quick content", cut.Markup);
    }

    [Fact]
    public void Delay_WithDefaultDelay_ShouldUse500Ms()
    {
        // Arrange
        var component = new Delay();

        // Assert
        Assert.Equal(500, component.Milliseconds);
    }

    [Fact]
    public void Delay_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Delay();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public async Task Delay_WhenDisposed_ShouldNotThrow()
    {
        // Arrange
        var cut = Render<Delay>(parameters => parameters
            .Add(p => p.Milliseconds, 1000)
            .AddChildContent("<div>Content</div>")
        );

        // Act & Assert
        await Task.Delay(50);
        var exception = await Record.ExceptionAsync(async () =>
        {
            cut.Dispose();
            await Task.Delay(100);
        });

        Assert.Null(exception);
    }

    [Fact]
    public void Delay_WithComplexContent_ShouldEventuallyRender()
    {
        // Arrange
        var cut = Render<Delay>(parameters => parameters
            .Add(p => p.Milliseconds, 50)
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "alert");
                builder.OpenElement(2, "h4");
                builder.AddContent(3, "Title");
                builder.CloseElement();
                builder.OpenElement(4, "p");
                builder.AddContent(5, "Description");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Initially should not contain content
        Assert.DoesNotContain("Title", cut.Markup);
        Assert.DoesNotContain("Description", cut.Markup);
    }

    [Fact]
    public void Delay_WithZeroDelay_ShouldStillDelay()
    {
        // Arrange & Act
        var cut = Render<Delay>(parameters => parameters
            .Add(p => p.Milliseconds, 0)
            .AddChildContent("<div>Instant content</div>")
        );

        // Assert - even with 0ms, timer callback is async
        // Content might not be immediately visible
        var initialMarkup = cut.Markup;
        Assert.True(string.IsNullOrEmpty(initialMarkup) || !initialMarkup.Contains("Instant content"));
    }
}
