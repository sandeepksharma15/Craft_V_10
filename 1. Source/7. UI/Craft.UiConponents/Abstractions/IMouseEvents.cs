using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Craft.UiConponents.Abstractions;

/// <summary>
/// Provides extended mouse event handling capabilities.
/// Implement this interface to add mouse events to your component.
/// </summary>
public interface IMouseEvents
{
    /// <summary>
    /// Gets or sets the callback invoked when the component is double-clicked.
    /// </summary>
    [Parameter] EventCallback<MouseEventArgs> OnDoubleClick { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the mouse enters the component.
    /// </summary>
    [Parameter] EventCallback<MouseEventArgs> OnMouseEnter { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the mouse leaves the component.
    /// </summary>
    [Parameter] EventCallback<MouseEventArgs> OnMouseLeave { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a mouse button is pressed on the component.
    /// </summary>
    [Parameter] EventCallback<MouseEventArgs> OnMouseDown { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a mouse button is released on the component.
    /// </summary>
    [Parameter] EventCallback<MouseEventArgs> OnMouseUp { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the mouse moves over the component.
    /// </summary>
    [Parameter] EventCallback<MouseEventArgs> OnMouseMove { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a context menu is requested.
    /// </summary>
    [Parameter] EventCallback<MouseEventArgs> OnContextMenu { get; set; }

    /// <summary>
    /// Gets whether the component is disabled.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// Handles the double-click event.
    /// </summary>
    Task HandleDoubleClickAsync(MouseEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnDoubleClick.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the mouse enter event.
    /// </summary>
    Task HandleMouseEnterAsync(MouseEventArgs args) => OnMouseEnter.InvokeAsync(args);

    /// <summary>
    /// Handles the mouse leave event.
    /// </summary>
    Task HandleMouseLeaveAsync(MouseEventArgs args) => OnMouseLeave.InvokeAsync(args);

    /// <summary>
    /// Handles the mouse down event.
    /// </summary>
    Task HandleMouseDownAsync(MouseEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnMouseDown.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the mouse up event.
    /// </summary>
    Task HandleMouseUpAsync(MouseEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnMouseUp.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the mouse move event.
    /// </summary>
    Task HandleMouseMoveAsync(MouseEventArgs args) => OnMouseMove.InvokeAsync(args);

    /// <summary>
    /// Handles the context menu event.
    /// </summary>
    Task HandleContextMenuAsync(MouseEventArgs args) => OnContextMenu.InvokeAsync(args);
}
