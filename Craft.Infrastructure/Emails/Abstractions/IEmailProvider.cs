namespace Craft.Infrastructure.Emails;

/// <summary>
/// Defines the contract for email provider implementations.
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Gets the name of the email provider (e.g., "smtp", "sendgrid", "ses").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="request">The email request containing all email details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the send operation with a result indicating success or failure.</returns>
    Task<EmailResult> SendAsync(MailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the provider configuration.
    /// </summary>
    /// <returns>True if the provider is properly configured, false otherwise.</returns>
    bool IsConfigured();
}
