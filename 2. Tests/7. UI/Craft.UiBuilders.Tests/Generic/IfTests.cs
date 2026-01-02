using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the If component.
/// Tests conditional rendering with both True and False render fragments.
/// </summary>
public class IfTests : ComponentTestBase
{
    [Fact]
    public void If_WhenConditionIsTrue_ShouldRenderTrueContent()
    {
        // Arrange & Act
        var cut = Render<If>(parameters => parameters
            .Add(p => p.Condition, true)
            .Add(p => p.True, builder => builder.AddMarkupContent(0, "<div>True Content</div>"))
            .Add(p => p.False, builder => builder.AddMarkupContent(0, "<div>False Content</div>"))
        );

        // Assert
        Assert.Contains("True Content", cut.Markup);
        Assert.DoesNotContain("False Content", cut.Markup);
    }

    [Fact]
    public void If_WhenConditionIsFalse_ShouldRenderFalseContent()
    {
        // Arrange & Act
        var cut = Render<If>(parameters => parameters
            .Add(p => p.Condition, false)
            .Add(p => p.True, builder => builder.AddMarkupContent(0, "<div>True Content</div>"))
            .Add(p => p.False, builder => builder.AddMarkupContent(0, "<div>False Content</div>"))
        );

        // Assert
        Assert.DoesNotContain("True Content", cut.Markup);
        Assert.Contains("False Content", cut.Markup);
    }

    [Fact]
    public void If_WhenConditionIsTrueAndFalseIsNull_ShouldRenderOnlyTrueContent()
    {
        // Arrange & Act
        var cut = Render<If>(parameters => parameters
            .Add(p => p.Condition, true)
            .Add(p => p.True, builder => builder.AddMarkupContent(0, "<div>True Content</div>"))
        );

        // Assert
        Assert.Contains("True Content", cut.Markup);
    }

    [Fact]
    public void If_WhenConditionIsFalseAndFalseIsNull_ShouldRenderNothing()
    {
        // Arrange & Act
        var cut = Render<If>(parameters => parameters
            .Add(p => p.Condition, false)
            .Add(p => p.True, builder => builder.AddMarkupContent(0, "<div>True Content</div>"))
        );

        // Assert
        Assert.DoesNotContain("True Content", cut.Markup);
    }

    [Fact]
    public void If_WhenConditionIsFalseAndTrueIsNull_ShouldRenderFalseContent()
    {
        // Arrange & Act
        var cut = Render<If>(parameters => parameters
            .Add(p => p.Condition, false)
            .Add(p => p.False, builder => builder.AddMarkupContent(0, "<div>False Content</div>"))
        );

        // Assert
        Assert.Contains("False Content", cut.Markup);
    }

    [Fact]
    public void If_WhenBothFragmentsAreNull_ShouldNotThrowException()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<If>(parameters => parameters
                .Add(p => p.Condition, true)
            )
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void If_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new If();

        // Assert
        Assert.IsType<Craft.UiComponents.CraftComponent>(component, exactMatch: false);
    }

    [Fact]
    public void If_WithComplexTrueContent_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<If>(parameters => parameters
            .Add(p => p.Condition, true)
            .Add(p => p.True, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "container");
                builder.OpenElement(2, "h1");
                builder.AddContent(3, "Welcome");
                builder.CloseElement();
                builder.OpenElement(4, "p");
                builder.AddContent(5, "This is a test");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("container", cut.Markup);
        Assert.Contains("Welcome", cut.Markup);
        Assert.Contains("This is a test", cut.Markup);
    }

    [Fact]
    public void If_WithComplexFalseContent_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<If>(parameters => parameters
            .Add(p => p.Condition, false)
            .Add(p => p.False, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "error");
                builder.OpenElement(2, "span");
                builder.AddContent(3, "Error occurred");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("error", cut.Markup);
        Assert.Contains("Error occurred", cut.Markup);
    }

    [Theory]
    [InlineData(true, "True Content", "False Content", true)]
    [InlineData(false, "True Content", "False Content", false)]
    public void If_WithVariousConditions_ShouldRenderCorrectly(bool condition, string trueContent, string falseContent, bool shouldShowTrue)
    {
        // Arrange & Act
        var cut = Render<If>(parameters => parameters
            .Add(p => p.Condition, condition)
            .Add(p => p.True, builder => builder.AddMarkupContent(0, $"<div>{trueContent}</div>"))
            .Add(p => p.False, builder => builder.AddMarkupContent(0, $"<div>{falseContent}</div>"))
        );

        // Assert
        if (shouldShowTrue)
        {
            Assert.Contains(trueContent, cut.Markup);
            Assert.DoesNotContain(falseContent, cut.Markup);
        }
        else
        {
            Assert.DoesNotContain(trueContent, cut.Markup);
            Assert.Contains(falseContent, cut.Markup);
        }
    }
}
