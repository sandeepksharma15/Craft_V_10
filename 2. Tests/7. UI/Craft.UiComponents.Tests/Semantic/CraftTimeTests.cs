using Bunit;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftTimeTests : BunitContext
{
    [Fact]
    public void CraftTime_ShouldRenderTimeElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftTime>(0);
            builder.CloseComponent();
        });

        // Assert
        var time = cut.Find("time");
        Assert.NotNull(time);
    }

    [Fact]
    public void CraftTime_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "publish-time";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftTime>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var time = cut.Find("time");
        Assert.Equal(expectedId, time.Id);
    }

    [Fact]
    public void CraftTime_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "timestamp";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftTime>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var time = cut.Find("time");
        Assert.Contains(expectedClass, time.ClassName);
    }

    [Fact]
    public void CraftTime_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "font-style: italic;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftTime>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var time = cut.Find("time");
        Assert.Contains(expectedStyle, time.GetAttribute("style"));
    }

    [Fact]
    public void CraftTime_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "March 15, 2024";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftTime>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var time = cut.Find("time");
        Assert.Equal(expectedContent, time.TextContent);
    }

    [Fact]
    public void CraftTime_ShouldRenderWithDatetimeAttribute()
    {
        // Arrange
        var datetime = "2024-03-15T14:30:00";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftTime>(0);
            builder.AddAttribute(1, "datetime", datetime);
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(3, "March 15, 2024");
            }));
            builder.CloseComponent();
        });

        // Assert
        var time = cut.Find("time");
        Assert.Equal(datetime, time.GetAttribute("datetime"));
    }

    [Fact]
    public void CraftTime_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataTimestamp = "1710511800";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftTime>(0);
            builder.AddAttribute(1, "data-timestamp", dataTimestamp);
            builder.CloseComponent();
        });

        // Assert
        var time = cut.Find("time");
        Assert.Equal(dataTimestamp, time.GetAttribute("data-timestamp"));
    }

    [Fact]
    public void CraftTime_ShouldRenderRelativeTime()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftTime>(0);
            builder.AddAttribute(1, "datetime", "2024-03-15");
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(3, "2 days ago");
            }));
            builder.CloseComponent();
        });

        // Assert
        var time = cut.Find("time");
        Assert.Equal("2024-03-15", time.GetAttribute("datetime"));
        Assert.Equal("2 days ago", time.TextContent);
    }

    [Fact]
    public void CraftTime_ShouldRenderInArticleContext()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "h1");
                childBuilder.AddContent(3, "Article Title");
                childBuilder.CloseElement();
                childBuilder.OpenElement(4, "time");
                childBuilder.AddAttribute(5, "datetime", "2024-03-15");
                childBuilder.AddContent(6, "Published: March 15, 2024");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var article = cut.Find("article");
        var time = article.QuerySelector("time");
        Assert.NotNull(time);
        Assert.Equal("2024-03-15", time.GetAttribute("datetime"));
        Assert.Contains("March 15, 2024", time.TextContent);
    }

    [Fact]
    public void CraftTime_ShouldRenderWithAllAttributes()
    {
        // Arrange
        var expectedId = "updated-time";
        var expectedClass = "last-modified";
        var expectedStyle = "font-size: 0.9em;";
        var expectedContent = "Last updated: March 15, 2024";
        var datetime = "2024-03-15T14:30:00Z";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftTime>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "datetime", datetime);
            builder.AddAttribute(5, "data-format", "relative");
            builder.AddAttribute(6, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(7, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var time = cut.Find("time");
        Assert.Equal(expectedId, time.Id);
        Assert.Contains(expectedClass, time.ClassName);
        Assert.Contains(expectedStyle, time.GetAttribute("style"));
        Assert.Equal(expectedContent, time.TextContent);
        Assert.Equal(datetime, time.GetAttribute("datetime"));
        Assert.Equal("relative", time.GetAttribute("data-format"));
    }
}
