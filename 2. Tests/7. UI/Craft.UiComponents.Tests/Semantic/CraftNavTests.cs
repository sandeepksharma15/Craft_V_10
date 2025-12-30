using Bunit;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftNavTests : BunitContext
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
    public void CraftNav_ShouldHaveRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal("navigation", nav.GetAttribute("role"));
    }

    [Fact]
    public void CraftNav_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var ariaLabel = "Main navigation";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.AddAttribute(1, "aria-label", ariaLabel);
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal(ariaLabel, nav.GetAttribute("aria-label"));
    }

    [Fact]
    public void CraftNav_ShouldRenderMultipleLinks()
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
                childBuilder.OpenElement(5, "a");
                childBuilder.AddAttribute(6, "href", "/about");
                childBuilder.AddContent(7, "About");
                childBuilder.CloseElement();
                childBuilder.OpenElement(8, "a");
                childBuilder.AddAttribute(9, "href", "/contact");
                childBuilder.AddContent(10, "Contact");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        var links = nav.QuerySelectorAll("a");
        Assert.Equal(3, links.Length);
    }

    [Fact]
    public void CraftNav_ShouldRenderWithNestedList()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "ul");
                childBuilder.OpenElement(3, "li");
                childBuilder.OpenElement(4, "a");
                childBuilder.AddAttribute(5, "href", "/");
                childBuilder.AddContent(6, "Home");
                childBuilder.CloseElement();
                childBuilder.CloseElement();
                childBuilder.OpenElement(7, "li");
                childBuilder.OpenElement(8, "a");
                childBuilder.AddAttribute(9, "href", "/about");
                childBuilder.AddContent(10, "About");
                childBuilder.CloseElement();
                childBuilder.CloseElement();
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        var ul = nav.QuerySelector("ul");
        var items = ul?.QuerySelectorAll("li");
        Assert.NotNull(ul);
        Assert.Equal(2, items?.Length);
    }

    [Fact]
    public void CraftNav_ShouldRenderWithAllAttributes()
    {
        // Arrange
        var expectedId = "primary-nav";
        var expectedClass = "top-navigation";
        var expectedStyle = "background: #333;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftNav>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "aria-label", "Primary");
            builder.AddAttribute(5, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(6, "a");
                childBuilder.AddAttribute(7, "href", "/");
                childBuilder.AddContent(8, "Home");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal(expectedId, nav.Id);
        Assert.Contains(expectedClass, nav.ClassName);
        Assert.Contains(expectedStyle, nav.GetAttribute("style"));
        Assert.NotNull(nav.QuerySelector("a"));
        Assert.Equal("Primary", nav.GetAttribute("aria-label"));
    }
}
