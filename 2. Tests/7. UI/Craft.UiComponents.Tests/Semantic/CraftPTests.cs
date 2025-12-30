using Bunit;
using Craft.UiComponents.Semantic;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftPTests : ComponentTestBase
{
    [Fact]
    public void CraftP_ShouldRenderParagraphElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftP>(0);
            builder.CloseComponent();
        });

        // Assert
        var p = cut.Find("p");
        Assert.NotNull(p);
    }

    [Fact]
    public void CraftP_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "intro-paragraph";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftP>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var p = cut.Find("p");
        Assert.Equal(expectedId, p.Id);
    }

    [Fact]
    public void CraftP_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "lead-text";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftP>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var p = cut.Find("p");
        Assert.Contains(expectedClass, p.ClassName);
    }

    [Fact]
    public void CraftP_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "font-size: 1.2em;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftP>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var p = cut.Find("p");
        Assert.Contains(expectedStyle, p.GetAttribute("style"));
    }

    [Fact]
    public void CraftP_ShouldRenderWithChildContent()
    {
        // Arrange
        var expectedContent = "This is a paragraph of text. It contains multiple sentences.";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftP>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var p = cut.Find("p");
        Assert.Equal(expectedContent, p.TextContent);
    }

    [Fact]
    public void CraftP_ShouldHaveComponentCssClass()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftP>(0);
            builder.CloseComponent();
        });

        // Assert
        var p = cut.Find("p");
        Assert.Contains("craft-p", p.ClassName);
    }

    [Fact]
    public void CraftP_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataAttr = "first-paragraph";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftP>(0);
            builder.AddAttribute(1, "data-position", dataAttr);
            builder.CloseComponent();
        });

        // Assert
        var p = cut.Find("p");
        Assert.Equal(dataAttr, p.GetAttribute("data-position"));
    }

    [Fact]
    public void CraftP_ShouldRenderWithComplexChildContent()
    {
        // Arrange
        var expectedText = "Hello";
        var expectedStrongText = "World";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftP>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedText);
                childBuilder.OpenElement(1, "strong");
                childBuilder.AddContent(2, expectedStrongText);
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var p = cut.Find("p");
        Assert.Contains(expectedText, p.TextContent);
        Assert.Contains(expectedStrongText, p.TextContent);
        var strong = p.QuerySelector("strong");
        Assert.NotNull(strong);
    }
}
