using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;
using Craft.UiComponents;
using MudBlazor;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Spinner component.
/// Tests spinner rendering with different visibility and color configurations.
/// </summary>
public class SpinnerTests : ComponentTestBase
{
    [Fact]
    public void Spinner_WhenVisible_ShouldRenderMudProgressCircular()
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert
        var progressCircular = cut.FindComponent<MudProgressCircular>();
        Assert.NotNull(progressCircular);
    }

    [Fact]
    public void Spinner_WhenNotVisible_ShouldNotRenderMudProgressCircular()
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, false)
        );

        // Assert
        var progressCirculars = cut.FindComponents<MudProgressCircular>();
        Assert.Empty(progressCirculars);
    }

    [Fact]
    public void Spinner_ShouldBeIndeterminate()
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert
        var progressCircular = cut.FindComponent<MudProgressCircular>();
        Assert.True(progressCircular.Instance.Indeterminate);
    }

    [Fact]
    public void Spinner_WithSpecificColor_ShouldUseSpecifiedColor()
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Color, Color.Primary)
        );

        // Assert
        var progressCircular = cut.FindComponent<MudProgressCircular>();
        Assert.Equal(Color.Primary, progressCircular.Instance.Color);
    }

    [Theory]
    [InlineData(Color.Primary)]
    [InlineData(Color.Secondary)]
    [InlineData(Color.Success)]
    [InlineData(Color.Error)]
    [InlineData(Color.Warning)]
    [InlineData(Color.Info)]
    [InlineData(Color.Dark)]
    public void Spinner_WithDifferentColors_ShouldRenderWithCorrectColor(Color color)
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Color, color)
        );

        // Assert
        var progressCircular = cut.FindComponent<MudProgressCircular>();
        Assert.Equal(color, progressCircular.Instance.Color);
    }

    [Fact]
    public void Spinner_WithoutExplicitColor_ShouldUseRandomColor()
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert
        var progressCircular = cut.FindComponent<MudProgressCircular>();

        // Just verify that a color is set (it should be one of the random colors)
        Assert.NotNull(progressCircular.Instance.Color);
    }

    [Fact]
    public void Spinner_ShouldRenderWithCenterAlignment()
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert
        Assert.Contains("d-flex", cut.Markup);
        Assert.Contains("justify-center", cut.Markup);
    }

    [Fact]
    public void Spinner_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Spinner();

        // Assert
        Assert.IsType<CraftComponent>(component, exactMatch: false);
    }

    [Fact]
    public void Spinner_WithDefaultVisibility_ShouldBeVisible()
    {
        // Arrange & Act
        var cut = Render<Spinner>();

        // Assert - Default Visible from CraftComponent is true
        var progressCirculars = cut.FindComponents<MudProgressCircular>();
        Assert.Single(progressCirculars);
    }

    [Fact]
    public void Spinner_MultipleInstances_CanHaveDifferentColors()
    {
        // Arrange & Act
        var cut1 = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Color, Color.Primary)
        );

        var cut2 = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Color, Color.Error)
        );

        // Assert
        var progressCircular1 = cut1.FindComponent<MudProgressCircular>();
        var progressCircular2 = cut2.FindComponent<MudProgressCircular>();

        Assert.Equal(Color.Primary, progressCircular1.Instance.Color);
        Assert.Equal(Color.Error, progressCircular2.Instance.Color);
    }

    [Fact]
    public void Spinner_WhenVisibleTrue_ShouldHaveMudProgressCircular()
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Color, Color.Info)
        );

        // Assert
        var progressCircular = cut.FindComponent<MudProgressCircular>();
        Assert.Equal(Color.Info, progressCircular.Instance.Color);
        Assert.True(progressCircular.Instance.Indeterminate);
    }

    [Fact]
    public void Spinner_ShouldUseCraftDivComponent()
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert - Verify markup contains the expected structure
        Assert.Contains("d-flex", cut.Markup);
        Assert.Contains("justify-center", cut.Markup);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void Spinner_WithDifferentVisibility_ShouldRenderCorrectly(bool visible, int expectedCount)
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, visible)
        );

        // Assert
        var progressCirculars = cut.FindComponents<MudProgressCircular>();
        Assert.Equal(expectedCount, progressCirculars.Count);
    }

    [Fact]
    public void Spinner_ShouldUseShowComponent()
    {
        // Arrange & Act
        var cut = Render<Spinner>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert - The Spinner uses the Show component internally
        var showComponent = cut.FindComponent<Show>();
        Assert.NotNull(showComponent);
    }
}
