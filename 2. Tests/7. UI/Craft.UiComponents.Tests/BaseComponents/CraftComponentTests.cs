using Bunit;
using Craft.UiComponents;
using Craft.UiComponents.Enums;
using Craft.UiComponents.Services;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Craft.UiComponents.Tests.BaseComponents;

/// <summary>
/// Tests for CraftComponent base class functionality.
/// Since CraftComponent is abstract, we use a concrete test implementation.
/// </summary>
public class CraftComponentTests : ComponentTestBase
{
    [Fact]
    public void CraftComponent_ShouldGenerateUniqueId()
    {
        // Act
        var cut1 = Render<TestCraftComponent>();
        var cut2 = Render<TestCraftComponent>();

        // Assert
        var id1 = cut1.Instance.Id;
        var id2 = cut2.Instance.Id;

        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
        Assert.StartsWith("craft-", id1);
    }

    [Fact]
    public void CraftComponent_ShouldAcceptCustomId()
    {
        // Arrange
        var expectedId = "custom-test-id";

        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Id, expectedId));

        // Assert
        Assert.Equal(expectedId, cut.Instance.Id);
    }

    [Fact]
    public void CraftComponent_ShouldApplyCustomClass()
    {
        // Arrange
        var customClass = "my-custom-class";

        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Class, customClass));

        // Assert
        Assert.Contains(customClass, cut.Markup);
    }

    [Fact]
    public void CraftComponent_ShouldApplyCustomStyle()
    {
        // Arrange
        var customStyle = "color: red; font-size: 16px;";

        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Style, customStyle));

        // Assert
        Assert.Contains(customStyle, cut.Markup);
    }

    [Fact]
    public void CraftComponent_ShouldRenderChildContent()
    {
        // Arrange
        var childText = "Test Child Content";

        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .AddChildContent(childText));

        // Assert
        Assert.Contains(childText, cut.Markup);
    }

    [Fact]
    public void CraftComponent_ShouldBeVisibleByDefault()
    {
        // Act
        var cut = Render<TestCraftComponent>();

        // Assert
        Assert.True(cut.Instance.Visible);
    }

    [Fact]
    public void CraftComponent_ShouldApplyHiddenClassWhenInvisible()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Visible, false));

        // Assert
        Assert.Contains("craft-hidden", cut.Markup);
    }

    [Fact]
    public void CraftComponent_ShouldApplyDisabledClass()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Assert
        Assert.Contains("craft-disabled", cut.Markup);
    }

    [Fact]
    public void CraftComponent_ShouldApplySizeCssClass()
    {
        // Arrange
        var testCases = new[]
        {
            (ComponentSize.ExtraSmall, "craft-size-xs"),
            (ComponentSize.Small, "craft-size-sm"),
            (ComponentSize.Large, "craft-size-lg"),
            (ComponentSize.ExtraLarge, "craft-size-xl")
        };

        foreach (var (size, expectedClass) in testCases)
        {
            // Act
            var cut = Render<TestCraftComponent>(parameters => parameters
                .Add(p => p.Size, size));

            // Assert
            Assert.Contains(expectedClass, cut.Markup);
        }
    }

    [Fact]
    public void CraftComponent_MediumSize_ShouldNotAddSizeClass()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Size, ComponentSize.Medium));

        // Assert
        Assert.DoesNotContain("craft-size-", cut.Markup);
    }

    [Fact]
    public void CraftComponent_ShouldApplyVariantCssClass()
    {
        // Arrange
        var testCases = new[]
        {
            (ComponentVariant.Primary, "craft-variant-primary"),
            (ComponentVariant.Secondary, "craft-variant-secondary"),
            (ComponentVariant.Success, "craft-variant-success"),
            (ComponentVariant.Warning, "craft-variant-warning"),
            (ComponentVariant.Danger, "craft-variant-danger"),
            (ComponentVariant.Info, "craft-variant-info"),
            (ComponentVariant.Light, "craft-variant-light"),
            (ComponentVariant.Dark, "craft-variant-dark")
        };

        foreach (var (variant, expectedClass) in testCases)
        {
            // Act
            var cut = Render<TestCraftComponent>(parameters => parameters
                .Add(p => p.Variant, variant));

            // Assert
            Assert.Contains(expectedClass, cut.Markup);
        }
    }

    [Fact]
    public void CraftComponent_DefaultVariant_ShouldNotAddVariantClass()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Variant, ComponentVariant.Default));

        // Assert
        Assert.DoesNotContain("craft-variant-", cut.Markup);
    }

    [Fact]
    public void CraftComponent_ShouldApplyAnimationCssClass()
    {
        // Arrange
        var testCases = new[]
        {
            (AnimationType.Fade, "craft-animate-fade"),
            (AnimationType.Slide, "craft-animate-slide"),
            (AnimationType.Scale, "craft-animate-scale"),
            (AnimationType.Bounce, "craft-animate-bounce")
        };

        foreach (var (animation, expectedClass) in testCases)
        {
            // Act
            var cut = Render<TestCraftComponent>(parameters => parameters
                .Add(p => p.Animation, animation));

            // Assert
            Assert.Contains(expectedClass, cut.Markup);
        }
    }

    [Fact]
    public void CraftComponent_ShouldHandleClickEvent()
    {
        // Arrange
        var clicked = false;

        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicked = true)));

        cut.Find("div").Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void CraftComponent_ShouldNotFireClickWhenDisabled()
    {
        // Arrange
        var clicked = false;

        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicked = true)));

        cut.Find("div").Click();

        // Assert
        Assert.False(clicked);
    }

    [Fact]
    public void CraftComponent_ShouldApplyUserAttributes()
    {
        // Arrange & Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<TestCraftComponent>(0);
            builder.AddAttribute(1, "data-test", "test-value");
            builder.AddAttribute(2, "aria-label", "test-label");
            builder.CloseComponent();
        });

        // Assert
        var element = cut.Find("div");
        Assert.Equal("test-value", element.GetAttribute("data-test"));
        Assert.Equal("test-label", element.GetAttribute("aria-label"));
    }

    [Fact]
    public void CraftComponent_ShouldCombineMultipleCssClasses()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Class, "custom-class")
            .Add(p => p.Size, ComponentSize.Large)
            .Add(p => p.Variant, ComponentVariant.Primary)
            .Add(p => p.Disabled, true));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("custom-class", markup);
        Assert.Contains("craft-size-lg", markup);
        Assert.Contains("craft-variant-primary", markup);
        Assert.Contains("craft-disabled", markup);
    }

    [Fact]
    public void CraftComponent_ShouldHandleAnimationDuration()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Animation, AnimationType.Fade)
            .Add(p => p.AnimationDuration, AnimationDuration.Slow));

        // Assert
        Assert.Contains("--craft-animation-duration", cut.Markup);
    }

    [Fact]
    public void CraftComponent_ShouldHandleCustomAnimationDuration()
    {
        // Arrange
        var customDuration = 500;

        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Animation, AnimationType.Fade)
            .Add(p => p.CustomAnimationDurationMs, customDuration));

        // Assert
        var style = cut.Find("div").GetAttribute("style");
        Assert.Contains("500ms", style ?? "");
    }

    [Fact]
    public async Task CraftComponent_ShouldCallRefresh()
    {
        // Arrange
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Id, "test-refresh"));
        
        var initialRenderCount = cut.RenderCount;

        // Act - Use InvokeAsync to ensure we're on the correct dispatcher thread
        await cut.InvokeAsync(() => cut.Instance.Refresh());

        // Wait for re-render
        cut.WaitForState(() => cut.RenderCount > initialRenderCount, timeout: TimeSpan.FromSeconds(1));

        // Assert
        Assert.True(cut.RenderCount > initialRenderCount);
    }

    [Fact]
    public async Task CraftComponent_ShouldCallRefreshAsync()
    {
        // Arrange
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Id, "test-refresh-async"));
        
        var initialRenderCount = cut.RenderCount;

        // Act
        await cut.InvokeAsync(async () => await cut.Instance.RefreshAsync());

        // Wait for re-render
        cut.WaitForState(() => cut.RenderCount > initialRenderCount, timeout: TimeSpan.FromSeconds(1));

        // Assert
        Assert.True(cut.RenderCount > initialRenderCount);
    }
}

/// <summary>
/// Test implementation of ThemedCraftComponent for testing purposes.
/// </summary>
internal class TestCraftComponent : ThemedCraftComponent
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", BuildCssClass());
        
        var style = BuildStyle();
        if (!string.IsNullOrEmpty(style))
        {
            builder.AddAttribute(3, "style", style);
        }

        builder.AddMultipleAttributes(4, UserAttributes);

        if (OnClick.HasDelegate)
        {
            builder.AddAttribute(5, "onclick", EventCallback.Factory.Create<MouseEventArgs>(
                this, async args => await HandleClickAsync(args)));
        }

        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}
