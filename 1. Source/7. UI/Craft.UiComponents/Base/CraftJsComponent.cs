using Craft.UiComponents.Abstractions;
using Craft.UiComponents.Features;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Craft.UiComponents;

/// <summary>
/// Base component for interactive UI components that require JavaScript interop functionality.
/// Uses composition via <see cref="JsModuleHandler"/> for JS module management.
/// </summary>
public abstract class CraftJsComponent : CraftInteractiveComponent, IJsComponent
{
    private JsModuleHandler? _jsHandler;

    #region Injected Services

    /// <summary>
    /// Gets the JavaScript runtime for interop operations.
    /// </summary>
    [Inject] protected IJSRuntime JsRuntime { get; set; } = default!;

    #endregion

    #region JavaScript Interop Properties

    /// <summary>
    /// Gets the JavaScript module reference for this component.
    /// </summary>
    public IJSObjectReference? JsModule => _jsHandler?.JsModule;

    /// <summary>
    /// Gets whether the JavaScript module has been initialized.
    /// </summary>
    public bool IsJsInitialized => _jsHandler?.IsJsInitialized ?? false;

    /// <summary>
    /// Gets the path to the JavaScript module for this component.
    /// Override in derived classes to specify a custom module path.
    /// </summary>
    protected virtual string? JsModulePath => null;

    /// <summary>
    /// Gets the JavaScript module handler for advanced operations.
    /// </summary>
    protected JsModuleHandler? JsHandler => _jsHandler;

    #endregion

    #region Lifecycle Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && !string.IsNullOrEmpty(JsModulePath))
            await InitializeJsModuleAsync();
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Initializes the JavaScript module for this component.
    /// </summary>
    protected virtual async Task InitializeJsModuleAsync()
    {
        if (string.IsNullOrEmpty(JsModulePath) || _jsHandler is not null)
            return;

        _jsHandler = new JsModuleHandler(JsRuntime, JsModulePath, LogDebug, LogWarning, LogError);

        await _jsHandler.InitializeAsync(this);
        await OnJsModuleInitializedAsync();
    }

    /// <summary>
    /// Called after the JavaScript module has been initialized.
    /// Override in derived classes to perform additional setup.
    /// </summary>
    protected virtual Task OnJsModuleInitializedAsync() => Task.CompletedTask;

    /// <summary>
    /// Invokes a JavaScript function from the loaded module.
    /// </summary>
    /// <param name="identifier">The function identifier.</param>
    /// <param name="args">The arguments to pass to the function.</param>
    protected ValueTask InvokeJsVoidAsync(string identifier, params object?[] args)
        => _jsHandler?.InvokeVoidAsync(identifier, args) ?? ValueTask.CompletedTask;

    /// <summary>
    /// Invokes a JavaScript function from the loaded module and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="identifier">The function identifier.</param>
    /// <param name="args">The arguments to pass to the function.</param>
    /// <returns>The result from the JavaScript function.</returns>
    protected ValueTask<TResult> InvokeJsAsync<TResult>(string identifier, params object?[] args)
        => _jsHandler?.InvokeAsync<TResult>(identifier, args) ?? ValueTask.FromResult<TResult>(default!);

    #endregion

    #region Disposal

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore()
    {
        if (_jsHandler is not null)
            await _jsHandler.DisposeAsync();

        await base.DisposeAsyncCore();
    }

    #endregion
}
