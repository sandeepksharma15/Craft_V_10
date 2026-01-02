using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that catches exceptions from child components and displays custom error UI.
/// Enhanced version of Blazor's built-in ErrorBoundary with retry support and custom callbacks.
/// Renamed from ErrorBoundary to CraftErrorBoundary to avoid conflicts with Microsoft.AspNetCore.Components.Web.ErrorBoundary.
/// </summary>
public partial class CraftErrorBoundary : Microsoft.AspNetCore.Components.Web.ErrorBoundary
{
    private CraftErrorContext? _craftErrorContext;

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
    /// If not specified, the base ErrorBoundary's ErrorContent will be used.
    /// </summary>
    [Parameter]
    public RenderFragment<CraftErrorContext>? CraftErrorContent { get; set; }

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

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    [Parameter]
    public int MaxRetryAttempts { get; set; } = 3;

    private int _retryCount;

    /// <summary>
    /// Called when an error is caught by this error boundary.
    /// </summary>
    protected override async Task OnErrorAsync(Exception exception)
    {
        await base.OnErrorAsync(exception);

        _craftErrorContext = new CraftErrorContext(exception, ShowDetails);

        if (OnError.HasDelegate)
        {
            await OnError.InvokeAsync(exception);
        }

        if (AutoRetry && _retryCount < MaxRetryAttempts)
        {
            _retryCount++;
            await Task.Delay(RetryDelayMs);
            await RecoverAsync();
        }
    }

    /// <summary>
    /// Recovers from the error state and attempts to re-render.
    /// </summary>
    public async Task RecoverAsync()
    {
        _craftErrorContext = null;
        Recover();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Resets the retry counter.
    /// </summary>
    public void ResetRetryCount()
    {
        _retryCount = 0;
    }
}

/// <summary>
/// Represents the context of an error that occurred in a component.
/// </summary>
/// <param name="Exception">The exception that was thrown.</param>
/// <param name="ShowDetails">Whether to show detailed error information.</param>
public record CraftErrorContext(Exception Exception, bool ShowDetails)
{
    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message => Exception.Message;

    /// <summary>
    /// Gets the stack trace if details should be shown.
    /// </summary>
    public string? StackTrace => ShowDetails ? Exception.StackTrace : null;

    /// <summary>
    /// Gets the full exception details if details should be shown.
    /// </summary>
    public string? FullDetails => ShowDetails ? Exception.ToString() : null;
}
