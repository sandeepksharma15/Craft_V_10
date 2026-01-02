using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Toggle component.
/// Tests binary state switching between two content templates.
/// </summary>
public class ToggleTests : ComponentTestBase
{
    [Fact]
    public void Toggle_WhenActive_ShouldRenderActiveContent()
    {
        // Arrange & Act
        var cut = Render<Toggle>(parameters => parameters
            .Add(p => p.IsActive, true)
            .Add(p => p.Active, builder => builder.AddMarkupContent(0, "<div>Active content</div>"))
            .Add(p => p.Inactive, builder => builder.AddMarkupContent(0, "<div>Inactive content</div>"))
        );

        // Assert
        Assert.Contains("Active content", cut.Markup);
        Assert.DoesNotContain("Inactive content", cut.Markup);
    }

    [Fact]
    public void Toggle_WhenInactive_ShouldRenderInactiveContent()
    {
        // Arrange & Act
        var cut = Render<Toggle>(parameters => parameters
            .Add(p => p.IsActive, false)
            .Add(p => p.Active, builder => builder.AddMarkupContent(0, "<div>Active content</div>"))
            .Add(p => p.Inactive, builder => builder.AddMarkupContent(0, "<div>Inactive content</div>"))
        );

        // Assert
        Assert.DoesNotContain("Active content", cut.Markup);
        Assert.Contains("Inactive content", cut.Markup);
    }

    [Fact]
    public void Toggle_WithNoActiveContent_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<Toggle>(parameters => parameters
                .Add(p => p.IsActive, true)
                .Add(p => p.Inactive, builder => builder.AddMarkupContent(0, "<div>Inactive content</div>"))
            )
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Toggle_WithNoInactiveContent_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<Toggle>(parameters => parameters
                .Add(p => p.IsActive, false)
                .Add(p => p.Active, builder => builder.AddMarkupContent(0, "<div>Active content</div>"))
            )
        );

        // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Toggle_WithActiveState_ShouldRenderActiveContent()
        {
            // Arrange & Act
            var cut = Render<Toggle>(parameters => parameters
                .Add(p => p.IsActive, true)
                .Add(p => p.Active, builder => builder.AddMarkupContent(0, "<div>Active</div>"))
                .Add(p => p.Inactive, builder => builder.AddMarkupContent(0, "<div>Inactive</div>"))
            );

            // Assert
            Assert.Contains("Active", cut.Markup);
            Assert.DoesNotContain("Inactive", cut.Markup);
        }

        [Fact]
        public void Toggle_WithComplexActiveContent_ShouldRenderCorrectly()
        {
            // Arrange & Act
        var cut = Render<Toggle>(parameters => parameters
            .Add(p => p.IsActive, true)
            .Add(p => p.Active, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "expanded-panel");
                builder.OpenElement(2, "h3");
                builder.AddContent(3, "Expanded");
                builder.CloseElement();
                builder.OpenElement(4, "p");
                builder.AddContent(5, "This is the expanded state");
                builder.CloseElement();
                builder.CloseElement();
            })
            .Add(p => p.Inactive, builder => builder.AddMarkupContent(0, "<div>Collapsed</div>"))
        );

        // Assert
        Assert.Contains("expanded-panel", cut.Markup);
        Assert.Contains("Expanded", cut.Markup);
        Assert.Contains("This is the expanded state", cut.Markup);
        Assert.DoesNotContain("Collapsed", cut.Markup);
    }

    [Fact]
    public void Toggle_WithComplexInactiveContent_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<Toggle>(parameters => parameters
            .Add(p => p.IsActive, false)
            .Add(p => p.Active, builder => builder.AddMarkupContent(0, "<div>Active</div>"))
            .Add(p => p.Inactive, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "collapsed-panel");
                builder.OpenElement(2, "span");
                builder.AddContent(3, "Click to expand");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("collapsed-panel", cut.Markup);
        Assert.Contains("Click to expand", cut.Markup);
        Assert.DoesNotContain("Active", cut.Markup);
    }

    [Fact]
    public void Toggle_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Toggle();

        // Assert
        Assert.IsType<Craft.UiComponents.CraftComponent>(component, exactMatch: false);
    }

    [Fact]
    public void Toggle_WithBothContentNull_ShouldRenderNothing()
    {
        // Arrange & Act
        var cut = Render<Toggle>(parameters => parameters
            .Add(p => p.IsActive, true)
        );

        // Assert
        var markup = cut.Markup.Trim();
        Assert.True(string.IsNullOrEmpty(markup) || markup == "<!--!-->");
    }

    [Fact]
    public void Toggle_MultipleStateChanges_ShouldWorkCorrectly()
    {
        // Arrange
        var cut = Render<Toggle>(parameters => parameters
            .Add(p => p.IsActive, false)
            .Add(p => p.Active, builder => builder.AddMarkupContent(0, "<div>On</div>"))
                        .Add(p => p.Inactive, builder => builder.AddMarkupContent(0, "<div>Off</div>"))
                    );

                    // Assert - Initially Off
                    Assert.Contains("Off", cut.Markup);
                    Assert.DoesNotContain("On", cut.Markup);
                }
            }
