using Bunit;
using Craft.UiComponents.Tests.Base;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftMarkTests : ComponentTestBase
{
    [Fact]
    public void CraftMark_ShouldRenderMarkElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMark>(0);
            builder.CloseComponent();
        });

        // Assert
        var mark = cut.Find("mark");
        Assert.NotNull(mark);
    }

    [Fact]
    public void CraftMark_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "highlight-1";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMark>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var mark = cut.Find("mark");
        Assert.Equal(expectedId, mark.Id);
    }

    [Fact]
    public void CraftMark_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "highlight-yellow";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMark>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var mark = cut.Find("mark");
        Assert.Contains(expectedClass, mark.ClassName);
    }

    [Fact]
    public void CraftMark_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "background-color: yellow;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMark>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var mark = cut.Find("mark");
        Assert.Contains(expectedStyle, mark.GetAttribute("style"));
    }

    [Fact]
    public void CraftMark_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Highlighted text";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMark>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var mark = cut.Find("mark");
        Assert.Equal(expectedContent, mark.TextContent);
    }

    [Fact]
    public void CraftMark_ShouldNotHaveRedundantRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMark>(0);
            builder.CloseComponent();
        });

        // Assert - mark element should not have role attribute (it's implicit)
        var mark = cut.Find("mark");
        Assert.Null(mark.GetAttribute("role"));
    }

    [Fact]
    public void CraftMark_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataHighlight = "search-result";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMark>(0);
            builder.AddAttribute(1, "data-highlight", dataHighlight);
            builder.CloseComponent();
        });

        // Assert
        var mark = cut.Find("mark");
        Assert.Equal(dataHighlight, mark.GetAttribute("data-highlight"));
    }

    [Fact]
    public void CraftMark_ShouldRenderInParagraph()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "p");
                childBuilder.AddContent(3, "This is ");
                childBuilder.OpenElement(4, "mark");
                childBuilder.AddContent(5, "highlighted");
                childBuilder.CloseElement();
                childBuilder.AddContent(6, " text.");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        var mark = div.QuerySelector("mark");
        Assert.NotNull(mark);
        Assert.Equal("highlighted", mark.TextContent);
    }

    [Fact]
    public void CraftMark_ShouldRenderMultipleHighlights()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDiv>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "p");
                childBuilder.OpenElement(3, "mark");
                childBuilder.AddContent(4, "First");
                childBuilder.CloseElement();
                childBuilder.AddContent(5, " and ");
                childBuilder.OpenElement(6, "mark");
                childBuilder.AddContent(7, "Second");
                childBuilder.CloseElement();
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var div = cut.Find("div");
        var marks = div.QuerySelectorAll("mark");
        Assert.Equal(2, marks.Length);
    }

    [Fact]
    public void CraftMark_ShouldRenderWithAllAttributes()
    {
        // Arrange
        var expectedId = "search-highlight";
        var expectedClass = "search-match";
        var expectedStyle = "background: #ff0;";
        var expectedContent = "Search Term";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftMark>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "data-match", "exact");
            builder.AddAttribute(5, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(6, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var mark = cut.Find("mark");
        Assert.Equal(expectedId, mark.Id);
        Assert.Contains(expectedClass, mark.ClassName);
        Assert.Contains(expectedStyle, mark.GetAttribute("style"));
        Assert.Equal(expectedContent, mark.TextContent);
        Assert.Equal("exact", mark.GetAttribute("data-match"));
    }
}
