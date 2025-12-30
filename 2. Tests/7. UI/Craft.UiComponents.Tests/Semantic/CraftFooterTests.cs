using Bunit;
using Craft.UiComponents.Tests.Base;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftFooterTests : ComponentTestBase
{
    [Fact]
    public void CraftFooter_ShouldRenderFooterElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFooter>(0);
            builder.CloseComponent();
        });

        // Assert
        var footer = cut.Find("footer");
        Assert.NotNull(footer);
    }

    [Fact]
    public void CraftFooter_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "page-footer";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFooter>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var footer = cut.Find("footer");
        Assert.Equal(expectedId, footer.Id);
    }

    [Fact]
    public void CraftFooter_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "site-footer";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFooter>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var footer = cut.Find("footer");
        Assert.Contains(expectedClass, footer.ClassName);
    }

    [Fact]
    public void CraftFooter_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "background-color: #333;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFooter>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var footer = cut.Find("footer");
        Assert.Contains(expectedStyle, footer.GetAttribute("style"));
    }

    [Fact]
    public void CraftFooter_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "� 2024 Company Name";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFooter>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var footer = cut.Find("footer");
        Assert.Contains(expectedContent, footer.TextContent);
    }

    [Fact]
    public void CraftFooter_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var ariaLabel = "Site footer";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFooter>(0);
            builder.AddAttribute(1, "aria-label", ariaLabel);
            builder.CloseComponent();
        });

        // Assert
        var footer = cut.Find("footer");
        Assert.Equal(ariaLabel, footer.GetAttribute("aria-label"));
    }

    [Fact]
    public void CraftFooter_ShouldRenderWithNestedComponents()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFooter>(0);
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
        var footer = cut.Find("footer");
        var nav = footer.QuerySelector("nav");
        Assert.NotNull(nav);
    }

    [Fact]
    public void CraftFooter_ShouldRenderComplexStructure()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFooter>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "div");
                childBuilder.AddAttribute(3, "class", "footer-content");
                childBuilder.OpenElement(4, "p");
                childBuilder.AddContent(5, "Contact: info@example.com");
                childBuilder.CloseElement();
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var footer = cut.Find("footer");
        var div = footer.QuerySelector("div.footer-content");
        Assert.NotNull(div);
    }

    [Fact]
    public void CraftFooter_ShouldRenderWithMultipleAttributes()
    {
        // Arrange
        var expectedId = "main-footer";
        var expectedClass = "sticky-footer";
        var expectedStyle = "position: fixed;";
        var expectedContent = "Footer Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFooter>(0);
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
        var footer = cut.Find("footer");
        Assert.Equal(expectedId, footer.Id);
        Assert.Contains(expectedClass, footer.ClassName);
        Assert.Contains(expectedStyle, footer.GetAttribute("style"));
        Assert.Contains(expectedContent, footer.TextContent);
    }
}
