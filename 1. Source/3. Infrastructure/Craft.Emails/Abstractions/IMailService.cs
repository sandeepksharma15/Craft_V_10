namespace Craft.Emails;

/// <summary>
/// Defines the contract for email sending operations.
/// </summary>
public interface IMailService
{
    /// <summary>
    /// Sends an email immediately.
    /// </summary>
    /// <param name="request">The email request containing all email details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<EmailResult> SendAsync(MailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queues an email for background sending.
    /// </summary>
    /// <param name="request">The email request containing all email details.</param>
    /// <param name="priority">The priority of the email.</param>
    /// <param name="scheduledFor">Optional scheduled send time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<string> QueueAsync(MailRequest request, EmailPriority priority = EmailPriority.Normal,
        DateTimeOffset? scheduledFor = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email using a template.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="to">Recipient email addresses.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="templateName">The name of the template.</param>
    /// <param name="model">The model data for the template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<EmailResult> SendTemplateAsync<T>(List<string> to, string subject, string templateName, T model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queues an email using a template for background sending.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="to">Recipient email addresses.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="templateName">The name of the template.</param>
    /// <param name="model">The model data for the template.</param>
    /// <param name="priority">The priority of the email.</param>
    /// <param name="scheduledFor">Optional scheduled send time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<string> QueueTemplateAsync<T>(List<string> to, string subject, string templateName, T model,
        EmailPriority priority = EmailPriority.Normal, DateTimeOffset? scheduledFor = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a preview of an email template without sending it.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="templateName">The name of the template.</param>
    /// <param name="model">The model data for the template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<string> PreviewTemplateAsync<T>(string templateName, T model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a queued email.
    /// </summary>
    /// <param name="emailId">The queued email ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<QueuedEmail?> GetQueuedEmailAsync(string emailId, CancellationToken cancellationToken = default);
}
