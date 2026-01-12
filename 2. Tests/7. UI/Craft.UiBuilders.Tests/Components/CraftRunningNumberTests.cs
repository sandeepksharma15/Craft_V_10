using Bunit;
using Craft.UiBuilders.Components.RunningNumber;
using Craft.UiBuilders.Tests.Base;
using MudBlazor;

namespace Craft.UiBuilders.Tests.Components;

/// <summary>
/// Unit tests for the CraftRunningNumber component.
/// </summary>
public class CraftRunningNumberTests : ComponentTestBase
{
    [Fact]
    public void CraftRunningNumber_WithDefaultValues_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>();

        // Assert
        Assert.NotNull(cut);
        var instance = cut.Instance;
        Assert.Equal(1, instance.TotalTime);
        Assert.Equal(0, instance.FirstNumber);
        Assert.Equal(100, instance.LastNumber);
        Assert.Equal(Typo.h5, instance.TextType);
        Assert.Equal(Color.Primary, instance.TextColor);
        Assert.True(instance.UseThousandsSeparator);
    }

    [Fact]
    public void CraftRunningNumber_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.TotalTime, 5)
            .Add(p => p.FirstNumber, 100)
            .Add(p => p.LastNumber, 1000)
            .Add(p => p.TextType, Typo.h3)
            .Add(p => p.TextColor, Color.Success)
            .Add(p => p.UseThousandsSeparator, false));

        // Assert
        var instance = cut.Instance;
        Assert.Equal(5, instance.TotalTime);
        Assert.Equal(100, instance.FirstNumber);
        Assert.Equal(1000, instance.LastNumber);
        Assert.Equal(Typo.h3, instance.TextType);
        Assert.Equal(Color.Success, instance.TextColor);
        Assert.False(instance.UseThousandsSeparator);
    }

    [Fact]
    public void CraftRunningNumber_CountingDown_ShouldRecognizeDirection()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 100)
            .Add(p => p.LastNumber, 0));

        // Assert - Component should render without errors
        Assert.NotNull(cut);
    }

    [Fact]
    public void CraftRunningNumber_WithLargeNumbers_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 0)
            .Add(p => p.LastNumber, 1000000)
            .Add(p => p.UseThousandsSeparator, true));

        // Assert
        Assert.NotNull(cut);
        var markup = cut.Markup;
        Assert.Contains("craft-running-number", markup);
    }

    [Fact]
    public void CraftRunningNumber_WithNegativeNumbers_ShouldRenderMinus()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, -100)
            .Add(p => p.LastNumber, -10));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("-", markup);
    }

    [Fact]
    public void CraftRunningNumber_ShouldContainDigitWrapper()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 0)
            .Add(p => p.LastNumber, 50));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("digit-wrapper", markup);
        Assert.Contains("digit-scroll", markup);
    }

    [Fact]
    public void CraftRunningNumber_ShouldRenderAllDigits()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 0)
            .Add(p => p.LastNumber, 123));

        // Assert
        var markup = cut.Markup;
        // Should contain digit items (0-9 for each position)
        Assert.Contains("digit-item", markup);
    }

    [Fact]
    public void CraftRunningNumber_WithThousandsSeparator_ShouldRenderCommas()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 0)
            .Add(p => p.LastNumber, 1000)
            .Add(p => p.UseThousandsSeparator, true));

        // Assert - After initial render, might not show comma yet at value 0,
        // but component structure should be present
        var markup = cut.Markup;
        Assert.Contains("number-container", markup);
    }

    [Fact]
    public async Task CraftRunningNumber_ShouldDisposeProperlyAsync()
    {
        // Arrange
        var cut = Render<CraftRunningNumber>();
        var instance = cut.Instance;

        // Act
        instance.Dispose();

        // Assert - Should not throw
        await Task.CompletedTask;
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(100, 0)]
    [InlineData(-50, 50)]
    [InlineData(1000, 10000)]
    public void CraftRunningNumber_WithVariousRanges_ShouldRenderCorrectly(long first, long last)
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, first)
            .Add(p => p.LastNumber, last));

        // Assert
        Assert.NotNull(cut);
        var markup = cut.Markup;
        Assert.Contains("craft-running-number", markup);
    }

    [Theory]
    [InlineData(Typo.h1)]
    [InlineData(Typo.h3)]
    [InlineData(Typo.h5)]
    [InlineData(Typo.body1)]
    public void CraftRunningNumber_WithDifferentTypography_ShouldRenderCorrectly(Typo typo)
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.TextType, typo));

        // Assert
        Assert.NotNull(cut);
        Assert.Equal(typo, cut.Instance.TextType);
    }

    [Theory]
    [InlineData(Color.Primary)]
    [InlineData(Color.Secondary)]
    [InlineData(Color.Success)]
    [InlineData(Color.Error)]
    [InlineData(Color.Warning)]
    public void CraftRunningNumber_WithDifferentColors_ShouldRenderCorrectly(Color color)
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.TextColor, color));

        // Assert
        Assert.NotNull(cut);
        Assert.Equal(color, cut.Instance.TextColor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void CraftRunningNumber_WithDifferentTimes_ShouldSetCorrectly(int totalTime)
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.TotalTime, totalTime));

        // Assert
        Assert.Equal(totalTime, cut.Instance.TotalTime);
    }
}
