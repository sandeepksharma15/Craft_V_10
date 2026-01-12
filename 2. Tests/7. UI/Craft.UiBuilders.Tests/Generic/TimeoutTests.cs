using Bunit;
using Craft.UiBuilders.Tests.Base;
using Craft.UiComponents;
using TimeoutComponent = Craft.UiBuilders.Generic.Timeout;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Timeout component.
/// Tests auto-expiring content with timeout callback.
/// </summary>
public class TimeoutTests : ComponentTestBase
{
    [Fact]
    public void Timeout_Initially_ShouldRenderContent()
    {
        // Arrange & Act
        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 1000)
            .AddChildContent("<div>Timeout content</div>")
        );

        // Assert - content should be visible initially
        Assert.Contains("Timeout content", cut.Markup);
    }

    [Fact]
    public async Task Timeout_AfterExpiration_ShouldHideContent()
    {
        // Arrange
        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 100)
            .AddChildContent("<div>Expiring content</div>")
        );

        // Assert - initially visible
        Assert.Contains("Expiring content", cut.Markup);

        // Act - wait for expiration
        await Task.Delay(200);
        cut.Render();

        // Assert - should be hidden
        Assert.DoesNotContain("Expiring content", cut.Markup);
    }

    [Fact]
    public void Timeout_WithDefaultDuration_ShouldBe5000Ms()
    {
        // Arrange
        var component = new TimeoutComponent();

        // Assert
        Assert.Equal(5000, component.DurationMs);
    }

    [Fact]
    public void Timeout_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new TimeoutComponent();

        // Assert
        Assert.IsType<CraftComponent>(component, exactMatch: false);
    }

    [Fact]
    public async Task Timeout_OnExpiredCallback_ShouldBeInvoked()
    {
        // Arrange
        var callbackInvoked = false;

        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 100)
            .Add(p => p.OnExpired, () =>
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            })
            .AddChildContent("<div>Content</div>")
        );

        // Act - wait for expiration
        await Task.Delay(200);

        // Assert
        Assert.True(callbackInvoked);
    }

    [Fact]
    public async Task Timeout_OnExpiredCallback_ShouldBeOptional()
    {
        // Arrange
        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 100)
            .AddChildContent("<div>Content without callback</div>")
        );

        // Act - wait for expiration
        await Task.Delay(200);
        cut.Render();

        // Assert - should not throw, content should be hidden
        Assert.DoesNotContain("Content without callback", cut.Markup);
    }

    [Fact]
    public async Task Timeout_WithShortDuration_ShouldExpireQuickly()
    {
        // Arrange
        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 50)
            .AddChildContent("<div>Quick expire</div>")
        );

        // Assert - initially visible
        Assert.Contains("Quick expire", cut.Markup);

        // Act
        await Task.Delay(100);
        cut.Render();

        // Assert - should be expired
        Assert.DoesNotContain("Quick expire", cut.Markup);
    }

    [Fact]
    public async Task Timeout_WithLongDuration_ShouldRemainVisibleDuringPeriod()
    {
        // Arrange
        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 2000)
            .AddChildContent("<div>Long duration content</div>")
        );

        // Act - wait less than duration
        await Task.Delay(500);
        cut.Render();

        // Assert - should still be visible
        Assert.Contains("Long duration content", cut.Markup);
    }

    [Fact]
    public void Timeout_WithComplexContent_ShouldRenderInitially()
    {
        // Arrange & Act
        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 1000)
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "notification");
                builder.OpenElement(2, "h4");
                builder.AddContent(3, "Alert");
                builder.CloseElement();
                builder.OpenElement(4, "p");
                builder.AddContent(5, "This will disappear");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("notification", cut.Markup);
        Assert.Contains("Alert", cut.Markup);
        Assert.Contains("This will disappear", cut.Markup);
    }

    [Fact]
    public async Task Timeout_WhenDisposed_ShouldNotThrow()
    {
        // Arrange
        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 1000)
            .AddChildContent("<div>Content</div>")
        );

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
        {
            await Task.Delay(50);
            cut.Dispose();
            await Task.Delay(100);
        });

        Assert.Null(exception);
    }

    [Fact]
    public async Task Timeout_MultipleInstances_ShouldExpireIndependently()
    {
        // Arrange
        var callback1Invoked = false;
        var callback2Invoked = false;

        var cut1 = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 100)
            .Add(p => p.OnExpired, () =>
            {
                callback1Invoked = true;
                return Task.CompletedTask;
            })
            .AddChildContent("<div>Content 1</div>")
        );

        var cut2 = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 200)
            .Add(p => p.OnExpired, () =>
            {
                callback2Invoked = true;
                return Task.CompletedTask;
            })
            .AddChildContent("<div>Content 2</div>")
        );

        // Act - wait for first to expire
        await Task.Delay(150);

        // Assert - first should expire, second should not
        Assert.True(callback1Invoked);
        Assert.False(callback2Invoked);

        // Act - wait for second to expire
        await Task.Delay(100);

        // Assert - both should be expired
        Assert.True(callback1Invoked);
        Assert.True(callback2Invoked);
    }

    [Fact]
    public void Timeout_WithNoChildContent_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<TimeoutComponent>(parameters => parameters
                .Add(p => p.DurationMs, 1000)
            )
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task Timeout_OnExpiredCallback_ShouldBeInvokedOnce()
    {
        // Arrange
        var callbackCount = 0;

        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 100)
            .Add(p => p.OnExpired, () =>
            {
                callbackCount++;
                return Task.CompletedTask;
            })
            .AddChildContent("<div>Content</div>")
        );

        // Act - wait well past expiration
        await Task.Delay(300);

        // Assert - callback should only be invoked once
        Assert.Equal(1, callbackCount);
    }

    [Fact]
    public async Task Timeout_WithZeroDuration_ShouldExpireImmediately()
    {
        // Arrange
        var callbackInvoked = false;

        // Act
        var cut = Render<TimeoutComponent>(parameters => parameters
            .Add(p => p.DurationMs, 0)
            .Add(p => p.OnExpired, () =>
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            })
            .AddChildContent("<div>Instant timeout</div>")
        );

        // Small delay to allow timer to fire
        await Task.Delay(50);

        // Assert - content should be expired and callback invoked
        Assert.DoesNotContain("Instant timeout", cut.Markup);
        Assert.True(callbackInvoked);
    }
}
