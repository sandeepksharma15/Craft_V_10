using Craft.UiComponents.Enums;

namespace Craft.UiComponents.Tests.Enums;

public class AnimationDurationTests
{
    [Theory]
    [InlineData(AnimationDuration.None, 0)]
    [InlineData(AnimationDuration.ExtraFast, 100)]
    [InlineData(AnimationDuration.Fast, 200)]
    [InlineData(AnimationDuration.Normal, 300)]
    [InlineData(AnimationDuration.Slow, 500)]
    [InlineData(AnimationDuration.ExtraSlow, 700)]
    public void AnimationDuration_ShouldHaveCorrectMillisecondValues(AnimationDuration duration, int expectedMs)
    {
        // Arrange & Act
        var actualMs = (int)duration;

        // Assert
        Assert.Equal(expectedMs, actualMs);
    }

    [Fact]
    public void AnimationDuration_ShouldHaveSixValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<AnimationDuration>();

        // Assert
        Assert.Equal(6, values.Length);
    }
}
