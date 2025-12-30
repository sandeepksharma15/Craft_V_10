using Bunit;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftFigCaptionTests : BunitContext
{
    [Fact]
    public void CraftFigCaption_ShouldRenderFigcaptionElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigCaption>(0);
            builder.CloseComponent();
        });

        // Assert
        var figcaption = cut.Find("figcaption");
        Assert.NotNull(figcaption);
    }

    [Fact]
    public void CraftFigCaption_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "test-caption";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigCaption>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var figcaption = cut.Find("figcaption");
        Assert.Equal(expectedId, figcaption.Id);
    }

    [Fact]
    public void CraftFigCaption_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "caption-text";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigCaption>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var figcaption = cut.Find("figcaption");
        Assert.Contains(expectedClass, figcaption.ClassName);
    }

    [Fact]
    public void CraftFigCaption_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "text-align: center;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigCaption>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var figcaption = cut.Find("figcaption");
        Assert.Contains(expectedStyle, figcaption.GetAttribute("style"));
    }

    [Fact]
    public void CraftFigCaption_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Figure 1: Sample Image Caption";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigCaption>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var figcaption = cut.Find("figcaption");
        Assert.Equal(expectedContent, figcaption.TextContent);
    }

    [Fact]
    public void CraftFigCaption_ShouldHaveRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigCaption>(0);
            builder.CloseComponent();
        });

        // Assert
        var figcaption = cut.Find("figcaption");
        Assert.Equal("contentinfo", figcaption.GetAttribute("role"));
    }

    [Fact]
    public void CraftFigCaption_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var ariaLabel = "Image caption";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigCaption>(0);
            builder.AddAttribute(1, "aria-label", ariaLabel);
            builder.CloseComponent();
        });

        // Assert
        var figcaption = cut.Find("figcaption");
        Assert.Equal(ariaLabel, figcaption.GetAttribute("aria-label"));
    }

    [Fact]
    public void CraftFigCaption_ShouldRenderWithHtmlContent()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigCaption>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "strong");
                childBuilder.AddContent(3, "Figure 1:");
                childBuilder.CloseElement();
                childBuilder.AddContent(4, " ");
                childBuilder.OpenElement(5, "em");
                childBuilder.AddContent(6, "Description");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var figcaption = cut.Find("figcaption");
        var strong = figcaption.QuerySelector("strong");
        var em = figcaption.QuerySelector("em");
        Assert.NotNull(strong);
        Assert.NotNull(em);
        Assert.Equal("Figure 1:", strong.TextContent);
        Assert.Equal("Description", em.TextContent);
    }

    [Fact]
    public void CraftFigCaption_ShouldRenderWithMultipleAttributes()
    {
        // Arrange
        var expectedId = "caption-1";
        var expectedClass = "image-caption";
        var expectedStyle = "color: gray;";
        var expectedContent = "Photo Caption";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigCaption>(0);
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
        var figcaption = cut.Find("figcaption");
        Assert.Equal(expectedId, figcaption.Id);
        Assert.Contains(expectedClass, figcaption.ClassName);
        Assert.Contains(expectedStyle, figcaption.GetAttribute("style"));
        Assert.Equal(expectedContent, figcaption.TextContent);
    }
}
