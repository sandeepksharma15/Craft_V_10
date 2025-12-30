using Bunit;
using Craft.UiComponents.Semantic;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftSpanTests : ComponentTestBase
{
    [Fact]
    public void CraftSpan_ShouldRenderSpanElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSpan>(0);
            builder.CloseComponent();
        });

        // Assert
        var span = cut.Find("span");
        Assert.NotNull(span);
    }

    [Fact]
    public void CraftSpan_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "test-span";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSpan>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var span = cut.Find("span");
        Assert.Equal(expectedId, span.Id);
    }

    [Fact]
    public void CraftSpan_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "highlight";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSpan>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var span = cut.Find("span");
        Assert.Contains(expectedClass, span.ClassName);
    }

    [Fact]
    public void CraftSpan_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "color: red;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSpan>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var span = cut.Find("span");
        Assert.Contains(expectedStyle, span.GetAttribute("style"));
    }

    [Fact]
    public void CraftSpan_ShouldRenderWithChildContent()
    {
        // Arrange
        var expectedContent = "Inline text content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSpan>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var span = cut.Find("span");
        Assert.Equal(expectedContent, span.TextContent);
    }

    [Fact]
    public void CraftSpan_ShouldHaveComponentCssClass()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSpan>(0);
            builder.CloseComponent();
        });

        // Assert
        var span = cut.Find("span");
        Assert.Contains("craft-span", span.ClassName);
    }

    [Fact]
    public void CraftSpan_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataValue = "important";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSpan>(0);
            builder.AddAttribute(1, "data-importance", dataValue);
            builder.CloseComponent();
        });

        // Assert
        var span = cut.Find("span");
        Assert.Equal(dataValue, span.GetAttribute("data-importance"));
    }

    [Fact]
    public void CraftSpan_ShouldRenderWithAllAttributes()
    {
        // Arrange
        var expectedId = "my-span";
        var expectedClass = "custom-class";
        var expectedContent = "Test content";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftSpan>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Class", expectedClass);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var span = cut.Find("span");
        Assert.Equal(expectedId, span.Id);
        Assert.Contains(expectedClass, span.ClassName);
        Assert.Equal(expectedContent, span.TextContent);
    }
}
