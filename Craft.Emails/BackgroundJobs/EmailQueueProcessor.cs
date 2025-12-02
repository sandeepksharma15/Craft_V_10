using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Emails;

/// <summary>
/// Background service that processes queued emails.
/// </summary>
public class EmailQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailQueueProcessor> _logger;
    private readonly EmailOptions _options;

    public EmailQueueProcessor(
        IServiceProvider serviceProvider,
        IOptions<EmailOptions> options,
        ILogger<EmailQueueProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Queue Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email queue");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("Email Queue Processor stopped");
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailQueue = scope.ServiceProvider.GetRequiredService<IEmailQueue>();
        var providerFactory = scope.ServiceProvider.GetRequiredService<IEmailProviderFactory>();

        var email = await emailQueue.DequeueAsync(cancellationToken);

        if (email == null)
            return;

        _logger.LogInformation(
            "Processing queued email {EmailId} (Attempt {Attempt})",
            email.Id,
            email.Attempts + 1);

        email.Status = EmailStatus.Sending;
        email.Attempts++;
        email.LastAttemptAt = DateTimeOffset.UtcNow;
        await emailQueue.UpdateAsync(email, cancellationToken);

        var provider = providerFactory.GetDefaultProvider();
        var result = await provider.SendAsync(email.Request, cancellationToken);

        if (result.IsSuccess)
        {
            email.Status = EmailStatus.Sent;
            email.SentAt = result.SentAt;
            email.MessageId = result.MessageId;
            _logger.LogInformation("Email {EmailId} sent successfully", email.Id);
        }
        else
        {
            email.ErrorMessage = result.ErrorMessage;

            if (email.Attempts >= _options.MaxRetryAttempts)
            {
                email.Status = EmailStatus.FailedPermanently;
                _logger.LogError(
                    "Email {EmailId} failed permanently after {Attempts} attempts: {Error}",
                    email.Id,
                    email.Attempts,
                    email.ErrorMessage);
            }
            else
            {
                email.Status = EmailStatus.Failed;
                _logger.LogWarning(
                    "Email {EmailId} failed (Attempt {Attempt}/{MaxAttempts}): {Error}",
                    email.Id,
                    email.Attempts,
                    _options.MaxRetryAttempts,
                    email.ErrorMessage);

                await emailQueue.EnqueueAsync(email, cancellationToken);
            }
        }

        await emailQueue.UpdateAsync(email, cancellationToken);
    }
}
