using Craft.UiConponents.Abstractions;
using Microsoft.JSInterop;

namespace Craft.UiConponents.Features;

/// <summary>
/// Provides JavaScript module management capabilities for components.
/// Use this as a composable feature for components that need JS interop.
/// </summary>
public sealed class JsModuleHandler : IJsComponent, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string _modulePath;
    private readonly Action<string, object?[]>? _logDebug;
    private readonly Action<Exception?, string, object?[]>? _logError;
    private readonly Action<string, object?[]>? _logWarning;

    /// <summary>
    /// Gets the JavaScript module reference.
    /// </summary>
    public IJSObjectReference? JsModule { get; private set; }

    /// <summary>
    /// Gets whether the JavaScript module has been initialized.
    /// </summary>
    public bool IsJsInitialized => JsModule is not null;

    /// <summary>
    /// Gets or sets the .NET object reference for JavaScript callbacks.
    /// </summary>
    public DotNetObjectReference<object>? DotNetRef { get; private set; }

    /// <summary>
    /// Initializes a new instance of <see cref="JsModuleHandler"/>.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime.</param>
    /// <param name="modulePath">The path to the JavaScript module.</param>
    /// <param name="logDebug">Optional debug logging action.</param>
    /// <param name="logWarning">Optional warning logging action.</param>
    /// <param name="logError">Optional error logging action.</param>
    public JsModuleHandler(
        IJSRuntime jsRuntime,
        string modulePath,
        Action<string, object?[]>? logDebug = null,
        Action<string, object?[]>? logWarning = null,
        Action<Exception?, string, object?[]>? logError = null)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _modulePath = modulePath ?? throw new ArgumentNullException(nameof(modulePath));
        _logDebug = logDebug;
        _logWarning = logWarning;
        _logError = logError;
    }

    /// <summary>
    /// Initializes the JavaScript module.
    /// </summary>
    /// <typeparam name="T">The type for the DotNetObjectReference.</typeparam>
    /// <param name="dotNetRefTarget">The target object for .NET callbacks from JavaScript.</param>
    public async Task InitializeAsync<T>(T dotNetRefTarget) where T : class
    {
        if (JsModule is not null)
            return;

        try
        {
            JsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", _modulePath);
            DotNetRef = DotNetObjectReference.Create<object>(dotNetRefTarget);

            _logDebug?.Invoke("JS module initialized: {ModulePath}", [_modulePath]);
        }
        catch (JSException ex)
        {
            _logError?.Invoke(ex, "Failed to initialize JS module: {ModulePath}", [_modulePath]);
            throw;
        }
    }

    /// <summary>
    /// Initializes the JavaScript module without a .NET reference.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (JsModule is not null)
            return;

        try
        {
            JsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", _modulePath);

            _logDebug?.Invoke("JS module initialized: {ModulePath}", [_modulePath]);
        }
        catch (JSException ex)
        {
            _logError?.Invoke(ex, "Failed to initialize JS module: {ModulePath}", [_modulePath]);
            throw;
        }
    }

    /// <summary>
    /// Invokes a JavaScript function from the loaded module.
    /// </summary>
    /// <param name="identifier">The function identifier.</param>
    /// <param name="args">The arguments to pass to the function.</param>
    public async ValueTask InvokeVoidAsync(string identifier, params object?[] args)
    {
        if (JsModule is null)
        {
            _logWarning?.Invoke("Cannot invoke JS function '{Identifier}': module not initialized", [identifier]);
            return;
        }

        try
        {
            await JsModule.InvokeVoidAsync(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
        catch (JSException ex)
        {
            _logError?.Invoke(ex, "Error invoking JS function: {Identifier}", [identifier]);
            throw;
        }
    }

    /// <summary>
    /// Invokes a JavaScript function from the loaded module and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="identifier">The function identifier.</param>
    /// <param name="args">The arguments to pass to the function.</param>
    /// <returns>The result from the JavaScript function.</returns>
    public async ValueTask<TResult> InvokeAsync<TResult>(string identifier, params object?[] args)
    {
        if (JsModule is null)
        {
            _logWarning?.Invoke("Cannot invoke JS function '{Identifier}': module not initialized", [identifier]);
            return default!;
        }

        try
        {
            return await JsModule.InvokeAsync<TResult>(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            return default!;
        }
        catch (JSException ex)
        {
            _logError?.Invoke(ex, "Error invoking JS function: {Identifier}", [identifier]);
            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        DotNetRef?.Dispose();

        if (JsModule is not null)
        {
            try
            {
                await JsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
        }

        GC.SuppressFinalize(this);
    }
}
