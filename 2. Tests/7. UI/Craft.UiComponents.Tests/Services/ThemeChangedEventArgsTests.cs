using Craft.UiComponents.Enums;
using Craft.UiComponents.Services;

namespace Craft.UiComponents.Tests.Services;

public class ThemeChangedEventArgsTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange
        var previousTheme = Theme.Light;
        var newTheme = Theme.Dark;

        // Act
        var eventArgs = new ThemeChangedEventArgs(previousTheme, newTheme);

        // Assert
        Assert.Equal(previousTheme, eventArgs.PreviousTheme);
        Assert.Equal(newTheme, eventArgs.NewTheme);
    }

    [Theory]
    [InlineData(Theme.Light, Theme.Dark)]
    [InlineData(Theme.Dark, Theme.Light)]
    [InlineData(Theme.System, Theme.Dark)]
    [InlineData(Theme.Light, Theme.System)]
    public void Constructor_ShouldHandleAllThemeCombinations(Theme previous, Theme newTheme)
    {
        // Arrange & Act
        var eventArgs = new ThemeChangedEventArgs(previous, newTheme);

        // Assert
        Assert.Equal(previous, eventArgs.PreviousTheme);
        Assert.Equal(newTheme, eventArgs.NewTheme);
    }
}
