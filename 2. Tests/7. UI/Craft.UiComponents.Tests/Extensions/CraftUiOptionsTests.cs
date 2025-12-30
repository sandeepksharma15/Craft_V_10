using Craft.UiComponents;
using Craft.UiComponents.Enums;

namespace Craft.UiComponents.Tests.Extensions;

public class CraftUiOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new CraftUiOptions();

        // Assert
        Assert.Equal(Theme.System, options.DefaultTheme);
        Assert.False(options.EnableLogging);
        Assert.Equal(AnimationDuration.Normal, options.DefaultAnimationDuration);
        Assert.Equal("craft", options.CssPrefix);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new CraftUiOptions
        {
            // Act
            DefaultTheme = Theme.Dark,
            EnableLogging = true,
            DefaultAnimationDuration = AnimationDuration.Slow,
            CssPrefix = "custom-prefix"
        };

        // Assert
        Assert.Equal(Theme.Dark, options.DefaultTheme);
        Assert.True(options.EnableLogging);
        Assert.Equal(AnimationDuration.Slow, options.DefaultAnimationDuration);
        Assert.Equal("custom-prefix", options.CssPrefix);
    }
}
