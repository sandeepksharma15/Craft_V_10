using Craft.UiConponents.Enums;

namespace Craft.UiComponents.Tests.Enums;

public class ComponentVariantTests
{
    [Theory]
    [InlineData(ComponentVariant.Default, 0)]
    [InlineData(ComponentVariant.Primary, 1)]
    [InlineData(ComponentVariant.Secondary, 2)]
    [InlineData(ComponentVariant.Success, 3)]
    [InlineData(ComponentVariant.Warning, 4)]
    [InlineData(ComponentVariant.Danger, 5)]
    [InlineData(ComponentVariant.Info, 6)]
    [InlineData(ComponentVariant.Light, 7)]
    [InlineData(ComponentVariant.Dark, 8)]
    [InlineData(ComponentVariant.Outlined, 9)]
    [InlineData(ComponentVariant.Text, 10)]
    public void ComponentVariant_ShouldHaveCorrectValues(ComponentVariant variant, int expectedValue)
    {
        // Arrange & Act
        var actualValue = (int)variant;

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ComponentVariant_ShouldHaveElevenValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<ComponentVariant>();

        // Assert
        Assert.Equal(11, values.Length);
    }
}
