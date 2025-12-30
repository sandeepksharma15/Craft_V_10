using Microsoft.JSInterop;

namespace Craft.UiComponents.Abstractions;

/// <summary>
/// Interface for components that require JavaScript interop functionality.
/// Provides a standard pattern for JS module loading and disposal.
/// </summary>
public interface IJsComponent : IAsyncDisposable
{
    /// <summary>
    /// Gets the JavaScript module reference for this component.
    /// </summary>
    IJSObjectReference? JsModule { get; }

    /// <summary>
    /// Gets whether the JavaScript module has been initialized.
    /// </summary>
    bool IsJsInitialized { get; }
}
