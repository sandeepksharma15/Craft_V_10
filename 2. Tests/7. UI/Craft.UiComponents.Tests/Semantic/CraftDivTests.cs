using Bunit;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftDivTests : BunitContext
{
    [Fact]
    public void CraftDiv_ShouldRenderDivElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        Assert.NotNull(div);
    }

    [Fact]
    public void CraftDiv_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "test-div";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        Assert.Equal(expectedId, div.Id);
    }

    [Fact]
    public void CraftDiv_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "container";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        Assert.Contains(expectedClass, div.ClassName);
    }

    [Fact]
    public void CraftDiv_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "padding: 20px;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        Assert.Contains(expectedStyle, div.GetAttribute("style"));
    }

    [Fact]
    public void CraftDiv_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Div Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        Assert.Equal(expectedContent, div.TextContent);
    }

    [Fact]
    public void CraftDiv_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataTestId = "test-container";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "data-testid", dataTestId);
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        Assert.Equal(dataTestId, div.GetAttribute("data-testid"));
    }

    [Fact]
    public void CraftDiv_ShouldRenderNestedDivs()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<CraftDiv>(2);
                childBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(innerBuilder =>
                {
                    innerBuilder.AddContent(4, "Nested Div");
                }));
                childBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        var outerDiv = cut.Find("div");
        var innerDiv = outerDiv.QuerySelector("div");
        Assert.NotNull(innerDiv);
        Assert.Equal("Nested Div", innerDiv.TextContent);
    }

    [Fact]
    public void CraftDiv_ShouldRenderComplexHtmlStructure()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "h1");
                childBuilder.AddContent(3, "Title");
                childBuilder.CloseElement();
                childBuilder.OpenElement(4, "p");
                childBuilder.AddContent(5, "Paragraph");
                childBuilder.CloseElement();
                childBuilder.OpenElement(6, "span");
                childBuilder.AddContent(7, "Span");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        var h1 = div.QuerySelector("h1");
        var p = div.QuerySelector("p");
        var span = div.QuerySelector("span");
        Assert.NotNull(h1);
        Assert.NotNull(p);
        Assert.NotNull(span);
    }

    [Fact]
    public void CraftDiv_ShouldRenderWithAllAttributes()
    {
        // Arrange
        var expectedId = "main-container";
        var expectedClass = "wrapper";
        var expectedStyle = "background: white;";
        var expectedContent = "Complete Div";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "data-role", "container");
            builder.AddAttribute(5, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(6, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        Assert.Equal(expectedId, div.Id);
        Assert.Contains(expectedClass, div.ClassName);
        Assert.Contains(expectedStyle, div.GetAttribute("style"));
        Assert.Contains(expectedContent, div.TextContent);
        Assert.Equal("container", div.GetAttribute("data-role"));
    }
}
