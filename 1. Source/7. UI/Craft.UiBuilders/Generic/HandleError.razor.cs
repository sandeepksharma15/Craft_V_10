using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace Craft.Components.Generic;

/// <summary>
/// A production-ready error handling component that wraps child content with error boundary protection.
/// Provides error logging, user-friendly error display, and navigation to error pages.
/// </summary>
/// <remarks>
/// This component leverages Craft framework's generic components (<see cref="Craft.UiBuilders.Generic.If"/>, 
/// <see cref="Craft.UiBuilders.Generic.Show"/>) for declarative conditional rendering, making the markup
/// more readable and maintainable while following Craft framework conventions.
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// &lt;HandleError ErrorPageUri="/error" ShowDetails="@IsDevelopment"&gt;
///     &lt;YourComponent /&gt;
/// &lt;/HandleError&gt;
/// </code>
/// </example>
public partial class HandleError : ComponentBase
{
    private ErrorBoundary? _errorBoundary;
    private string? _currentErrorId;
    private Exception? _currentException;

    /// <summary>
    /// Gets or sets the logger for recording errors.
    /// </summary>
    [Inject] protected ILogger<HandleError> Logger { get; set; } = default!;

    /// <summary>
    /// Gets or sets the navigation manager for redirecting to error pages.
    /// </summary>
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets the child content to be rendered with error boundary protection.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the URI of the error page to navigate to when an error occurs.
    /// If not specified, errors will be displayed inline.
    /// </summary>
    [Parameter] public string? ErrorPageUri { get; set; }

    /// <summary>
    /// Gets or sets whether to display detailed error information including stack traces.
    /// Should be false in production environments.
    /// </summary>
    [Parameter] public bool ShowDetails { get; set; }

    /// <summary>
    /// Gets or sets custom error content to display instead of the default error UI.
    /// </summary>
    [Parameter] public RenderFragment? CustomErrorContent { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when an error is caught.
    /// </summary>
    [Parameter] public EventCallback<Exception> OnError { get; set; }

    /// <summary>
    /// Processes an error with logging and optional navigation to an error page.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="customMessage">Optional custom message to display instead of the exception message.</param>
    public void ProcessError(Exception exception, string? customMessage = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var errorId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        _currentErrorId = errorId;
        _currentException = exception;

        Logger.LogError(exception,
            "Error caught in HandleError component. Type: {ExceptionType}, Error ID: {ErrorId}",
            exception.GetType().Name, errorId);

        var message = customMessage ?? exception.Message;
        var fullMessage = $"{message} [Error ID: {errorId}]";

        if (!string.IsNullOrWhiteSpace(ErrorPageUri))
        {
            var encodedMessage = Uri.EscapeDataString(fullMessage);
            NavigationManager.NavigateTo($"{ErrorPageUri}?message={encodedMessage}&id={errorId}", forceLoad: true);
        }
    }

    /// <summary>
    /// Handles errors caught by the ErrorBoundary component.
    /// </summary>
    private void HandleErrorInternal(Exception? exception)
    {
        if (exception is null)
            return;

        var errorId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        _currentErrorId = errorId;
        _currentException = exception;

        Logger.LogError(exception,
            "Unhandled error caught by HandleError boundary. Type: {ExceptionType}, Error ID: {ErrorId}",
            exception.GetType().Name, errorId);

        if (OnError.HasDelegate)
            InvokeAsync(() => OnError.InvokeAsync(exception));
    }

    /// <summary>
    /// Gets a user-friendly error message from the exception.
    /// </summary>
    private string GetErrorMessage(Exception? exception)
    {
        if (exception is null)
            return "An unexpected error occurred. Please try again later.";

        var errorId = _currentErrorId ?? Activity.Current?.Id ?? "Unknown";
        var message = ShowDetails
            ? exception.Message
            : "An unexpected error occurred. Please try again later.";

        return $"{message} [Error ID: {errorId}]";
    }

    /// <summary>
    /// Navigates to the configured error page.
    /// </summary>
    private void NavigateToErrorPage()
    {
        if (string.IsNullOrWhiteSpace(ErrorPageUri) || _currentException is null)
            return;

        var errorId = _currentErrorId ?? "Unknown";
        var message = _currentException.Message;
        var encodedMessage = Uri.EscapeDataString(message);

        NavigationManager.NavigateTo($"{ErrorPageUri}?message={encodedMessage}&id={errorId}", forceLoad: true);
    }

    /// <summary>
    /// Reloads the current page.
    /// </summary>
    private void ReloadPage() 
        => NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);

    /// <summary>
    /// Recovers from the error state and attempts to re-render the component.
    /// </summary>
    public void Recover()
    {
        _errorBoundary?.Recover();
        _currentErrorId = null;
        _currentException = null;
    }
}
