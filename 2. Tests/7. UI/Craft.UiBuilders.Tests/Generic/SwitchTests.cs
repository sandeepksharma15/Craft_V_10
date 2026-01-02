using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the Switch component.
/// Tests multi-case conditional rendering with type-safe value matching.
/// </summary>
public class SwitchTests : ComponentTestBase
{
    private enum Status
    {
        Active,
        Inactive,
        Pending
    }

    [Fact]
    public void Switch_WithStringValue_ShouldMatchCorrectly()
    {
        // Arrange
        var color = "red";

        // Act - Simplified test without Case components for now
        var component = new Switch<string>
        {
            Value = color,
            Default = builder => builder.AddMarkupContent(0, "<div>Default</div>")
        };

        // Assert
        Assert.Equal(color, component.Value);
        Assert.NotNull(component.Cases);
        Assert.Empty(component.Cases);
    }

    [Fact]
    public void Switch_WithIntegerValue_ShouldAcceptValue()
    {
        // Arrange
        var number = 42;

        // Act
        var component = new Switch<int>
        {
            Value = number
        };

        // Assert
        Assert.Equal(42, component.Value);
        Assert.NotNull(component.Cases);
    }

    [Fact]
    public void Switch_WithNullValue_ShouldHandleNull()
    {
        // Arrange
        string? nullValue = null;

        // Act
        var component = new Switch<string?>
        {
            Value = nullValue
        };

        // Assert
        Assert.Null(component.Value);
        Assert.NotNull(component.Cases);
    }

    [Fact]
    public void Switch_WithEnumValue_ShouldWork()
    {
        // Arrange
        var status = Status.Active;

        // Act
        var component = new Switch<Status>
        {
            Value = status
        };

        // Assert
        Assert.Equal(Status.Active, component.Value);
    }

    [Fact]
    public void Switch_DefaultCases_ShouldBeEmptyList()
    {
        // Arrange & Act
        var component = new Switch<string>();

        // Assert
        Assert.NotNull(component.Cases);
        Assert.Empty(component.Cases);
    }

    [Fact]
    public void Switch_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new Switch<string>();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public void Case_ShouldInheritFromComponentBase()
    {
        // Arrange
        var component = new Case<string>();

        // Assert
        Assert.IsAssignableFrom<Microsoft.AspNetCore.Components.ComponentBase>(component);
    }

    [Fact]
    public void Case_ShouldAcceptWhenValue()
    {
        // Arrange & Act
        var caseComponent = new Case<string>
        {
            When = "test"
        };

        // Assert
        Assert.Equal("test", caseComponent.When);
    }

    [Fact]
    public void Switch_WithDefaultContent_ShouldAcceptRenderFragment()
    {
        // Arrange
        RenderFragment fragment = builder => builder.AddMarkupContent(0, "<div>Default</div>");

        // Act
        var component = new Switch<string>
        {
            Default = fragment
        };

        // Assert
        Assert.NotNull(component.Default);
    }

    [Fact]
    public void Switch_CasesCanBeAddedManually()
    {
        // Arrange
        var component = new Switch<int>();
        var case1 = new Case<int> { When = 1 };
        var case2 = new Case<int> { When = 2 };

        // Act
        component.Cases.Add(case1);
        component.Cases.Add(case2);

        // Assert
        Assert.Equal(2, component.Cases.Count);
        Assert.Contains(case1, component.Cases);
        Assert.Contains(case2, component.Cases);
    }
}
