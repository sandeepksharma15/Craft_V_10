using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazor;

namespace Craft.UiBuilders.Services.AppStatus;

/// <summary>
/// Default implementation of <see cref="IAppStatusService"/>.
/// Register as <b>Scoped</b> so each Blazor circuit (browser tab) maintains
/// its own independent status state.
/// </summary>
public sealed class AppStatusService : IAppStatusService, IAsyncDisposable
{
    private readonly ILogger<AppStatusService> _logger;
    private readonly AppStatusOptions _options;

    // Cancels the pending auto-clear task whenever a new message arrives.
    private CancellationTokenSource? _autoClearCts;

    public AppStatusService(ILogger<AppStatusService> logger, IOptions<AppStatusOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public StatusMessage? Current { get; private set; }

    /// <inheritdoc/>
    public event Func<Task>? OnStatusChanged;

    /// <inheritdoc/>
    public async Task ShowAsync(string text, Severity severity = Severity.Info)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        // Cancel any pending auto-clear for the previous message.
        await CancelAutoClearAsync();

        Current = StatusMessage.Create(text, severity);
        _logger.LogInformation("Status [{Severity}]: {Text}", severity, text);

        await NotifySubscriberAsync();

        // Schedule auto-clear for non-error severities.
        var durationMs = GetDurationMs(severity);

        if (durationMs > 0)
            _ = ScheduleAutoClearAsync(durationMs);
    }

    /// <inheritdoc/>
    public async Task ClearAsync()
    {
        await CancelAutoClearAsync();
        Current = null;
        await NotifySubscriberAsync();
    }

    // -----------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------

    private async Task ScheduleAutoClearAsync(int durationMs)
    {
        _autoClearCts = new CancellationTokenSource();
        var token = _autoClearCts.Token;

        try
        {
            await Task.Delay(durationMs, token);

            if (!token.IsCancellationRequested)
            {
                Current = null;
                await NotifySubscriberAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when a new message arrives before the delay completes.
        }
    }

    private async Task CancelAutoClearAsync()
    {
        // Capture into a local before the first await — a concurrent call can null
        // the field between the null-check and the subsequent await/dispose.
        var cts = _autoClearCts;
        if (cts is null)
            return;

        _autoClearCts = null;
        await cts.CancelAsync();
        cts.Dispose();
    }

    private async Task NotifySubscriberAsync()
    {
        if (OnStatusChanged is null)
            return;

        try
        {
            await OnStatusChanged.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error notifying status bar subscriber");
        }
    }

    private int GetDurationMs(Severity severity) => severity switch
    {
        Severity.Info    => _options.InfoDurationMs,
        Severity.Success => _options.SuccessDurationMs,
        Severity.Warning => _options.WarningDurationMs,
        Severity.Error   => 0,   // Errors stay until explicitly cleared.
        _                => _options.InfoDurationMs
    };

    public async ValueTask DisposeAsync()
    {
        if (_autoClearCts is not null)
        {
            await _autoClearCts.CancelAsync();
            _autoClearCts.Dispose();
        }
    }
}
