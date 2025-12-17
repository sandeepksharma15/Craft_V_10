using Craft.UiConponents.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Craft.UiConponents;

/// <summary>
/// Base component for interactive UI components that handle extended user events.
/// Implements multiple event interfaces to provide comprehensive event handling.
/// </summary>
public abstract class CraftInteractiveComponent : CraftComponent, IMouseEvents, IKeyboardEvents, IFocusEvents, ITouchEvents
{
    #region IMouseEvents Implementation

    /// <inheritdoc />
    [Parameter] public EventCallback<MouseEventArgs> OnDoubleClick { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<MouseEventArgs> OnMouseEnter { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<MouseEventArgs> OnMouseLeave { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<MouseEventArgs> OnMouseDown { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<MouseEventArgs> OnMouseUp { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<MouseEventArgs> OnMouseMove { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<MouseEventArgs> OnContextMenu { get; set; }

    #endregion

    #region IFocusEvents Implementation

    /// <inheritdoc />
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    #endregion

    #region IKeyboardEvents Implementation

    /// <inheritdoc />
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyPress { get; set; }

    #endregion

    #region ITouchEvents Implementation

    /// <inheritdoc />
    [Parameter] public EventCallback<TouchEventArgs> OnTouchStart { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<TouchEventArgs> OnTouchEnd { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<TouchEventArgs> OnTouchMove { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<TouchEventArgs> OnTouchCancel { get; set; }

    #endregion
}
