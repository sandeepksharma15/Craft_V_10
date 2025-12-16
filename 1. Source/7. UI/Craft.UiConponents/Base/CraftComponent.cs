using Craft.UiConponents.Abstractions;
using Craft.UiConponents.Enums;
using Craft.UiConponents.Services;
using Craft.Utilities.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Craft.UiConponents;

/// <summary>
/// Base component for all Craft UI components providing common functionality
/// including styling, theming, event handling, animations, and JavaScript interop.
/// </summary>
public abstract class CraftComponent : ComponentBase, IJsComponent, IAsyncDisposable
{
    private bool _disposed;
    private bool _previousVisible;
    private ElementReference _elementRef;

    #region Injected Services

    /// <summary>
    /// Gets the JavaScript runtime for interop operations.
    /// </summary>
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;

    /// <summary>
    /// Gets the theme service for theme-aware components.
    /// </summary>
    [Inject]
    protected IThemeService? ThemeService { get; set; }

    /// <summary>
    /// Gets the optional logger for component diagnostics.
    /// </summary>
    [Inject]
    protected ILogger<CraftComponent>? Logger { get; set; }

    #endregion

    #region Common Parameters

    /// <summary>
    /// Gets or sets the child content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the component.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the element reference for JavaScript interop.
    /// </summary>
    [Parameter]
    public ElementReference ElementRef
    {
        get => _elementRef;
        set
        {
            if (!_elementRef.Equals(value))
            {
                _elementRef = value;
                ElementRefChanged.InvokeAsync(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the callback invoked when the element reference changes.
    /// </summary>
    [Parameter]
    public EventCallback<ElementReference> ElementRefChanged { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this component.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets additional inline styles to apply to the component.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Gets or sets an arbitrary object value to associate with this component.
    /// </summary>
    [Parameter]
    public object? Tag { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? UserAttributes { get; set; }

    /// <summary>
    /// Gets or sets whether the component is visible.
    /// </summary>
    [Parameter]
    public virtual bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the component is disabled.
    /// </summary>
    [Parameter]
    public virtual bool Disabled { get; set; }

    #endregion

    #region Theming Parameters

    /// <summary>
    /// Gets or sets the size variant for this component.
    /// </summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>
    /// Gets or sets the visual style variant for this component.
    /// </summary>
    [Parameter]
    public ComponentVariant Variant { get; set; } = ComponentVariant.Default;

    #endregion

    #region Animation Parameters

    /// <summary>
    /// Gets or sets the animation type for this component.
    /// </summary>
    [Parameter]
    public AnimationType Animation { get; set; } = AnimationType.None;

    /// <summary>
    /// Gets or sets the animation duration.
    /// </summary>
    [Parameter]
    public AnimationDuration AnimationDuration { get; set; } = AnimationDuration.Normal;

    /// <summary>
    /// Gets or sets a custom animation duration in milliseconds.
    /// Overrides <see cref="AnimationDuration"/> when set to a positive value.
    /// </summary>
    [Parameter]
    public int? CustomAnimationDurationMs { get; set; }

    #endregion

    #region Event Callbacks

    /// <summary>
    /// Gets or sets the callback invoked when the component is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the component is double-clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnDoubleClick { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the mouse enters the component.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnMouseEnter { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the mouse leaves the component.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnMouseLeave { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a mouse button is pressed on the component.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnMouseDown { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a mouse button is released on the component.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnMouseUp { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the mouse moves over the component.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnMouseMove { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the component receives focus.
    /// </summary>
    [Parameter]
    public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the component loses focus.
    /// </summary>
    [Parameter]
    public EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a key is pressed while the component has focus.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a key is released while the component has focus.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a key press produces a character while the component has focus.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyPress { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a touch point is placed on the touch surface.
    /// </summary>
    [Parameter]
    public EventCallback<TouchEventArgs> OnTouchStart { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a touch point is removed from the touch surface.
    /// </summary>
    [Parameter]
    public EventCallback<TouchEventArgs> OnTouchEnd { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a touch point moves along the touch surface.
    /// </summary>
    [Parameter]
    public EventCallback<TouchEventArgs> OnTouchMove { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a context menu is requested.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnContextMenu { get; set; }

    #endregion

    #region JavaScript Interop

    /// <summary>
    /// Gets the JavaScript module reference for this component.
    /// </summary>
    public IJSObjectReference? JsModule { get; private set; }

    /// <summary>
    /// Gets whether the JavaScript module has been initialized.
    /// </summary>
    public bool IsJsInitialized => JsModule is not null;

    /// <summary>
    /// Gets the path to the JavaScript module for this component.
    /// Override in derived classes to specify a custom module path.
    /// </summary>
    protected virtual string? JsModulePath => null;

    /// <summary>
    /// Gets the .NET object reference for JavaScript callbacks.
    /// </summary>
    protected DotNetObjectReference<CraftComponent>? DotNetRef { get; private set; }

    #endregion

    #region Lifecycle Methods

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Id ??= GenerateId();
        _previousVisible = Visible;

        ThemeService?.ThemeChanged += OnThemeChanged;

        LogDebug("Component initialized with Id: {Id}", Id);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(JsModulePath))
            await InitializeJsModuleAsync();
    }

    /// <inheritdoc />
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        if (Visible != _previousVisible)
        {
            _previousVisible = Visible;
            LogDebug("Visibility changed to: {Visible}", Visible);
            StateHasChanged();
        }
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Generates a unique identifier for this component.
    /// </summary>
    /// <returns>A unique identifier string.</returns>
    protected virtual string GenerateId() => $"craft-{Guid.NewGuid():N}"[..16];

    /// <summary>
    /// Gets the component's ID, prioritizing the user-defined "id" attribute over the default Id property.
    /// </summary>
    /// <returns>The component's ID as a string.</returns>
    protected internal string GetId()
    {
        if (UserAttributes?.TryGetValue("id", out var id) == true)
        {
            var idString = Convert.ToString(id);

            if (!string.IsNullOrEmpty(idString))
                return idString;
        }

        return Id ?? string.Empty;
    }

    /// <summary>
    /// Builds the CSS class string for the component.
    /// </summary>
    /// <returns>The combined CSS class string.</returns>
    protected virtual string BuildCssClass()
    {
        var builder = new CssBuilder(GetComponentCssClass() ?? string.Empty)
            .AddClass(GetSizeCssClass()!, !string.IsNullOrEmpty(GetSizeCssClass()))
            .AddClass(GetVariantCssClass()!, !string.IsNullOrEmpty(GetVariantCssClass()))
            .AddClass(GetAnimationCssClass()!, !string.IsNullOrEmpty(GetAnimationCssClass()))
            .AddClass("craft-disabled", Disabled)
            .AddClass("craft-hidden", !Visible)
            .AddClass(Class ?? String.Empty, !string.IsNullOrEmpty(Class));

        return builder.Build();
    }

    /// <summary>
    /// Gets the base CSS class for this component type.
    /// Override in derived classes.
    /// </summary>
    protected virtual string? GetComponentCssClass() => null;

    /// <summary>
    /// Gets the CSS class for the current size setting.
    /// </summary>
    protected virtual string? GetSizeCssClass() => Size switch
    {
        ComponentSize.ExtraSmall => "craft-size-xs",
        ComponentSize.Small => "craft-size-sm",
        ComponentSize.Medium => null,
        ComponentSize.Large => "craft-size-lg",
        ComponentSize.ExtraLarge => "craft-size-xl",
        _ => null
    };

    /// <summary>
    /// Gets the CSS class for the current variant setting.
    /// </summary>
    protected virtual string? GetVariantCssClass() => Variant switch
    {
        ComponentVariant.Default => null,
        ComponentVariant.Primary => "craft-variant-primary",
        ComponentVariant.Secondary => "craft-variant-secondary",
        ComponentVariant.Success => "craft-variant-success",
        ComponentVariant.Warning => "craft-variant-warning",
        ComponentVariant.Danger => "craft-variant-danger",
        ComponentVariant.Info => "craft-variant-info",
        ComponentVariant.Light => "craft-variant-light",
        ComponentVariant.Dark => "craft-variant-dark",
        ComponentVariant.Outlined => "craft-variant-outlined",
        ComponentVariant.Text => "craft-variant-text",
        _ => null
    };

    /// <summary>
    /// Gets the CSS class for the current animation setting.
    /// </summary>
    protected virtual string? GetAnimationCssClass() => Animation switch
    {
        AnimationType.None => null,
        AnimationType.Fade => "craft-animate-fade",
        AnimationType.Slide => "craft-animate-slide",
        AnimationType.Scale => "craft-animate-scale",
        AnimationType.Collapse => "craft-animate-collapse",
        AnimationType.Bounce => "craft-animate-bounce",
        AnimationType.Shake => "craft-animate-shake",
        AnimationType.Pulse => "craft-animate-pulse",
        AnimationType.Flip => "craft-animate-flip",
        AnimationType.Rotate => "craft-animate-rotate",
        _ => null
    };

    /// <summary>
    /// Builds the inline style string for the component.
    /// </summary>
    /// <returns>The combined style string.</returns>
    protected virtual string? BuildStyle()
    {
        var duration = CustomAnimationDurationMs ?? (int)AnimationDuration;
        var hasAnimationDuration = duration > 0 && Animation != AnimationType.None;

        var builder = StyleBuilder.Empty()
            .AddStyle("--craft-animation-duration", $"{duration}ms", hasAnimationDuration)
            .AddStyle(Style ?? String.Empty);

        return builder.NullIfEmpty();
    }

    /// <summary>
    /// Initializes the JavaScript module for this component.
    /// </summary>
    protected virtual async Task InitializeJsModuleAsync()
    {
        if (string.IsNullOrEmpty(JsModulePath) || JsModule is not null)
            return;

        try
        {
            JsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", JsModulePath);
            DotNetRef = DotNetObjectReference.Create(this);

            LogDebug("JS module initialized: {ModulePath}", JsModulePath);
            await OnJsModuleInitializedAsync();
        }
        catch (JSException ex)
        {
            LogError(ex, "Failed to initialize JS module: {ModulePath}", JsModulePath);
        }
    }

    /// <summary>
    /// Called after the JavaScript module has been initialized.
    /// Override in derived classes to perform additional setup.
    /// </summary>
    protected virtual Task OnJsModuleInitializedAsync() => Task.CompletedTask;

    /// <summary>
    /// Handles theme change events.
    /// </summary>
    protected virtual void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        LogDebug("Theme changed from {Previous} to {New}", e.PreviousTheme, e.NewTheme);
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Forces the component to re-render.
    /// </summary>
    public void Refresh() => StateHasChanged();

    /// <summary>
    /// Asynchronously forces the component to re-render.
    /// </summary>
    public Task RefreshAsync() => InvokeAsync(StateHasChanged);

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the click event.
    /// </summary>
    protected virtual async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled)
            return;

        await OnClick.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the double-click event.
    /// </summary>
    protected virtual async Task HandleDoubleClickAsync(MouseEventArgs args)
    {
        if (Disabled)
            return;

        await OnDoubleClick.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the focus event.
    /// </summary>
    protected virtual Task HandleFocusAsync(FocusEventArgs args) => OnFocus.InvokeAsync(args);

    /// <summary>
    /// Handles the blur event.
    /// </summary>
    protected virtual Task HandleBlurAsync(FocusEventArgs args) => OnBlur.InvokeAsync(args);

    /// <summary>
    /// Handles the key down event.
    /// </summary>
    protected virtual Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnKeyDown.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the key up event.
    /// </summary>
    protected virtual Task HandleKeyUpAsync(KeyboardEventArgs args)
    {
        if (Disabled)
            return Task.CompletedTask;

        return OnKeyUp.InvokeAsync(args);
    }

    #endregion

    #region Logging Helpers

    /// <summary>
    /// Logs a debug message if logging is enabled.
    /// </summary>
    protected void LogDebug(string message, params object?[] args)
    {
        Logger?.LogDebug(message, args);
    }

    /// <summary>
    /// Logs an information message if logging is enabled.
    /// </summary>
    protected void LogInfo(string message, params object?[] args)
    {
        Logger?.LogInformation(message, args);
    }

    /// <summary>
    /// Logs a warning message if logging is enabled.
    /// </summary>
    protected void LogWarning(string message, params object?[] args)
    {
        Logger?.LogWarning(message, args);
    }

    /// <summary>
    /// Logs an error message if logging is enabled.
    /// </summary>
    protected void LogError(Exception? exception, string message, params object?[] args)
    {
        Logger?.LogError(exception, message, args);
    }

    #endregion

    #region IAsyncDisposable

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        ThemeService?.ThemeChanged -= OnThemeChanged;

        await DisposeAsyncCore();

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

        LogDebug("Component disposed: {Id}", Id);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Override in derived classes to dispose additional resources.
    /// </summary>
    protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;

    #endregion
}
