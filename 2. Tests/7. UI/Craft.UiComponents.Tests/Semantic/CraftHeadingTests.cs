using Bunit;
using Craft.UiComponents.Semantic;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftHeadingTests : ComponentTestBase
{
    [Theory]
    [InlineData(1, "h1")]
    [InlineData(2, "h2")]
    [InlineData(3, "h3")]
    [InlineData(4, "h4")]
    [InlineData(5, "h5")]
    [InlineData(6, "h6")]
    public void CraftHeading_ShouldRenderCorrectHeadingLevel(int level, string expectedTag)
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeading>(0);
            builder.AddAttribute(1, "Level", level);
            builder.CloseComponent();
        });

        // Assert
        var heading = cut.Find(expectedTag);
        Assert.NotNull(heading);
    }

    [Fact]
    public void CraftHeading_ShouldDefaultToH2()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeading>(0);
            builder.CloseComponent();
        });

        // Assert
        var h2 = cut.Find("h2");
        Assert.NotNull(h2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(-1)]
    [InlineData(10)]
    public void CraftHeading_InvalidLevelShouldDefaultToH2(int invalidLevel)
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeading>(0);
            builder.AddAttribute(1, "Level", invalidLevel);
            builder.CloseComponent();
        });

        // Assert
        var h2 = cut.Find("h2");
        Assert.NotNull(h2);
    }

    [Fact]
    public void CraftHeading_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "main-title";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeading>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.AddAttribute(2, "Level", 1);
            builder.CloseComponent();
        });

        // Assert
        var h1 = cut.Find("h1");
        Assert.Equal(expectedId, h1.Id);
    }

    [Fact]
    public void CraftHeading_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "page-title";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeading>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.AddAttribute(2, "Level", 1);
            builder.CloseComponent();
        });

        // Assert
        var h1 = cut.Find("h1");
        Assert.Contains(expectedClass, h1.ClassName);
    }

    [Fact]
    public void CraftHeading_ShouldHaveComponentCssClassWithLevel()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeading>(0);
            builder.AddAttribute(1, "Level", 3);
            builder.CloseComponent();
        });

        // Assert
        var h3 = cut.Find("h3");
        Assert.Contains("craft-heading", h3.ClassName);
        Assert.Contains("craft-heading-3", h3.ClassName);
    }

    [Fact]
    public void CraftHeading_ShouldRenderWithChildContent()
    {
        // Arrange
        var expectedContent = "Welcome to Our Website";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeading>(0);
            builder.AddAttribute(1, "Level", 1);
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var h1 = cut.Find("h1");
        Assert.Equal(expectedContent, h1.TextContent);
    }

    [Fact]
    public void CraftHeading_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "color: blue;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeading>(0);
            builder.AddAttribute(1, "Level", 2);
            builder.AddAttribute(2, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var h2 = cut.Find("h2");
        Assert.Contains(expectedStyle, h2.GetAttribute("style"));
    }

    [Fact]
    public void CraftHeading_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataSection = "hero";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftHeading>(0);
            builder.AddAttribute(1, "Level", 1);
            builder.AddAttribute(2, "data-section", dataSection);
            builder.CloseComponent();
        });

        // Assert
        var h1 = cut.Find("h1");
        Assert.Equal(dataSection, h1.GetAttribute("data-section"));
    }

    [Fact]
    public void CraftHeading_AllLevelsShouldHaveDifferentCssClasses()
    {
        for (int level = 1; level <= 6; level++)
        {
            // Act
            var cut = Render(builder =>
            {
                builder.OpenComponent<CraftHeading>(0);
                builder.AddAttribute(1, "Level", level);
                builder.CloseComponent();
            });

            // Assert
            var heading = cut.Find($"h{level}");
            Assert.Contains($"craft-heading-{level}", heading.ClassName);
        }
    }
}
