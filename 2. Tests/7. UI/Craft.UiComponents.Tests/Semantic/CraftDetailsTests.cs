using Bunit;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftDetailsTests : BunitContext
{
    [Fact]
    public void CraftDetails_ShouldRenderDetailsElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        Assert.NotNull(details);
    }

    [Fact]
    public void CraftDetails_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "test-details";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        Assert.Equal(expectedId, details.Id);
    }

    [Fact]
    public void CraftDetails_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "details-class";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        Assert.Contains(expectedClass, details.ClassName);
    }

    [Fact]
    public void CraftDetails_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "border: 1px solid black;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        Assert.Contains(expectedStyle, details.GetAttribute("style"));
    }

    [Fact]
    public void CraftDetails_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Details Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        Assert.Contains(expectedContent, details.TextContent);
    }

    [Fact]
    public void CraftDetails_ShouldHaveRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        Assert.Equal("contentinfo", details.GetAttribute("role"));
    }

    [Fact]
    public void CraftDetails_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var openAttribute = "open";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
            builder.AddAttribute(1, "open", openAttribute);
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        Assert.Equal(openAttribute, details.GetAttribute("open"));
    }

    [Fact]
    public void CraftDetails_ShouldRenderWithSummaryChild()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<CraftSummary>(2);
                childBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(summaryBuilder =>
                {
                    summaryBuilder.AddContent(4, "Summary");
                }));
                childBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        var summary = details.QuerySelector("summary");
        Assert.NotNull(summary);
        Assert.Contains("Summary", summary.TextContent);
    }

    [Fact]
    public void CraftDetails_ShouldRenderWithMultipleAttributes()
    {
        // Arrange
        var expectedId = "details-1";
        var expectedClass = "expandable";
        var expectedStyle = "margin: 10px;";
        var expectedContent = "Expandable Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
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
        var details = cut.Find("details");
        Assert.Equal(expectedId, details.Id);
        Assert.Contains(expectedClass, details.ClassName);
        Assert.Contains(expectedStyle, details.GetAttribute("style"));
        Assert.Contains(expectedContent, details.TextContent);
    }

    [Fact]
    public void CraftDetails_ShouldRenderCompleteStructure()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDetails>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "summary");
                childBuilder.AddContent(3, "Click to expand");
                childBuilder.CloseElement();
                childBuilder.OpenElement(4, "p");
                childBuilder.AddContent(5, "Hidden content");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        var summary = details.QuerySelector("summary");
        var paragraph = details.QuerySelector("p");
        Assert.NotNull(summary);
        Assert.NotNull(paragraph);
        Assert.Equal("Click to expand", summary.TextContent);
        Assert.Equal("Hidden content", paragraph.TextContent);
    }
}
