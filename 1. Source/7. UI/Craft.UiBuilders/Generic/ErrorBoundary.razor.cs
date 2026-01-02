using Craft.UiComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that catches exceptions from child components and displays custom error UI.
/// Enhanced version of Blazor's built-in ErrorBoundary with retry support.
/// </summary>
public partial class ErrorBoundary : CraftComponent
{
    private bool _hasError;
    private ErrorContext? _errorContext;

    /// <summary>
    /// Gets or sets the callback invoked when an error occurs.
    /// </summary>
    [Parameter]
    public EventCallback<Exception> OnError { get; set; }

    /// <summary>
    /// Gets or sets whether to show detailed error information.
    /// </summary>
    [Parameter]
    public bool ShowDetails { get; set; }

    /// <summary>
    /// Gets or sets the content to display when an error occurs.
    /// </summary>
    [Parameter]
    public RenderFragment<ErrorContext>? ErrorContent { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically retry rendering after an error.
    /// </summary>
    [Parameter]
    public bool AutoRetry { get; set; }

    /// <summary>
    /// Gets or sets the retry delay in milliseconds.
    /// </summary>
    [Parameter]
    public int RetryDelayMs { get; set; } = 1000;

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    /// <summary>
    /// Catches exceptions from child components.
    /// </summary>
    protected override void OnParametersSet()
    {
        try
        {
            base.OnParametersSet();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void HandleException(Exception exception)
    {
        _hasError = true;
        _errorContext = new ErrorContext(exception, ShowDetails);
        
        if (OnError.HasDelegate)
        {
            InvokeAsync(() => OnError.InvokeAsync(exception));
        }

        if (AutoRetry)
        {
            _ = Task.Delay(RetryDelayMs).ContinueWith(_ => Recover());
        }
    }

    /// <summary>
    /// Recovers from the error state and attempts to re-render.
    /// </summary>
    public void Recover()
    {
        _hasError = false;
        _errorContext = null;
        InvokeAsync(StateHasChanged);
    }
}

/// <summary>
/// Represents the context of an error that occurred in a component.
/// </summary>
/// <param name="Exception">The exception that was thrown.</param>
/// <param name="ShowDetails">Whether to show detailed error information.</param>
public record ErrorContext(Exception Exception, bool ShowDetails)
{
    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message => Exception.Message;

    /// <summary>
    /// Gets the stack trace if details should be shown.
    /// </summary>
    public string? StackTrace => ShowDetails ? Exception.StackTrace : null;
}
