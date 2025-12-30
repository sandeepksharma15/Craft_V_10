using Bunit;
using Craft.UiComponents.Tests.BaseComponents;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.UiComponents.Tests.Base;

/// <summary>
/// Tests to verify CraftComponent works without IThemeService registered.
/// This demonstrates that theme service is truly optional.
/// </summary>
public class CraftComponentWithoutServicesTests : BunitContext
{
    public CraftComponentWithoutServicesTests()
    {
        // Intentionally NOT registering IThemeService or ILogger
        // Only configure minimal JSInterop
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void CraftComponent_ShouldWorkWithoutThemeService()
    {
        // Act - Component should render successfully without IThemeService
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Id, "test-no-theme")
            .Add(p => p.Class, "test-class"));

        // Assert
        var element = cut.Find("div");
        Assert.Equal("test-no-theme", element.Id);
        Assert.Contains("test-class", element.ClassName);
    }

    [Fact]
    public void CraftComponent_ShouldRenderWithoutLogger()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<span>Content</span>")));

        // Assert
        var element = cut.Find("div");
        var span = element.QuerySelector("span");
        Assert.NotNull(span);
        Assert.Equal("Content", span.TextContent);
    }

    [Fact]
    public void CraftComponent_ShouldHandleClickWithoutServices()
    {
        // Arrange
        var clicked = false;

        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.OnClick, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                this, _ => clicked = true)));

        cut.Find("div").Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void CraftComponent_ShouldApplyStylesWithoutThemeService()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Style, "color: red;")
            .Add(p => p.Variant, Craft.UiComponents.Enums.ComponentVariant.Primary));

        // Assert
        var element = cut.Find("div");
        Assert.Contains("color: red", element.GetAttribute("style") ?? "");
        Assert.Contains("craft-variant-primary", element.ClassName);
    }

    [Fact]
    public void CraftComponent_ShouldHandleDisabledWithoutServices()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Assert
        Assert.Contains("craft-disabled", cut.Markup);
    }

    [Fact]
    public void CraftComponent_ShouldHandleVisibilityWithoutServices()
    {
        // Act
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Visible, false));

        // Assert
        Assert.Contains("craft-hidden", cut.Markup);
    }

    [Fact]
    public async Task CraftComponent_ShouldRefreshWithoutServices()
    {
        // Arrange
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Id, "test-refresh"));

        var initialRenderCount = cut.RenderCount;

        // Act
        await cut.InvokeAsync(() => cut.Instance.Refresh());

        // Assert
        Assert.True(cut.RenderCount > initialRenderCount);
    }

    [Fact]
    public async Task CraftComponent_ShouldDisposeWithoutServices()
    {
        // Arrange
        var cut = Render<TestCraftComponent>(parameters => parameters
            .Add(p => p.Id, "test-dispose"));

        // Act & Assert - Should not throw
        cut.Dispose();
        await Task.CompletedTask;
    }
}
