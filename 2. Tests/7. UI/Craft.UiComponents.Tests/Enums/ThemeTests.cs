using Craft.UiConponents.Enums;

namespace Craft.UiComponents.Tests.Enums;

public class ThemeTests
{
    [Theory]
    [InlineData(Theme.Light, 0)]
    [InlineData(Theme.Dark, 1)]
    [InlineData(Theme.System, 2)]
    public void Theme_ShouldHaveCorrectValues(Theme theme, int expectedValue)
    {
        // Arrange & Act
        var actualValue = (int)theme;

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void Theme_ShouldHaveThreeValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<Theme>();

        // Assert
        Assert.Equal(3, values.Length);
    }
}
