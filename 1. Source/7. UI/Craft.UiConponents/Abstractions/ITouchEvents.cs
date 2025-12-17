using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Craft.UiConponents.Abstractions;

/// <summary>
/// Provides touch event handling capabilities.
/// Implement this interface to add touch events to your component.
/// </summary>
public interface ITouchEvents
{
    /// <summary>
    /// Gets or sets the callback invoked when a touch point is placed on the touch surface.
    /// </summary>
    [Parameter] EventCallback<TouchEventArgs> OnTouchStart { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a touch point is removed from the touch surface.
    /// </summary>
    [Parameter] EventCallback<TouchEventArgs> OnTouchEnd { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a touch point moves along the touch surface.
    /// </summary>
    [Parameter] EventCallback<TouchEventArgs> OnTouchMove { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a touch event is canceled.
    /// </summary>
    [Parameter] EventCallback<TouchEventArgs> OnTouchCancel { get; set; }

    /// <summary>
    /// Gets whether the component is disabled.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// Handles the touch start event.
    /// </summary>
    Task HandleTouchStartAsync(TouchEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnTouchStart.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the touch end event.
    /// </summary>
    Task HandleTouchEndAsync(TouchEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnTouchEnd.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the touch move event.
    /// </summary>
    Task HandleTouchMoveAsync(TouchEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnTouchMove.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the touch cancel event.
    /// </summary>
    Task HandleTouchCancelAsync(TouchEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnTouchCancel.InvokeAsync(args);
    }
}
