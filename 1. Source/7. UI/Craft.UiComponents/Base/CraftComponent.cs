using Craft.UiComponents.Enums;
using Craft.UiComponents.Services;
using Craft.Utilities.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace Craft.UiComponents;

/// <summary>
/// Base component for all Craft UI components providing common functionality
/// including styling, theming, element reference, and basic click handling.
/// </summary>
public abstract class CraftComponent : ComponentBase, IAsyncDisposable
{
    private bool _disposed;
    private bool _previousVisible;

    #region Injected Services

    /// <summary>
    /// Gets the theme service for theme-aware components.
    /// </summary>
    [Inject] protected IThemeService? ThemeService { get; set; }

    /// <summary>
    /// Gets the optional logger for component diagnostics.
    /// </summary>
    [Inject] protected ILogger<CraftComponent>? Logger { get; set; }

    #endregion

    #region Common Parameters

    /// <summary>
    /// Gets or sets the child content to be rendered inside the component.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Gets the element reference for JavaScript interop.
    /// This is set automatically when the component renders.
    /// </summary>
    public ElementReference ElementRef { get; protected set; }

    /// <summary>
    /// Gets or sets the unique identifier for this component.
    /// </summary>
    [Parameter] public string? Id { get; set; }

    /// <summary>
    /// Gets or sets additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// Gets or sets an arbitrary object value to associate with this component.
    /// </summary>
    [Parameter] public object? Tag { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? UserAttributes { get; set; }

    /// <summary>
    /// Gets or sets whether the component is visible.
    /// </summary>
    [Parameter] public virtual bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the component is disabled.
    /// </summary>
    [Parameter] public virtual bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the component is clicked.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    #endregion

    #region Theming Parameters

    /// <summary>
    /// Gets or sets the size variant for this component.
    /// </summary>
    [Parameter] public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>
    /// Gets or sets the visual style variant for this component.
    /// </summary>
    [Parameter] public ComponentVariant Variant { get; set; } = ComponentVariant.Default;

    #endregion

    #region Animation Parameters

    /// <summary>
    /// Gets or sets the animation type for this component.
    /// </summary>
    [Parameter] public AnimationType Animation { get; set; } = AnimationType.None;

    /// <summary>
    /// Gets or sets the animation duration.
    /// </summary>
    [Parameter] public AnimationDuration AnimationDuration { get; set; } = AnimationDuration.Normal;

    /// <summary>
    /// Gets or sets a custom animation duration in milliseconds.
    /// Overrides <see cref="AnimationDuration"/> when set to a positive value.
    /// </summary>
    [Parameter] public int? CustomAnimationDurationMs { get; set; }

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
            .AddClass(Class!, !string.IsNullOrEmpty(Class));

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
            .AddStyle(Style!);

        return builder.NullIfEmpty();
    }

    /// <summary>
    /// Handles theme change events.
    /// </summary>
    protected virtual void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        LogDebug("Theme changed from {Previous} to {New}", e.PreviousTheme, e.NewTheme);
        InvokeAsync(StateHasChanged);
    }

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
    /// Forces the component to re-render.
    /// </summary>
    public void Refresh() => StateHasChanged();

    /// <summary>
    /// Asynchronously forces the component to re-render.
    /// </summary>
    public Task RefreshAsync() => InvokeAsync(StateHasChanged);

    #endregion

    #region Logging Helpers

    protected void LogDebug(string message, params object?[] args) 
        => Logger?.LogDebug(message, args);

    protected void LogInfo(string message, params object?[] args) 
        => Logger?.LogInformation(message, args);

    protected void LogWarning(string message, params object?[] args) 
        => Logger?.LogWarning(message, args);

    protected void LogError(Exception? exception, string message, params object?[] args) 
        => Logger?.LogError(exception, message, args);

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

        LogDebug("Component disposed: {Id}", Id);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Override in derived classes to dispose additional resources.
    /// </summary>
    protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;

    #endregion
}
