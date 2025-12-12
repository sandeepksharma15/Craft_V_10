using Microsoft.Extensions.Logging;

namespace Craft.Jobs.Samples;

/// <summary>
/// Sample job for sending email notifications.
/// Demonstrates a job with parameters.
/// </summary>
public class SendEmailJob : IBackgroundJob<SendEmailJob.Parameters>
{
    private readonly ILogger<SendEmailJob> _logger;

    public SendEmailJob(ILogger<SendEmailJob> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use ExecuteAsync with parameters");
    }

    public async Task ExecuteAsync(Parameters parameters, JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending email to {To} with subject '{Subject}'",
            parameters.To,
            parameters.Subject);

        // Simulate email sending
        await Task.Delay(1000, cancellationToken);

        _logger.LogInformation("Email sent successfully to {To}", parameters.To);
    }

    public record Parameters(string To, string Subject, string Body);
}
