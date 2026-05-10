using MudBlazor;

namespace Craft.UiBuilders.Services.AppStatus;

/// <summary>
/// A lightweight pub/sub service that allows any component or service to post status
/// messages to the <c>CraftStatusBar</c> component currently mounted in the UI.
/// When no subscriber is present, messages are silently discarded.
/// </summary>
public interface IAppStatusService
{
    /// <summary>
    /// Gets the currently displayed message, or <c>null</c> when the bar is empty.
    /// </summary>
    StatusMessage? Current { get; }

    /// <summary>
    /// Shows <paramref name="text"/> in the status bar with the given <paramref name="severity"/>.
    /// Any message already visible is replaced immediately.
    /// For all severities except <see cref="Severity.Error"/> the message is automatically
    /// cleared after the configured duration.
    /// </summary>
    /// <param name="text">The status text to display.</param>
    /// <param name="severity">Controls colour coding and auto-clear behaviour.</param>
    Task ShowAsync(string text, Severity severity = Severity.Info);

    /// <summary>
    /// Clears the current message immediately, leaving the status bar empty.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Raised on the subscriber (the <c>CraftStatusBar</c> component) whenever
    /// the current message changes so the component can trigger a re-render.
    /// The event is dispatched via the component's <c>InvokeAsync</c> to ensure
    /// thread-safety across Blazor Server circuits and WASM.
    /// </summary>
    event Func<Task>? OnStatusChanged;
}
