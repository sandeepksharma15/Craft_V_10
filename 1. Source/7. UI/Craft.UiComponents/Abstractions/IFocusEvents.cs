using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Craft.UiComponents.Abstractions;

/// <summary>
/// Provides focus event handling capabilities.
/// Implement this interface to add focus events to your component.
/// </summary>
public interface IFocusEvents
{
    /// <summary>
    /// Gets or sets the callback invoked when the component receives focus.
    /// </summary>
    [Parameter] EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the component loses focus.
    /// </summary>
    [Parameter] EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Handles the focus event.
    /// </summary>
    Task HandleFocusAsync(FocusEventArgs args) => OnFocus.InvokeAsync(args);

    /// <summary>
    /// Handles the blur event.
    /// </summary>
    Task HandleBlurAsync(FocusEventArgs args) => OnBlur.InvokeAsync(args);
}
