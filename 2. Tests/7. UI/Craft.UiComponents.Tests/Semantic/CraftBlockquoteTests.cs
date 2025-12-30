using Bunit;
using Craft.UiComponents.Semantic;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftBlockquoteTests : ComponentTestBase
{
    [Fact]
    public void CraftBlockquote_ShouldRenderBlockquoteElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftBlockquote>(0);
            builder.CloseComponent();
        });

        // Assert
        var blockquote = cut.Find("blockquote");
        Assert.NotNull(blockquote);
    }

    [Fact]
    public void CraftBlockquote_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "test-quote";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftBlockquote>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var blockquote = cut.Find("blockquote");
        Assert.Equal(expectedId, blockquote.Id);
    }

    [Fact]
    public void CraftBlockquote_ShouldRenderWithCite()
    {
        // Arrange
        var expectedCite = "https://www.example.com/source";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftBlockquote>(0);
            builder.AddAttribute(1, "Cite", expectedCite);
            builder.CloseComponent();
        });

        // Assert
        var blockquote = cut.Find("blockquote");
        Assert.Equal(expectedCite, blockquote.GetAttribute("cite"));
    }

    [Fact]
    public void CraftBlockquote_ShouldRenderWithChildContent()
    {
        // Arrange
        var expectedContent = "This is a famous quote from an important source.";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftBlockquote>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var blockquote = cut.Find("blockquote");
        Assert.Contains(expectedContent, blockquote.TextContent);
    }

    [Fact]
    public void CraftBlockquote_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "highlight-quote";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftBlockquote>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var blockquote = cut.Find("blockquote");
        Assert.Contains(expectedClass, blockquote.ClassName);
    }

    [Fact]
    public void CraftBlockquote_ShouldHaveComponentCssClass()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftBlockquote>(0);
            builder.CloseComponent();
        });

        // Assert
        var blockquote = cut.Find("blockquote");
        Assert.Contains("craft-blockquote", blockquote.ClassName);
    }

    [Fact]
    public void CraftBlockquote_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataAuthor = "John Doe";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftBlockquote>(0);
            builder.AddAttribute(1, "data-author", dataAuthor);
            builder.CloseComponent();
        });

        // Assert
        var blockquote = cut.Find("blockquote");
        Assert.Equal(dataAuthor, blockquote.GetAttribute("data-author"));
    }

    [Fact]
    public void CraftBlockquote_ShouldRenderWithCiteAndContent()
    {
        // Arrange
        var expectedCite = "https://example.com";
        var expectedContent = "Quote text";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftBlockquote>(0);
            builder.AddAttribute(1, "Cite", expectedCite);
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var blockquote = cut.Find("blockquote");
        Assert.Equal(expectedCite, blockquote.GetAttribute("cite"));
        Assert.Contains(expectedContent, blockquote.TextContent);
    }
}
