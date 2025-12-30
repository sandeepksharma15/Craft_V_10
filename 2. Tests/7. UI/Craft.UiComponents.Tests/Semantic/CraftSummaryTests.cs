using Bunit;
using Craft.UiComponents.Tests.Base;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftSummaryTests : ComponentTestBase
{
    [Fact]
    public void CraftSummary_ShouldRenderSummaryElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSummary>(0);
            builder.CloseComponent();
        });

        // Assert
        var summary = cut.Find("summary");
        Assert.NotNull(summary);
    }

    [Fact]
    public void CraftSummary_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "details-summary";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSummary>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var summary = cut.Find("summary");
        Assert.Equal(expectedId, summary.Id);
    }

    [Fact]
    public void CraftSummary_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "expandable-header";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSummary>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var summary = cut.Find("summary");
        Assert.Contains(expectedClass, summary.ClassName);
    }

    [Fact]
    public void CraftSummary_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "cursor: pointer;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSummary>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var summary = cut.Find("summary");
        Assert.Contains(expectedStyle, summary.GetAttribute("style"));
    }

    [Fact]
    public void CraftSummary_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Click to expand";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSummary>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var summary = cut.Find("summary");
        Assert.Equal(expectedContent, summary.TextContent);
    }

    [Fact]
    public void CraftSummary_ShouldHaveRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSummary>(0);
            builder.CloseComponent();
        });

        // Assert
        var summary = cut.Find("summary");
        Assert.Equal("article", summary.GetAttribute("role"));
    }

    [Fact]
    public void CraftSummary_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var ariaExpanded = "false";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSummary>(0);
            builder.AddAttribute(1, "aria-expanded", ariaExpanded);
            builder.CloseComponent();
        });

        // Assert
        var summary = cut.Find("summary");
        Assert.Equal(ariaExpanded, summary.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void CraftSummary_ShouldRenderWithIconContent()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSummary>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "span");
                childBuilder.AddAttribute(3, "class", "icon");
                childBuilder.AddContent(4, "?");
                childBuilder.CloseElement();
                childBuilder.AddContent(5, " Expand Details");
            }));
            builder.CloseComponent();
        });

        // Assert
        var summary = cut.Find("summary");
        var icon = summary.QuerySelector("span.icon");
        Assert.NotNull(icon);
        Assert.Equal("?", icon.TextContent);
    }

    [Fact]
    public void CraftSummary_ShouldRenderInDetailsContext()
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
                    summaryBuilder.AddContent(4, "Summary Text");
                }));
                childBuilder.CloseComponent();
                childBuilder.OpenElement(5, "p");
                childBuilder.AddContent(6, "Hidden content");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var details = cut.Find("details");
        var summary = details.QuerySelector("summary");
        Assert.NotNull(summary);
        Assert.Equal("Summary Text", summary.TextContent);
    }

    [Fact]
    public void CraftSummary_ShouldRenderWithAllAttributes()
    {
        // Arrange
        var expectedId = "faq-summary";
        var expectedClass = "accordion-header";
        var expectedStyle = "padding: 10px;";
        var expectedContent = "Frequently Asked Questions";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSummary>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "data-toggle", "collapse");
            builder.AddAttribute(5, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(6, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var summary = cut.Find("summary");
        Assert.Equal(expectedId, summary.Id);
        Assert.Contains(expectedClass, summary.ClassName);
        Assert.Contains(expectedStyle, summary.GetAttribute("style"));
        Assert.Equal(expectedContent, summary.TextContent);
        Assert.Equal("collapse", summary.GetAttribute("data-toggle"));
    }
}
