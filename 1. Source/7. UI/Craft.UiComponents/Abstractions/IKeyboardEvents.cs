using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Craft.UiComponents.Abstractions;

/// <summary>
/// Provides keyboard event handling capabilities.
/// Implement this interface to add keyboard events to your component.
/// </summary>
public interface IKeyboardEvents
{
    /// <summary>
    /// Gets or sets the callback invoked when a key is pressed while the component has focus.
    /// </summary>
    [Parameter] EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a key is released while the component has focus.
    /// </summary>
    [Parameter] EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a key press produces a character while the component has focus.
    /// </summary>
    [Parameter] EventCallback<KeyboardEventArgs> OnKeyPress { get; set; }

    /// <summary>
    /// Gets whether the component is disabled.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// Handles the key down event.
    /// </summary>
    Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnKeyDown.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the key up event.
    /// </summary>
    Task HandleKeyUpAsync(KeyboardEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnKeyUp.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the key press event.
    /// </summary>
    Task HandleKeyPressAsync(KeyboardEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnKeyPress.InvokeAsync(args);
    }
}
