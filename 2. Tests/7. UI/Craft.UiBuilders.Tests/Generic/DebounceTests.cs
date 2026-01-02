using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Debounce component.
/// Tests debounced rendering based on value changes.
/// </summary>
public class DebounceTests : ComponentTestBase
{
    [Fact]
    public void Debounce_Initially_ShouldRenderContent()
    {
        // Arrange & Act
        var cut = Render<Debounce<string>>(parameters => parameters
            .Add(p => p.Value, "initial")
            .Add(p => p.DelayMs, 300)
            .AddChildContent("<div>Content</div>")
        );

        // Assert - should render content initially
        Assert.Contains("Content", cut.Markup);
    }

    [Fact]
    public async Task Debounce_AfterDelay_ShouldShowContent()
    {
        // Arrange
        var cut = Render<Debounce<string>>(parameters => parameters
            .Add(p => p.Value, "initial")
            .Add(p => p.DelayMs, 100)
            .AddChildContent("<div>Debounced content</div>")
        );

        // Wait for debounce
        await Task.Delay(150);
        cut.Render();

        // Assert - content should be visible after delay
        Assert.Contains("Debounced content", cut.Markup);
    }

    [Fact]
    public void Debounce_WithDefaultDelay_ShouldBe300Ms()
    {
        // Arrange
        var component = new Debounce<string>();

        // Assert
        Assert.Equal(300, component.DelayMs);
    }

    [Fact]
    public void Debounce_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Debounce<string>();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public void Debounce_WithIntegerValue_ShouldWork()
    {
        // Arrange & Act
        var cut = Render<Debounce<int>>(parameters => parameters
            .Add(p => p.Value, 42)
            .Add(p => p.DelayMs, 200)
            .AddChildContent("<div>Integer debounce</div>")
        );

        // Assert
        Assert.Contains("Integer debounce", cut.Markup);
    }

    [Fact]
    public void Debounce_WithNullableValue_ShouldWork()
    {
        // Arrange & Act
        var cut = Render<Debounce<int?>>(parameters => parameters
            .Add(p => p.Value, (int?)null)
            .Add(p => p.DelayMs, 200)
            .AddChildContent("<div>Nullable debounce</div>")
        );

        // Assert
        Assert.Contains("Nullable debounce", cut.Markup);
    }

    [Fact]
    public async Task Debounce_OnDebouncedCallback_ShouldBeInvokable()
    {
        // Arrange
        var cut = Render<Debounce<string>>(parameters => parameters
            .Add(p => p.Value, "test")
            .Add(p => p.DelayMs, 100)
            .Add(p => p.OnDebounced, value => Task.CompletedTask)
            .AddChildContent("<div>Content</div>")
        );

        // Act - wait for initial debounce
        await Task.Delay(150);

        // Assert - This test verifies the callback parameter is accepted
        Assert.True(cut.Instance.OnDebounced.HasDelegate);
    }

    [Fact]
    public async Task Debounce_WhenDisposed_ShouldNotThrow()
    {
        // Arrange
        var cut = Render<Debounce<string>>(parameters => parameters
            .Add(p => p.Value, "test")
            .Add(p => p.DelayMs, 300)
            .AddChildContent("<div>Content</div>")
        );

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
        {
            cut.Dispose();
            await Task.Delay(100);
        });

        Assert.Null(exception);
    }

    [Fact]
    public void Debounce_WithComplexType_ShouldWork()
    {
        // Arrange
        var searchQuery = new { Text = "search", Filters = new[] { "filter1", "filter2" } };

        // Act
        var cut = Render<Debounce<dynamic>>(parameters => parameters
            .Add(p => p.Value, searchQuery)
            .Add(p => p.DelayMs, 250)
            .AddChildContent("<div>Search results</div>")
        );

        // Assert
        Assert.Contains("Search results", cut.Markup);
    }

    [Fact]
    public async Task Debounce_WithZeroDelay_ShouldStillDebounce()
    {
        // Arrange
        var cut = Render<Debounce<string>>(parameters => parameters
            .Add(p => p.Value, "test")
            .Add(p => p.DelayMs, 0)
            .AddChildContent("<div>Instant</div>")
        );

        // Brief wait for timer callback
        await Task.Delay(50);
        cut.Render();

        // Assert
        Assert.Contains("Instant", cut.Markup);
    }

    [Fact]
    public void Debounce_WithCustomDelay_ShouldAcceptValue()
    {
        // Arrange & Act
        var cut = Render<Debounce<string>>(parameters => parameters
            .Add(p => p.Value, "test")
            .Add(p => p.DelayMs, 500)
            .AddChildContent("<div>Content</div>")
        );

        var instance = cut.Instance;

        // Assert
        Assert.Equal(500, instance.DelayMs);
    }

    [Fact]
    public void Debounce_WithStringValue_ShouldRenderInitially()
    {
        // Arrange & Act
        var cut = Render<Debounce<string>>(parameters => parameters
            .Add(p => p.Value, "initial value")
            .Add(p => p.DelayMs, 200)
            .AddChildContent("<div>Debounce content</div>")
        );

        // Assert
        Assert.Contains("Debounce content", cut.Markup);
        Assert.Equal("initial value", cut.Instance.Value);
    }
}
