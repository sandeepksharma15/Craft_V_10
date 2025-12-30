using Bunit;
using Craft.UiComponents.Tests.Base;
using Craft.UiComponents.Semantic;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftArticleTests : ComponentTestBase
{
    [Fact]
    public void CraftArticle_ShouldRenderArticleElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
            builder.CloseComponent();
        });

        // Assert
        var article = cut.Find("article");
        Assert.NotNull(article);
    }

    [Fact]
    public void CraftArticle_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "test-article";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var article = cut.Find("article");
        Assert.Equal(expectedId, article.Id);
    }

    [Fact]
    public void CraftArticle_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "custom-class";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var article = cut.Find("article");
        Assert.Contains(expectedClass, article.ClassName);
    }

    [Fact]
    public void CraftArticle_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "color: red;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var article = cut.Find("article");
        Assert.Contains(expectedStyle, article.GetAttribute("style"));
    }

    [Fact]
    public void CraftArticle_ShouldRenderChildContent()
    {
        // Arrange
        var expectedContent = "Test Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(2, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var article = cut.Find("article");
        Assert.Contains(expectedContent, article.TextContent);
    }

    [Fact]
    public void CraftArticle_ShouldHaveRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
            builder.CloseComponent();
        });

        // Assert
        var article = cut.Find("article");
        Assert.Equal("article", article.GetAttribute("role"));
    }

    [Fact]
    public void CraftArticle_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataTestId = "test-value";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
            builder.AddAttribute(1, "data-testid", dataTestId);
            builder.CloseComponent();
        });

        // Assert
        var article = cut.Find("article");
        Assert.Equal(dataTestId, article.GetAttribute("data-testid"));
    }

    [Fact]
    public void CraftArticle_ShouldRenderWithMultipleAttributes()
    {
        // Arrange
        var expectedId = "test-article";
        var expectedClass = "custom-class";
        var expectedStyle = "color: blue;";
        var expectedContent = "Article Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
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
        var article = cut.Find("article");
        Assert.Equal(expectedId, article.Id);
        Assert.Contains(expectedClass, article.ClassName);
        Assert.Contains(expectedStyle, article.GetAttribute("style"));
        Assert.Contains(expectedContent, article.TextContent);
    }

    [Fact]
    public void CraftArticle_ShouldRenderNestedComponents()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftArticle>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<CraftDiv>(2);
                childBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(divChildBuilder =>
                {
                    divChildBuilder.AddContent(4, "Nested Content");
                }));
                childBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Assert
        var article = cut.Find("article");
        var div = article.QuerySelector("div");
        Assert.NotNull(div);
        Assert.Contains("Nested Content", div.TextContent);
    }
}
