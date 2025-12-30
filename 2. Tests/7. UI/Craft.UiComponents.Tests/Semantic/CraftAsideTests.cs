using Bunit;
using Craft.UiComponents.Tests.Base;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftAsideTests : ComponentTestBase
{
    [Fact]
    public void CraftAside_ShouldRenderAsideElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAside>(0);
            builder.CloseComponent();
        });

        // Assert
        var aside = cut.Find("aside");
        Assert.NotNull(aside);
    }

    [Fact]
    public void CraftAside_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "test-aside";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAside>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var aside = cut.Find("aside");
        Assert.Equal(expectedId, aside.Id);
    }

    [Fact]
    public void CraftAside_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "sidebar-class";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAside>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var aside = cut.Find("aside");
        Assert.Contains(expectedClass, aside.ClassName);
    }

    [Fact]
    public void CraftAside_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "width: 250px;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAside>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var aside = cut.Find("aside");
        Assert.Contains(expectedStyle, aside.GetAttribute("style"));
    }

    [Fact]
    public void CraftAside_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Sidebar Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAside>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var aside = cut.Find("aside");
        Assert.Contains(expectedContent, aside.TextContent);
    }

    [Fact]
    public void CraftAside_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataAttribute = "aside-data";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAside>(0);
            builder.AddAttribute(1, "data-sidebar", dataAttribute);
            builder.CloseComponent();
        });

        // Assert
        var aside = cut.Find("aside");
        Assert.Equal(dataAttribute, aside.GetAttribute("data-sidebar"));
    }

    [Fact]
    public void CraftAside_ShouldRenderWithMultipleAttributes()
    {
        // Arrange
        var expectedId = "sidebar";
        var expectedClass = "sidebar-panel";
        var expectedStyle = "background: gray;";
        var expectedContent = "Aside Panel";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAside>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(5, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var aside = cut.Find("aside");
        Assert.Equal(expectedId, aside.Id);
        Assert.Contains(expectedClass, aside.ClassName);
        Assert.Contains(expectedStyle, aside.GetAttribute("style"));
        Assert.Contains(expectedContent, aside.TextContent);
    }

    [Fact]
    public void CraftAside_ShouldRenderNestedComponents()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAside>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<CraftNav>(2);
                childBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(navChildBuilder =>
                {
                    navChildBuilder.AddContent(4, "Navigation");
                }));
                childBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        var aside = cut.Find("aside");
        var nav = aside.QuerySelector("nav");
        Assert.NotNull(nav);
        Assert.Contains("Navigation", nav.TextContent);
    }

    [Fact]
    public void CraftAside_ShouldRenderMultipleChildElements()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAside>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "h3");
                childBuilder.AddContent(3, "Title");
                childBuilder.CloseElement();
                childBuilder.OpenElement(4, "p");
                childBuilder.AddContent(5, "Content");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var aside = cut.Find("aside");
        var h3 = aside.QuerySelector("h3");
        var p = aside.QuerySelector("p");
        Assert.NotNull(h3);
        Assert.NotNull(p);
        Assert.Equal("Title", h3.TextContent);
        Assert.Equal("Content", p.TextContent);
    }
}
