using Bunit;
using Craft.UiComponents.Tests.Base;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftSectionTests : ComponentTestBase
{
    [Fact]
    public void CraftSection_ShouldRenderSectionElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.CloseComponent();
        });

        // Assert
        var section = cut.Find("section");
        Assert.NotNull(section);
    }

    [Fact]
    public void CraftSection_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "intro-section";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var section = cut.Find("section");
        Assert.Equal(expectedId, section.Id);
    }

    [Fact]
    public void CraftSection_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "content-section";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var section = cut.Find("section");
        Assert.Contains(expectedClass, section.ClassName);
    }

    [Fact]
    public void CraftSection_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "padding: 40px;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var section = cut.Find("section");
        Assert.Contains(expectedStyle, section.GetAttribute("style"));
    }

    [Fact]
    public void CraftSection_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Section Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var section = cut.Find("section");
        Assert.Contains(expectedContent, section.TextContent);
    }

    [Fact]
    public void CraftSection_ShouldNotHaveRedundantRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.CloseComponent();
        });

        // Assert - section element should not have role attribute (it's implicit)
        var section = cut.Find("section");
        Assert.Null(section.GetAttribute("role"));
    }

    [Fact]
    public void CraftSection_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var ariaLabelledBy = "section-heading";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.AddAttribute(1, "aria-labelledby", ariaLabelledBy);
            builder.CloseComponent();
        });

        // Assert
        var section = cut.Find("section");
        Assert.Equal(ariaLabelledBy, section.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void CraftSection_ShouldRenderWithHeadingAndContent()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(2, "h2");
                childBuilder.AddContent(3, "Section Title");
                childBuilder.CloseElement();
                childBuilder.OpenElement(4, "p");
                childBuilder.AddContent(5, "Section paragraph");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var section = cut.Find("section");
        var heading = section.QuerySelector("h2");
        var paragraph = section.QuerySelector("p");
        Assert.NotNull(heading);
        Assert.NotNull(paragraph);
    }

    [Fact]
    public void CraftSection_ShouldRenderNestedArticles()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<CraftArticle>(2);
                childBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(articleBuilder =>
                {
                    articleBuilder.AddContent(4, "Article 1");
                }));
                childBuilder.CloseComponent();
                childBuilder.OpenComponent<CraftArticle>(5);
                childBuilder.AddAttribute(6, "ChildContent", (RenderFragment)(articleBuilder =>
                {
                    articleBuilder.AddContent(7, "Article 2");
                }));
                childBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        var section = cut.Find("section");
        var articles = section.QuerySelectorAll("article");
        Assert.Equal(2, articles.Length);
    }

    [Fact]
    public void CraftSection_ShouldRenderWithAllAttributes()
    {
        // Arrange
        var expectedId = "features";
        var expectedClass = "features-section";
        var expectedStyle = "margin: 20px;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSection>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "Style", expectedStyle);
            builder.AddAttribute(4, "data-section", "features");
            builder.AddAttribute(5, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(6, "h2");
                childBuilder.AddContent(7, "Features");
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var section = cut.Find("section");
        Assert.Equal(expectedId, section.Id);
        Assert.Contains(expectedClass, section.ClassName);
        Assert.Contains(expectedStyle, section.GetAttribute("style"));
        Assert.NotNull(section.QuerySelector("h2"));
        Assert.Equal("features", section.GetAttribute("data-section"));
    }
}
