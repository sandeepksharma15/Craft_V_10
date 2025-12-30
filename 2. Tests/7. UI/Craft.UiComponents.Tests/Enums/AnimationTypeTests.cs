using Craft.UiComponents.Enums;

namespace Craft.UiComponents.Tests.Enums;

public class AnimationTypeTests
{
    [Theory]
    [InlineData(AnimationType.None, 0)]
    [InlineData(AnimationType.Fade, 1)]
    [InlineData(AnimationType.Slide, 2)]
    [InlineData(AnimationType.Scale, 3)]
    [InlineData(AnimationType.Collapse, 4)]
    [InlineData(AnimationType.Bounce, 5)]
    [InlineData(AnimationType.Shake, 6)]
    [InlineData(AnimationType.Pulse, 7)]
    [InlineData(AnimationType.Flip, 8)]
    [InlineData(AnimationType.Rotate, 9)]
    public void AnimationType_ShouldHaveCorrectValues(AnimationType animationType, int expectedValue)
    {
        // Arrange & Act
        var actualValue = (int)animationType;

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void AnimationType_ShouldHaveTenValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<AnimationType>();

        // Assert
        Assert.Equal(10, values.Length);
    }
}
