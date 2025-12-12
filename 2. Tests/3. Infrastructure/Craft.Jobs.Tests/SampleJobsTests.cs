using Microsoft.Extensions.Logging;

namespace Craft.Jobs.Tests;

public class SampleJobsTests
{
    [Fact]
    public async Task SendEmailJob_ExecutesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<Samples.SendEmailJob>>();
        var job = new Samples.SendEmailJob(loggerMock.Object);
        var parameters = new Samples.SendEmailJob.Parameters(
            "test@example.com",
            "Test Subject",
            "Test Body");
        var context = new JobExecutionContext { JobId = "test-job" };

        // Act
        await job.ExecuteAsync(parameters, context, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("test@example.com")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task DatabaseCleanupJob_ExecutesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<Samples.DatabaseCleanupJob>>();
        var job = new Samples.DatabaseCleanupJob(loggerMock.Object);
        var context = new JobExecutionContext { JobId = "test-job", TenantId = "tenant-123" };

        // Act
        await job.ExecuteAsync(context, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting database cleanup")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateReportJob_ExecutesSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<Samples.GenerateReportJob>>();
        var job = new Samples.GenerateReportJob(loggerMock.Object);
        var parameters = new Samples.GenerateReportJob.Parameters(
            "Monthly Report",
            DateTime.UtcNow.AddMonths(-1),
            DateTime.UtcNow);
        var context = new JobExecutionContext { JobId = "test-job" };

        // Act
        await job.ExecuteAsync(parameters, context, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Monthly Report")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void SendEmailJob_Parameters_CanBeCreated()
    {
        // Arrange & Act
        var parameters = new Samples.SendEmailJob.Parameters(
            "to@example.com",
            "Subject",
            "Body");

        // Assert
        Assert.Equal("to@example.com", parameters.To);
        Assert.Equal("Subject", parameters.Subject);
        Assert.Equal("Body", parameters.Body);
    }

    [Fact]
    public void GenerateReportJob_Parameters_CanBeCreated()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;

        // Act
        var parameters = new Samples.GenerateReportJob.Parameters(
            "Sales Report",
            startDate,
            endDate,
            "Excel");

        // Assert
        Assert.Equal("Sales Report", parameters.ReportType);
        Assert.Equal(startDate, parameters.StartDate);
        Assert.Equal(endDate, parameters.EndDate);
        Assert.Equal("Excel", parameters.OutputFormat);
    }

    [Fact]
    public void GenerateReportJob_Parameters_DefaultsOutputFormatToPDF()
    {
        // Arrange & Act
        var parameters = new Samples.GenerateReportJob.Parameters(
            "Report",
            DateTime.UtcNow,
            DateTime.UtcNow);

        // Assert
        Assert.Equal("PDF", parameters.OutputFormat);
    }
}
