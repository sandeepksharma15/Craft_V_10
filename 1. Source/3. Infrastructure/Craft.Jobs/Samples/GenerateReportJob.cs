using Microsoft.Extensions.Logging;

namespace Craft.Jobs.Samples;

/// <summary>
/// Sample job for generating reports.
/// Demonstrates a job with complex parameters.
/// </summary>
public class GenerateReportJob : IBackgroundJob<GenerateReportJob.Parameters>
{
    private readonly ILogger<GenerateReportJob> _logger;

    public GenerateReportJob(ILogger<GenerateReportJob> logger)
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
            "Generating {ReportType} report for period {StartDate} to {EndDate}",
            parameters.ReportType,
            parameters.StartDate,
            parameters.EndDate);

        // Simulate report generation
        await Task.Delay(3000, cancellationToken);

        _logger.LogInformation("Report generated successfully");
    }

    public record Parameters(
        string ReportType,
        DateTime StartDate,
        DateTime EndDate,
        string? OutputFormat = "PDF");
}
