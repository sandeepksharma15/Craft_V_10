using Bunit;
using Craft.UiComponents.Tests.Base;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftMainTests : ComponentTestBase
{
    [Fact]
    public void CraftMain_ShouldRenderMainElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        Assert.NotNull(main);
    }

    [Fact]
    public void CraftMain_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "main-content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        Assert.Equal(expectedId, main.Id);
    }

    [Fact]
    public void CraftMain_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "content-area";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        Assert.Contains(expectedClass, main.ClassName);
    }

    [Fact]
    public void CraftMain_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "max-width: 1200px;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        Assert.Contains(expectedStyle, main.GetAttribute("style"));
    }

    [Fact]
    public void CraftMain_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Main Page Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        Assert.Contains(expectedContent, main.TextContent);
    }

    [Fact]
    public void CraftMain_ShouldHaveRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        Assert.Equal("main", main.GetAttribute("role"));
    }

    [Fact]
    public void CraftMain_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var ariaLabel = "Main content area";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.AddAttribute(1, "aria-label", ariaLabel);
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        Assert.Equal(ariaLabel, main.GetAttribute("aria-label"));
    }

    [Fact]
    public void CraftMain_ShouldRenderWithSectionComponents()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<CraftSection>(2);
                childBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(sectionBuilder =>
                {
                    sectionBuilder.AddContent(4, "Section 1");
                }));
                childBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        var section = main.QuerySelector("section");
        Assert.NotNull(section);
    }

    [Fact]
    public void CraftMain_ShouldRenderComplexLayout()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "section");
                childBuilder.OpenElement(3, "h1");
                childBuilder.AddContent(4, "Title");
                childBuilder.CloseElement();
                childBuilder.CloseElement();
                childBuilder.OpenElement(5, "article");
                childBuilder.OpenElement(6, "h2");
                childBuilder.AddContent(7, "Article Title");
                childBuilder.CloseElement();
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        var section = main.QuerySelector("section");
        var article = main.QuerySelector("article");
        Assert.NotNull(section);
        Assert.NotNull(article);
    }

    [Fact]
    public void CraftMain_ShouldRenderWithAllAttributes()
    {
        // Arrange
        var expectedId = "content";
        var expectedClass = "main-content";
        var expectedStyle = "padding: 20px;";
        var expectedContent = "Primary Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMain>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "data-main", "content");
            builder.AddAttribute(5, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(6, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var main = cut.Find("main");
        Assert.Equal(expectedId, main.Id);
        Assert.Contains(expectedClass, main.ClassName);
        Assert.Contains(expectedStyle, main.GetAttribute("style"));
        Assert.Contains(expectedContent, main.TextContent);
        Assert.Equal("content", main.GetAttribute("data-main"));
    }
}
