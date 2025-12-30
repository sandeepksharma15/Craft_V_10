using Bunit;
using Craft.UiComponents.Semantic;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftPreTests : ComponentTestBase
{
    [Fact]
    public void CraftPre_ShouldRenderPreElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftPre>(0);
            builder.CloseComponent();
        });

        // Assert
        var pre = cut.Find("pre");
        Assert.NotNull(pre);
    }

    [Fact]
    public void CraftPre_ShouldRenderWithChildContent()
    {
        // Arrange
        var expectedContent = "Line 1\n  Line 2\n    Line 3";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftPre>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var pre = cut.Find("pre");
        Assert.Contains(expectedContent, pre.TextContent);
    }

    [Fact]
    public void CraftPre_ShouldRenderWithContentParameter()
    {
        // Arrange
        var expectedContent = "function test() {\n  return true;\n}";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftPre>(0);
            builder.AddAttribute(1, "Content", expectedContent);
            builder.CloseComponent();
        });

        // Assert
        var pre = cut.Find("pre");
        Assert.Contains(expectedContent, pre.TextContent);
    }

    [Fact]
    public void CraftPre_ContentParameterShouldTakePrecedenceOverChildContent()
    {
        // Arrange
        var contentParam = "Content from parameter";
        var childContent = "Child content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftPre>(0);
            builder.AddAttribute(1, "Content", contentParam);
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, childContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var pre = cut.Find("pre");
        Assert.Contains(contentParam, pre.TextContent);
        Assert.DoesNotContain(childContent, pre.TextContent);
    }

    [Fact]
    public void CraftPre_ShouldRenderWithWrapLines()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftPre>(0);
            builder.AddAttribute(1, "WrapLines", true);
            builder.CloseComponent();
        });

        // Assert
        var pre = cut.Find("pre");
        Assert.Contains("craft-pre-wrap", pre.ClassName);
    }

    [Fact]
    public void CraftPre_ShouldHaveComponentCssClass()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftPre>(0);
            builder.CloseComponent();
        });

        // Assert
        var pre = cut.Find("pre");
        Assert.Contains("craft-pre", pre.ClassName);
    }

    [Fact]
    public void CraftPre_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "code-block";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftPre>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var pre = cut.Find("pre");
        Assert.Contains(expectedClass, pre.ClassName);
    }

    [Fact]
    public void CraftPre_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "code-example";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftPre>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var pre = cut.Find("pre");
        Assert.Equal(expectedId, pre.Id);
    }
}
