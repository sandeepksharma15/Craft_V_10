using Bunit;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftFigureTests : BunitContext
{
    [Fact]
    public void CraftFigure_ShouldRenderFigureElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        Assert.NotNull(figure);
    }

    [Fact]
    public void CraftFigure_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "test-figure";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        Assert.Equal(expectedId, figure.Id);
    }

    [Fact]
    public void CraftFigure_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "image-figure";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        Assert.Contains(expectedClass, figure.ClassName);
    }

    [Fact]
    public void CraftFigure_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "width: 300px;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        Assert.Contains(expectedStyle, figure.GetAttribute("style"));
    }

    [Fact]
    public void CraftFigure_ShouldRenderChildContent()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "img");
                childBuilder.AddAttribute(3, "src", "test.jpg");
                childBuilder.AddAttribute(4, "alt", "Test");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        var img = figure.QuerySelector("img");
        Assert.NotNull(img);
    }

    [Fact]
    public void CraftFigure_ShouldHaveRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        Assert.Equal("contentinfo", figure.GetAttribute("role"));
    }

    [Fact]
    public void CraftFigure_ShouldRenderWithFigcaptionChild()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "img");
                childBuilder.AddAttribute(3, "src", "photo.jpg");
                childBuilder.AddAttribute(4, "alt", "Photo");
                childBuilder.CloseElement();
                childBuilder.OpenElement(5, "figcaption");
                childBuilder.AddContent(6, "Photo caption");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        var img = figure.QuerySelector("img");
        var figcaption = figure.QuerySelector("figcaption");
        Assert.NotNull(img);
        Assert.NotNull(figcaption);
        Assert.Equal("Photo caption", figcaption.TextContent);
    }

    [Fact]
    public void CraftFigure_ShouldRenderWithCraftFigcaptionComponent()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<CraftFigCaption>(2);
                childBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(captionBuilder =>
                {
                    captionBuilder.AddContent(4, "Component Caption");
                }));
                childBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        var figcaption = figure.QuerySelector("figcaption");
        Assert.NotNull(figcaption);
        Assert.Contains("Component Caption", figcaption.TextContent);
    }

    [Fact]
    public void CraftFigure_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataFigure = "main-figure";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.AddAttribute(1, "data-figure", dataFigure);
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        Assert.Equal(dataFigure, figure.GetAttribute("data-figure"));
    }

    [Fact]
    public void CraftFigure_ShouldRenderCompleteStructure()
    {
        // Arrange
        var expectedId = "figure-1";
        var expectedClass = "gallery-item";
        var expectedStyle = "border: 1px solid #ccc;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftFigure>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(5, "img");
                childBuilder.AddAttribute(6, "src", "image.jpg");
                childBuilder.AddAttribute(7, "alt", "Image");
                childBuilder.CloseElement();
                childBuilder.OpenElement(8, "figcaption");
                childBuilder.AddContent(9, "Image description");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var figure = cut.Find("figure");
        Assert.Equal(expectedId, figure.Id);
        Assert.Contains(expectedClass, figure.ClassName);
        Assert.Contains(expectedStyle, figure.GetAttribute("style"));
        var img = figure.QuerySelector("img");
        var figcaption = figure.QuerySelector("figcaption");
        Assert.NotNull(img);
        Assert.NotNull(figcaption);
    }
}
