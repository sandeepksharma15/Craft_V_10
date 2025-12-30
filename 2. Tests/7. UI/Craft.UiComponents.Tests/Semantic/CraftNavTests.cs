using Bunit;
using Craft.UiComponents.Tests.Base;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftNavTests : ComponentTestBase
{
    [Fact]
    public void CraftNav_ShouldRenderNavElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        Assert.NotNull(nav);
    }

    [Fact]
    public void CraftNav_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "main-nav";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal(expectedId, nav.Id);
    }

    [Fact]
    public void CraftNav_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "navbar";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        Assert.Contains(expectedClass, nav.ClassName);
    }

    [Fact]
    public void CraftNav_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "display: flex;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        Assert.Contains(expectedStyle, nav.GetAttribute("style"));
    }

    [Fact]
    public void CraftNav_ShouldRenderChildContent()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "a");
                childBuilder.AddAttribute(3, "href", "/");
                childBuilder.AddContent(4, "Home");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        var link = nav.QuerySelector("a");
        Assert.NotNull(link);
        Assert.Equal("Home", link.TextContent);
    }

    [Fact]
    public void CraftNav_ShouldNotHaveRedundantRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.CloseComponent();
        });

        // Assert - nav element should not have role attribute (it's implicit)
        var nav = cut.Find("nav");
        Assert.Null(nav.GetAttribute("role"));
    }
}
