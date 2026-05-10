namespace Craft.UiBuilders.Services.AppStatus;

/// <summary>
/// Configuration options for <see cref="IAppStatusService"/>.
/// Controls how long messages are displayed before being automatically cleared.
/// Error messages are never auto-cleared regardless of these settings.
/// </summary>
public sealed record AppStatusOptions
{
    /// <summary>
    /// Gets the duration in milliseconds that <see cref="MudBlazor.Severity.Info"/> messages
    /// remain visible before being automatically cleared. Default is 1 000 ms.
    /// </summary>
    public int InfoDurationMs { get; init; } = 1_000;

    /// <summary>
    /// Gets the duration in milliseconds that <see cref="MudBlazor.Severity.Success"/> messages
    /// remain visible before being automatically cleared. Default is 2 000 ms.
    /// </summary>
    public int SuccessDurationMs { get; init; } = 2_000;

    /// <summary>
    /// Gets the duration in milliseconds that <see cref="MudBlazor.Severity.Warning"/> messages
    /// remain visible before being automatically cleared. Default is 3 000 ms.
    /// </summary>
    public int WarningDurationMs { get; init; } = 3_000;
}
