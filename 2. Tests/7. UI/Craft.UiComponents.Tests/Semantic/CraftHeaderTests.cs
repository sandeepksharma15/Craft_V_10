using Bunit;
using Craft.UiComponents.Tests.Base;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftHeaderTests : ComponentTestBase
{
    [Fact]
    public void CraftHeader_ShouldRenderHeaderElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        Assert.NotNull(header);
    }

    [Fact]
    public void CraftHeader_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "page-header";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        Assert.Equal(expectedId, header.Id);
    }

    [Fact]
    public void CraftHeader_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "site-header";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        Assert.Contains(expectedClass, header.ClassName);
    }

    [Fact]
    public void CraftHeader_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "background-color: blue;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        Assert.Contains(expectedStyle, header.GetAttribute("style"));
    }

    [Fact]
    public void CraftHeader_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Website Header";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        Assert.Contains(expectedContent, header.TextContent);
    }

    [Fact]
    public void CraftHeader_ShouldHaveRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        Assert.Equal("heading", header.GetAttribute("role"));
    }

    [Fact]
    public void CraftHeader_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var ariaLabel = "Main header";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.AddAttribute(1, "aria-label", ariaLabel);
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        Assert.Equal(ariaLabel, header.GetAttribute("aria-label"));
    }

    [Fact]
    public void CraftHeader_ShouldRenderWithNavigationComponent()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<CraftNav>(2);
                childBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(navBuilder =>
                {
                    navBuilder.OpenElement(4, "a");
                    navBuilder.AddAttribute(5, "href", "#");
                    navBuilder.AddContent(6, "Home");
                    navBuilder.CloseElement();
                }));
                childBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        var nav = header.QuerySelector("nav");
        Assert.NotNull(nav);
    }

    [Fact]
    public void CraftHeader_ShouldRenderComplexStructure()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "div");
                childBuilder.AddAttribute(3, "class", "logo");
                childBuilder.AddContent(4, "Logo");
                childBuilder.CloseElement();
                childBuilder.OpenElement(5, "nav");
                childBuilder.AddContent(6, "Menu");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        var logo = header.QuerySelector("div.logo");
        var nav = header.QuerySelector("nav");
        Assert.NotNull(logo);
        Assert.NotNull(nav);
    }

    [Fact]
    public void CraftHeader_ShouldRenderWithAllAttributes()
    {
        // Arrange
        var expectedId = "main-header";
        var expectedClass = "fixed-header";
        var expectedStyle = "position: fixed;";
        var expectedContent = "Application Header";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeader>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "data-header", "main");
            builder.AddAttribute(5, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(6, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var header = cut.Find("header");
        Assert.Equal(expectedId, header.Id);
        Assert.Contains(expectedClass, header.ClassName);
        Assert.Contains(expectedStyle, header.GetAttribute("style"));
        Assert.Contains(expectedContent, header.TextContent);
        Assert.Equal("main", header.GetAttribute("data-header"));
    }
}
