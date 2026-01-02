using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Craft.UiBuilders.Extensions;

/// <summary>
/// Extension methods for <see cref="NavigationManager"/> to provide browser navigation functionality.
/// </summary>
public static class NavigationExtensions
{
    /// <summary>
    /// Navigates back to the previous page in the browser history.
    /// </summary>
    /// <param name="navigationManager">The navigation manager instance.</param>
    /// <param name="jsRuntime">The JavaScript runtime for invoking browser history API.</param>
    /// <param name="fallbackUrl">Optional fallback URL to navigate to if history navigation fails or is not available. Defaults to "/".</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="navigationManager"/> or <paramref name="jsRuntime"/> is null.</exception>
    public static async Task GoBackAsync(this NavigationManager navigationManager, IJSRuntime jsRuntime,
        string? fallbackUrl = "/", CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);
        ArgumentNullException.ThrowIfNull(jsRuntime);

        try
        {
            await jsRuntime.InvokeVoidAsync("history.back", cancellationToken);
        }
        catch (JSException)
        {
            // If JavaScript execution fails, navigate to fallback URL
            if (!string.IsNullOrWhiteSpace(fallbackUrl))
                navigationManager.NavigateTo(fallbackUrl);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, cannot perform navigation; Silently fail as there's no active connection
        }
        catch (TaskCanceledException)
        {
            // Operation was cancelled, don't throw
        }
    }

    /// <summary>
    /// Navigates forward to the next page in the browser history.
    /// </summary>
    /// <param name="navigationManager">The navigation manager instance.</param>
    /// <param name="jsRuntime">The JavaScript runtime for invoking browser history API.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="navigationManager"/> or <paramref name="jsRuntime"/> is null.</exception>
    public static async Task GoForwardAsync(this NavigationManager navigationManager, IJSRuntime jsRuntime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);
        ArgumentNullException.ThrowIfNull(jsRuntime);

        try
        {
            await jsRuntime.InvokeVoidAsync("history.forward", cancellationToken);
        }
        catch (JSException)
        {
            // If JavaScript execution fails, silently fail
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, cannot perform navigation
        }
        catch (TaskCanceledException)
        {
            // Operation was cancelled, don't throw
        }
    }

    /// <summary>
    /// Navigates to a specific position in the browser history.
    /// </summary>
    /// <param name="navigationManager">The navigation manager instance.</param>
    /// <param name="jsRuntime">The JavaScript runtime for invoking browser history API.</param>
    /// <param name="delta">The position in history to navigate to, relative to the current page. Negative values go back, positive values go forward.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="navigationManager"/> or <paramref name="jsRuntime"/> is null.</exception>
    public static async Task GoAsync(this NavigationManager navigationManager, IJSRuntime jsRuntime, int delta,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);
        ArgumentNullException.ThrowIfNull(jsRuntime);

        try
        {
            await jsRuntime.InvokeVoidAsync("history.go", cancellationToken, delta);
        }
        catch (JSException)
        {
            // If JavaScript execution fails, silently fail
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, cannot perform navigation
        }
        catch (TaskCanceledException)
        {
            // Operation was cancelled, don't throw
        }
    }

    /// <summary>
    /// Reloads the current page.
    /// </summary>
    /// <param name="navigationManager">The navigation manager instance.</param>
    /// <param name="jsRuntime">The JavaScript runtime for invoking browser location API.</param>
    /// <param name="forceReload">If true, forces a reload from the server, bypassing the browser cache.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="navigationManager"/> or <paramref name="jsRuntime"/> is null.</exception>
    public static async Task ReloadAsync(this NavigationManager navigationManager, IJSRuntime jsRuntime,
        bool forceReload = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);
        ArgumentNullException.ThrowIfNull(jsRuntime);

        try
        {
            await jsRuntime.InvokeVoidAsync("location.reload", cancellationToken, forceReload);
        }
        catch (JSException)
        {
            // If JavaScript execution fails, silently fail
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, cannot perform navigation
        }
        catch (TaskCanceledException)
        {
            // Operation was cancelled, don't throw
        }
    }
}
