using Bunit;
using Craft.UiBuilders.Components;
using Craft.UiBuilders.Tests.Base;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Tests.Components;

/// <summary>
/// Unit tests for the DarkModeToggle component.
/// Tests focus on component logic and state management.
/// Note: Full UI rendering tests are limited due to MudBlazor's MudPopoverProvider requirement.
/// </summary>
public class DarkModeToggleTests : ComponentTestBase
{
    [Fact]
    public void DarkModeToggle_WithDarkModeFalse_ShouldSetPropertyCorrectly()
    {
        // Arrange
        var component = new DarkModeToggle
        {
            DarkMode = false,
            DarkModeChanged = EventCallback.Factory.Create<bool>(this, _ => { })
        };

        // Act & Assert
        Assert.False(component.DarkMode);
    }

    [Fact]
    public void DarkModeToggle_WithDarkModeTrue_ShouldSetPropertyCorrectly()
    {
        // Arrange
        var component = new DarkModeToggle
        {
            DarkMode = true,
            DarkModeChanged = EventCallback.Factory.Create<bool>(this, _ => { })
        };

        // Act & Assert
        Assert.True(component.DarkMode);
    }

    [Fact]
    public async Task DarkModeToggle_WhenOnDarkModeChangedCalled_ShouldInvokeDarkModeChanged()
    {
        // Arrange
        bool callbackInvoked = false;
        bool receivedValue = false;

        var component = new DarkModeToggle
        {
            DarkMode = false,
            DarkModeChanged = EventCallback.Factory.Create<bool>(this, value =>
            {
                callbackInvoked = true;
                receivedValue = value;
            })
        };

        // Act
        await component.OnDarkModeChanged(true);

        // Assert
        Assert.True(callbackInvoked);
        Assert.True(receivedValue);
    }

    [Fact]
    public async Task DarkModeToggle_WhenOnDarkModeChangedToFalse_ShouldInvokeCallbackWithFalse()
    {
        // Arrange
        bool callbackInvoked = false;
        bool receivedValue = true;

        var component = new DarkModeToggle
        {
            DarkMode = true,
            DarkModeChanged = EventCallback.Factory.Create<bool>(this, value =>
            {
                callbackInvoked = true;
                receivedValue = value;
            })
        };

        // Act
        await component.OnDarkModeChanged(false);

        // Assert
        Assert.True(callbackInvoked);
        Assert.False(receivedValue);
    }

    [Fact]
    public async Task DarkModeToggle_WhenOnDarkModeChangedCalled_ShouldUpdateInternalState()
    {
        // Arrange
        var component = new DarkModeToggle
        {
            DarkMode = false,
            DarkModeChanged = EventCallback.Factory.Create<bool>(this, _ => { })
        };

        // Act
        await component.OnDarkModeChanged(true);

        // Assert
        Assert.True(component.DarkMode);
    }

    [Fact]
    public async Task DarkModeToggle_MultipleOnDarkModeChangedCalls_ShouldUpdateStateCorrectly()
    {
        // Arrange
        var states = new List<bool>();

        var component = new DarkModeToggle
        {
            DarkMode = false,
            DarkModeChanged = EventCallback.Factory.Create<bool>(this, value =>
            {
                states.Add(value);
            })
        };

        // Act
        await component.OnDarkModeChanged(true);
        await component.OnDarkModeChanged(false);
        await component.OnDarkModeChanged(true);

        // Assert
        Assert.Equal(3, states.Count);
        Assert.True(states[0]);
        Assert.False(states[1]);
        Assert.True(states[2]);
        Assert.True(component.DarkMode); // Final state should be true
    }

    [Fact]
    public async Task DarkModeToggle_WithDefaultCallback_ShouldNotThrow()
    {
        // Arrange
        var component = new DarkModeToggle
        {
            DarkMode = false,
            DarkModeChanged = default(EventCallback<bool>)
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
        {
            await component.OnDarkModeChanged(true);
        });

        Assert.Null(exception);
    }

    [Fact]
    public void DarkModeToggle_ShouldInheritFromCraftComponent()
    {
        // Arrange & Act
        var component = new DarkModeToggle();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Fact]
    public void DarkModeToggle_DarkModeParameter_ShouldHaveParameterAttribute()
    {
        // Arrange & Act
        var property = typeof(DarkModeToggle).GetProperty(nameof(DarkModeToggle.DarkMode));
        var parameterAttribute = property?.GetCustomAttributes(typeof(ParameterAttribute), false)
            .Cast<ParameterAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(parameterAttribute);
    }

    [Fact]
    public void DarkModeToggle_DarkModeParameter_ShouldBeRequired()
    {
        // Arrange & Act
        var property = typeof(DarkModeToggle).GetProperty(nameof(DarkModeToggle.DarkMode));
        var requiredAttribute = property?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false)
            .FirstOrDefault();

        // Assert
        Assert.NotNull(requiredAttribute);
    }

    [Fact]
    public void DarkModeToggle_DarkModeChangedParameter_ShouldHaveParameterAttribute()
    {
        // Arrange & Act
        var property = typeof(DarkModeToggle).GetProperty(nameof(DarkModeToggle.DarkModeChanged));
        var parameterAttribute = property?.GetCustomAttributes(typeof(ParameterAttribute), false)
            .Cast<ParameterAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(parameterAttribute);
    }

    [Fact]
    public void DarkModeToggle_DarkModeChangedParameter_ShouldBeRequired()
    {
        // Arrange & Act
        var property = typeof(DarkModeToggle).GetProperty(nameof(DarkModeToggle.DarkModeChanged));
        var requiredAttribute = property?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false)
            .FirstOrDefault();

        // Assert
        Assert.NotNull(requiredAttribute);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DarkModeToggle_WithDifferentInitialStates_ShouldSetStateCorrectly(bool initialDarkMode)
    {
        // Arrange & Act
        var component = new DarkModeToggle
        {
            DarkMode = initialDarkMode,
            DarkModeChanged = EventCallback.Factory.Create<bool>(this, _ => { })
        };

        // Assert
        Assert.Equal(initialDarkMode, component.DarkMode);
    }

    [Fact]
    public async Task DarkModeToggle_AfterOnDarkModeChanged_ShouldMaintainStateConsistency()
    {
        // Arrange
        bool externalState = false;

        var component = new DarkModeToggle
        {
            DarkMode = externalState,
            DarkModeChanged = EventCallback.Factory.Create<bool>(this, value =>
            {
                externalState = value;
            })
        };

        // Act
        await component.OnDarkModeChanged(true);

        // Assert
        Assert.True(component.DarkMode);
        Assert.True(externalState);
    }

    [Fact]
    public void DarkModeToggle_OnDarkModeChangedMethod_ShouldExistAndBePublic()
    {
        // Arrange & Act
        var method = typeof(DarkModeToggle).GetMethod(nameof(DarkModeToggle.OnDarkModeChanged));

        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsPublic);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void DarkModeToggle_OnDarkModeChangedMethod_ShouldTakeBoolParameter()
    {
        // Arrange & Act
        var method = typeof(DarkModeToggle).GetMethod(nameof(DarkModeToggle.OnDarkModeChanged));
        var parameters = method?.GetParameters();

        // Assert
        Assert.NotNull(parameters);
        Assert.Single(parameters);
        Assert.Equal(typeof(bool), parameters[0].ParameterType);
    }

    [Fact]
    public void DarkModeToggle_DarkModeProperty_ShouldBePublicAndAccessible()
    {
        // Arrange & Act
        var property = typeof(DarkModeToggle).GetProperty(nameof(DarkModeToggle.DarkMode));

        // Assert
        Assert.NotNull(property);
        Assert.True(property.CanRead);
        Assert.True(property.CanWrite);
        Assert.Equal(typeof(bool), property.PropertyType);
    }

    [Fact]
    public void DarkModeToggle_DarkModeChangedProperty_ShouldBePublicAndAccessible()
    {
        // Arrange & Act
        var property = typeof(DarkModeToggle).GetProperty(nameof(DarkModeToggle.DarkModeChanged));

        // Assert
        Assert.NotNull(property);
        Assert.True(property.CanRead);
        Assert.True(property.CanWrite);
        Assert.Equal(typeof(EventCallback<bool>), property.PropertyType);
    }

    [Fact]
    public async Task DarkModeToggle_OnDarkModeChanged_ShouldBeAsyncMethod()
    {
        // Arrange
        bool callbackCompleted = false;
        var tcs = new TaskCompletionSource<bool>();

        var component = new DarkModeToggle
        {
            DarkMode = false,
            DarkModeChanged = EventCallback.Factory.Create<bool>(this, async value =>
            {
                await Task.Delay(10); // Simulate async work
                callbackCompleted = true;
                tcs.SetResult(value);
            })
        };

        // Act
        var task = component.OnDarkModeChanged(true);
        await task;
        await tcs.Task;

        // Assert
        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(callbackCompleted);
    }
}

