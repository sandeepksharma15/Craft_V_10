using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Placeholder component.
/// Tests skeleton/loading placeholder rendering with variants.
/// </summary>
public class PlaceholderTests : ComponentTestBase
{
    [Fact]
    public void Placeholder_WhenLoading_ShouldRenderPlaceholder()
    {
        // Arrange & Act
        var cut = Render<Placeholder>(parameters => parameters
            .Add(p => p.Loading, true)
            .Add(p => p.Lines, 3)
            .Add(p => p.Content, builder => builder.AddMarkupContent(0, "<div>Actual content</div>"))
        );

        // Assert
        Assert.Contains("craft-placeholder", cut.Markup);
        Assert.DoesNotContain("Actual content", cut.Markup);
    }

    [Fact]
    public void Placeholder_WhenNotLoading_ShouldRenderActualContent()
    {
        // Arrange & Act
        var cut = Render<Placeholder>(parameters => parameters
            .Add(p => p.Loading, false)
            .Add(p => p.Lines, 3)
            .Add(p => p.Content, builder => builder.AddMarkupContent(0, "<div>Actual content</div>"))
        );

        // Assert
        Assert.Contains("Actual content", cut.Markup);
        Assert.DoesNotContain("craft-placeholder", cut.Markup);
    }

    [Fact]
    public void Placeholder_WithTextVariant_ShouldRenderTextPlaceholder()
    {
        // Arrange & Act
        var cut = Render<Placeholder>(parameters => parameters
            .Add(p => p.Loading, true)
            .Add(p => p.Variant, PlaceholderVariant.Text)
            .Add(p => p.Lines, 2)
        );

        // Assert
        Assert.Contains("craft-placeholder-text", cut.Markup);
    }

    [Fact]
    public void Placeholder_WithCircleVariant_ShouldRenderCirclePlaceholder()
    {
        // Arrange & Act
        var cut = Render<Placeholder>(parameters => parameters
            .Add(p => p.Loading, true)
            .Add(p => p.Variant, PlaceholderVariant.Circle)
            .Add(p => p.Lines, 1)
        );

        // Assert
        Assert.Contains("craft-placeholder-circle", cut.Markup);
    }

    [Fact]
    public void Placeholder_WithRectangleVariant_ShouldRenderRectanglePlaceholder()
    {
        // Arrange & Act
        var cut = Render<Placeholder>(parameters => parameters
            .Add(p => p.Loading, true)
            .Add(p => p.Variant, PlaceholderVariant.Rectangle)
            .Add(p => p.Lines, 1)
        );

        // Assert
        Assert.Contains("craft-placeholder-rectangle", cut.Markup);
    }

    [Fact]
    public void Placeholder_WithMultipleLines_ShouldRenderCorrectNumberOfLines()
    {
        // Arrange & Act
        var cut = Render<Placeholder>(parameters => parameters
            .Add(p => p.Loading, true)
            .Add(p => p.Lines, 5)
            .Add(p => p.Variant, PlaceholderVariant.Text)
        );

        // Assert
        var lineCount = cut.Markup.Split("craft-placeholder-line").Length - 1;
        Assert.Equal(5, lineCount);
    }

    [Fact]
    public void Placeholder_WithDefaultLines_ShouldRenderThreeLines()
    {
        // Arrange
        var component = new Placeholder();

        // Assert
        Assert.Equal(3, component.Lines);
    }

    [Fact]
    public void Placeholder_WithDefaultVariant_ShouldBeText()
    {
        // Arrange
        var component = new Placeholder();

        // Assert
        Assert.Equal(PlaceholderVariant.Text, component.Variant);
    }

    [Fact]
    public void Placeholder_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Placeholder();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public void Placeholder_WithZeroLines_ShouldNotRenderLines()
    {
        // Arrange & Act
        var cut = Render<Placeholder>(parameters => parameters
            .Add(p => p.Loading, true)
            .Add(p => p.Lines, 0)
        );

        // Assert
                Assert.DoesNotContain("craft-placeholder-line", cut.Markup);
            }

            [Fact]
            public void Placeholder_WithLoadingFalse_ShouldShowRealContent()
            {
                // Arrange & Act
                var cut = Render<Placeholder>(parameters => parameters
                    .Add(p => p.Loading, false)
                    .Add(p => p.Lines, 2)
                    .Add(p => p.Content, builder => builder.AddMarkupContent(0, "<div>Real content</div>"))
                );

                // Assert - should show real content, not placeholder
                Assert.DoesNotContain("craft-placeholder", cut.Markup);
                Assert.Contains("Real content", cut.Markup);
            }

            [Fact]
            public void PlaceholderVariant_ShouldHaveAllExpectedValues()
            {
                // Assert
                var values = Enum.GetValues<PlaceholderVariant>();
                Assert.Contains(PlaceholderVariant.Text, values);
                Assert.Contains(PlaceholderVariant.Circle, values);
                Assert.Contains(PlaceholderVariant.Rectangle, values);
                Assert.Equal(3, values.Length);
            }
        }
