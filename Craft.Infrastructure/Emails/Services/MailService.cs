using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Infrastructure.Emails;

/// <summary>
/// Main email service implementation with support for multiple providers, queuing, and templates.
/// </summary>
public class MailService : IMailService
{
    private readonly IEmailProviderFactory _providerFactory;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly IEmailQueue _emailQueue;
    private readonly EmailOptions _options;
    private readonly ILogger<MailService> _logger;

    public MailService(
        IEmailProviderFactory providerFactory,
        IEmailTemplateRenderer templateRenderer,
        IEmailQueue emailQueue,
        IOptions<EmailOptions> options,
        ILogger<MailService> logger)
    {
        _providerFactory = providerFactory;
        _templateRenderer = templateRenderer;
        _emailQueue = emailQueue;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmailResult> SendAsync(MailRequest request, CancellationToken cancellationToken = default)
    {
        var provider = _providerFactory.GetDefaultProvider();

        _logger.LogInformation(
            "Sending email to {To} with subject '{Subject}' using provider '{Provider}'",
            string.Join(", ", request.To),
            request.Subject,
            provider.Name);

        return await SendWithRetryAsync(provider, request, cancellationToken);
    }

    public async Task<string> QueueAsync(
        MailRequest request,
        EmailPriority priority = EmailPriority.Normal,
        DateTimeOffset? scheduledFor = null,
        CancellationToken cancellationToken = default)
    {
        var queuedEmail = new QueuedEmail
        {
            Request = request,
            Priority = priority,
            ScheduledFor = scheduledFor,
            Status = scheduledFor.HasValue ? EmailStatus.Scheduled : EmailStatus.Pending
        };

        await _emailQueue.EnqueueAsync(queuedEmail, cancellationToken);

        _logger.LogInformation(
            "Email queued with ID {EmailId} for {To} with subject '{Subject}'",
            queuedEmail.Id,
            string.Join(", ", request.To),
            request.Subject);

        return queuedEmail.Id;
    }

    public async Task<EmailResult> SendTemplateAsync<T>(
        List<string> to,
        string subject,
        string templateName,
        T model,
        CancellationToken cancellationToken = default)
    {
        var body = await _templateRenderer.RenderAsync(templateName, model, cancellationToken);

        var request = new MailRequest(
            to: to,
            subject: subject,
            body: body);

        return await SendAsync(request, cancellationToken);
    }

    public async Task<string> QueueTemplateAsync<T>(
        List<string> to,
        string subject,
        string templateName,
        T model,
        EmailPriority priority = EmailPriority.Normal,
        DateTimeOffset? scheduledFor = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _templateRenderer.RenderAsync(templateName, model, cancellationToken);

        var request = new MailRequest(
            to: to,
            subject: subject,
            body: body);

        return await QueueAsync(request, priority, scheduledFor, cancellationToken);
    }

    public async Task<string> PreviewTemplateAsync<T>(
        string templateName,
        T model,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating preview for template '{TemplateName}'", templateName);
        return await _templateRenderer.RenderAsync(templateName, model, cancellationToken);
    }

    public Task<QueuedEmail?> GetQueuedEmailAsync(string emailId, CancellationToken cancellationToken = default)
    {
        return _emailQueue.GetByIdAsync(emailId, cancellationToken);
    }

    private async Task<EmailResult> SendWithRetryAsync(
        IEmailProvider provider,
        MailRequest request,
        CancellationToken cancellationToken)
    {
        var maxAttempts = _options.MaxRetryAttempts + 1;
        EmailResult? lastResult = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                lastResult = await provider.SendAsync(request, cancellationToken);

                if (lastResult.IsSuccess)
                {
                    if (attempt > 1)
                        _logger.LogInformation(
                            "Email sent successfully on attempt {Attempt} of {MaxAttempts}",
                            attempt,
                            maxAttempts);

                    return lastResult;
                }

                _logger.LogWarning(
                    "Email send failed on attempt {Attempt} of {MaxAttempts}: {Error}",
                    attempt,
                    maxAttempts,
                    lastResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception on email send attempt {Attempt} of {MaxAttempts}",
                    attempt,
                    maxAttempts);

                lastResult = EmailResult.Failure(ex.Message, ex);
            }

            if (attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(_options.RetryDelaySeconds * attempt);
                _logger.LogInformation("Retrying email send in {Delay} seconds", delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        _logger.LogError(
            "Email failed permanently after {MaxAttempts} attempts",
            maxAttempts);

        return lastResult ?? EmailResult.Failure("Email send failed");
    }
}
