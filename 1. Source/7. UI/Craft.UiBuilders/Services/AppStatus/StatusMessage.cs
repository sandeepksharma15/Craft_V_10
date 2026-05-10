using MudBlazor;

namespace Craft.UiBuilders.Services.AppStatus;

/// <summary>
/// Represents a single status message displayed in the <c>CraftStatusBar</c> component.
/// </summary>
/// <param name="Text">The message text to display.</param>
/// <param name="Severity">The MudBlazor severity level that controls colour coding.</param>
/// <param name="Timestamp">The UTC time the message was created.</param>
public sealed record StatusMessage(string Text, Severity Severity, DateTimeOffset Timestamp)
{
    /// <summary>Creates a new <see cref="StatusMessage"/> stamped with the current UTC time.</summary>
    public static StatusMessage Create(string text, Severity severity = Severity.Info)
        => new(text, severity, DateTimeOffset.UtcNow);
}
