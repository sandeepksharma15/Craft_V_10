using Craft.UiBuilders.Tests.Base;

namespace Craft.UiBuilders.Tests.Components;

public class HexInputTests : ComponentTestBase
{
    //[Fact]
    //public void Render_WithInitialHexValue_ShowsUppercaseBytePairs()
    //{
    //    // Arrange
    //    var cut = Context.Render<HexInput>(parameters => parameters
    //        .Add(component => component.Value, "a0b1c2"));

    //    // Act
    //    var renderedValue = cut.Find("input").GetAttribute("value");

    //    // Assert
    //    Assert.Equal("A0 B1 C2", renderedValue);
    //}

    //[Fact]
    //public void Input_WithInvalidAndLowercaseCharacters_NormalizesBeforeRerender()
    //{
    //    // Arrange
    //    var emittedValue = string.Empty;
    //    var cut = Context.Render<HexInput>(parameters => parameters
    //        .Add(component => component.Value, string.Empty)
    //        .Add(component => component.ValueChanged, EventCallback.Factory.Create<string>(this, value => emittedValue = value))
    //        .Add(component => component.MaxLength, 6));

    //    // Act
    //    cut.Find("input").Input("abz12cd34");

    //    // Assert
    //    Assert.Equal("AB12CD", emittedValue);
    //}

    //[Fact]
    //public void Input_WithConfiguredMaxLength_ShowsFormattedTruncatedValue()
    //{
    //    // Arrange
    //    var cut = Context.Render<HexInput>(parameters => parameters
    //        .Add(component => component.Value, string.Empty)
    //        .Add(component => component.MaxLength, 6));

    //    // Act
    //    cut.Find("input").Input("ab12cd34");
    //    var renderedValue = cut.Find("input").GetAttribute("value");

    //    // Assert
    //    Assert.Equal("AB 12 CD", renderedValue);
    //}
}
