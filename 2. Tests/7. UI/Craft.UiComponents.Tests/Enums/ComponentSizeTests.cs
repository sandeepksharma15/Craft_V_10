using Craft.UiComponents.Enums;

namespace Craft.UiComponents.Tests.Enums;

public class ComponentSizeTests
{
    [Theory]
    [InlineData(ComponentSize.ExtraSmall, 0)]
    [InlineData(ComponentSize.Small, 1)]
    [InlineData(ComponentSize.Medium, 2)]
    [InlineData(ComponentSize.Large, 3)]
    [InlineData(ComponentSize.ExtraLarge, 4)]
    public void ComponentSize_ShouldHaveCorrectValues(ComponentSize size, int expectedValue)
    {
        // Arrange & Act
        var actualValue = (int)size;

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ComponentSize_ShouldHaveFiveValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<ComponentSize>();

        // Assert
        Assert.Equal(5, values.Length);
    }
}
