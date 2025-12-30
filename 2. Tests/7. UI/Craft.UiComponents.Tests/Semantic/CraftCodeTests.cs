using Bunit;
using Craft.UiComponents.Semantic;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftCodeTests : ComponentTestBase
{
    [Fact]
    public void CraftCode_ShouldRenderCodeElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftCode>(0);
            builder.CloseComponent();
        });

        // Assert
        var code = cut.Find("code");
        Assert.NotNull(code);
    }

    [Fact]
    public void CraftCode_ShouldRenderWithChildContent()
    {
        // Arrange
        var expectedContent = "var x = 10;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftCode>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var code = cut.Find("code");
        Assert.Contains(expectedContent, code.TextContent);
    }

    [Fact]
    public void CraftCode_ShouldRenderWithContentParameter()
    {
        // Arrange
        var expectedContent = "Console.WriteLine();";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftCode>(0);
            builder.AddAttribute(1, "Content", expectedContent);
            builder.CloseComponent();
        });

        // Assert
        var code = cut.Find("code");
        Assert.Contains(expectedContent, code.TextContent);
    }

    [Fact]
    public void CraftCode_ContentParameterShouldTakePrecedenceOverChildContent()
    {
        // Arrange
        var contentParam = "Content Parameter";
        var childContent = "Child Content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftCode>(0);
            builder.AddAttribute(1, "Content", contentParam);
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, childContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var code = cut.Find("code");
        Assert.Contains(contentParam, code.TextContent);
        Assert.DoesNotContain(childContent, code.TextContent);
    }

    [Fact]
    public void CraftCode_ShouldRenderWithLanguage()
    {
        // Arrange
        var expectedLanguage = "csharp";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftCode>(0);
            builder.AddAttribute(1, "Language", expectedLanguage);
            builder.CloseComponent();
        });

        // Assert
        var code = cut.Find("code");
        Assert.Contains($"language-{expectedLanguage}", code.ClassName);
    }

    [Fact]
    public void CraftCode_ShouldHaveComponentCssClass()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftCode>(0);
            builder.CloseComponent();
        });

        // Assert
        var code = cut.Find("code");
        Assert.Contains("craft-code", code.ClassName);
    }

    [Fact]
    public void CraftCode_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "inline-code";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftCode>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var code = cut.Find("code");
        Assert.Contains(expectedClass, code.ClassName);
    }

    [Fact]
    public void CraftCode_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "code-snippet";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftCode>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var code = cut.Find("code");
        Assert.Equal(expectedId, code.Id);
    }

    [Fact]
    public void CraftCode_ShouldRenderWithLanguageAndContent()
    {
        // Arrange
        var expectedLanguage = "javascript";
        var expectedContent = "console.log('Hello');";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftCode>(0);
            builder.AddAttribute(1, "Language", expectedLanguage);
            builder.AddAttribute(2, "Content", expectedContent);
            builder.CloseComponent();
        });

        // Assert
        var code = cut.Find("code");
        Assert.Contains($"language-{expectedLanguage}", code.ClassName);
        Assert.Contains(expectedContent, code.TextContent);
    }
}
