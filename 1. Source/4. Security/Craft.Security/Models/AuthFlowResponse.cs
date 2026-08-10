namespace Craft.Security;

/// <summary>
/// Represents the user-facing outcome of an authentication flow that may involve email delivery.
/// </summary>
public sealed record AuthFlowResponse
{
    /// <summary>
    /// Gets the message the client should display to the user after a successful auth action.
    /// </summary>
    public string UserMessage { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether a real auth email sender is configured.
    /// </summary>
    public bool EmailSenderEnabled { get; init; }

    /// <summary>
    /// Gets a value indicating whether a confirmation email was sent for this action.
    /// </summary>
    public bool ConfirmationEmailSent { get; init; }

    /// <summary>
    /// Gets a value indicating whether a welcome email was sent for this action.
    /// </summary>
    public bool WelcomeEmailSent { get; init; }

    /// <summary>
    /// Gets a value indicating whether the forgot-password flow should instruct the user to check their inbox.
    /// </summary>
    public bool PasswordResetEmailRequested { get; init; }
}
