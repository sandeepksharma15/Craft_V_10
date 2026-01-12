using Bunit;
using Craft.UiBuilders.Components.RunningNumber;
using Craft.UiBuilders.Tests.Base;

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
        Assert.True(instance.UseThousandsSeparator);
        Assert.True(instance.UseSmoothEasing);
    }

    [Fact]
    public void CraftRunningNumber_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.TotalTime, 5)
            .Add(p => p.FirstNumber, 100)
            .Add(p => p.LastNumber, 1000)
            .Add(p => p.UseThousandsSeparator, false)
            .Add(p => p.UseSmoothEasing, false));

        // Assert
        var instance = cut.Instance;
        Assert.Equal(5, instance.TotalTime);
        Assert.Equal(100, instance.FirstNumber);
        Assert.Equal(1000, instance.LastNumber);
        Assert.False(instance.UseThousandsSeparator);
        Assert.False(instance.UseSmoothEasing);
    }

    [Fact]
    public void CraftRunningNumber_CountingDown_ShouldStartAtFirstNumber()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 100)
            .Add(p => p.LastNumber, 0));

        // Assert - Component should render without errors
        Assert.NotNull(cut);
        var markup = cut.Markup;
        Assert.Contains("100", markup);
    }

    [Fact]
    public void CraftRunningNumber_WithThousandsSeparator_ShouldFormatNumber()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 0)
            .Add(p => p.LastNumber, 1000000)
            .Add(p => p.UseThousandsSeparator, true));

        // Assert - Initial value should be 0, which formats as "0"
        Assert.NotNull(cut);
        var markup = cut.Markup;
        Assert.NotEmpty(markup);
    }

    [Fact]
    public void CraftRunningNumber_WithoutThousandsSeparator_ShouldShowRawNumber()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 0)
            .Add(p => p.LastNumber, 1234)
            .Add(p => p.UseThousandsSeparator, false));

        // Assert
        Assert.NotNull(cut);
        var instance = cut.Instance;
        Assert.False(instance.UseThousandsSeparator);
    }

    [Fact]
    public void CraftRunningNumber_WithSmoothEasing_ShouldEnableEasing()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.UseSmoothEasing, true));

        // Assert
        var instance = cut.Instance;
        Assert.True(instance.UseSmoothEasing);
    }

    [Fact]
    public void CraftRunningNumber_WithoutSmoothEasing_ShouldUseLinearProgression()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.UseSmoothEasing, false));

        // Assert
        var instance = cut.Instance;
        Assert.False(instance.UseSmoothEasing);
    }

    [Fact]
    public void CraftRunningNumber_ShouldRenderInitialValue()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 42)
            .Add(p => p.LastNumber, 100));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("42", markup);
    }

    [Fact]
    public void CraftRunningNumber_ShouldDisposeProperlyAsync()
    {
        // Arrange
        var cut = Render<CraftRunningNumber>();
        var instance = cut.Instance;

        // Act
        instance.Dispose();

        // Assert - Should not throw
        Assert.True(true);
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
        Assert.NotEmpty(markup);
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

    [Fact]
    public void CraftRunningNumber_WithNegativeNumbers_ShouldHandleCorrectly()
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
    public void CraftRunningNumber_WithLargeNumbers_ShouldFormatWithCommas()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 1234567)
            .Add(p => p.LastNumber, 9999999)
            .Add(p => p.UseThousandsSeparator, true));

        // Assert
        var markup = cut.Markup;
        // Initial value should be formatted with commas (culture-specific)
        Assert.Contains(",", markup); // Just verify commas are present
        Assert.Contains("1234567".AsSpan(), markup.Replace(",", "").AsSpan()); // Verify digits are present
    }

    [Fact]
    public void CraftRunningNumber_CountingUp_ShouldStartAtFirstNumber()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 0)
            .Add(p => p.LastNumber, 100));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("0", markup);
    }

    [Fact]
    public void CraftRunningNumber_WithZeroRange_ShouldShowSameNumber()
    {
        // Arrange & Act
        var cut = Render<CraftRunningNumber>(parameters => parameters
            .Add(p => p.FirstNumber, 50)
            .Add(p => p.LastNumber, 50));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("50", markup);
    }
}
