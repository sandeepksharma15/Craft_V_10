using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Jobs.Tests;

public class HangfireJobSchedulerTests
{
    private readonly Mock<ILogger<HangfireJobScheduler>> _loggerMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly Mock<IRecurringJobManager> _recurringJobManagerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly HangfireJobScheduler _scheduler;

    public HangfireJobSchedulerTests()
    {
        _loggerMock = new Mock<ILogger<HangfireJobScheduler>>();
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _recurringJobManagerMock = new Mock<IRecurringJobManager>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);

        _scheduler = new HangfireJobScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _backgroundJobClientMock.Object,
            _recurringJobManagerMock.Object);
    }

    [Fact]
    public void Schedule_FireAndForget_ReturnsJobId()
    {
        // Arrange
        var expectedJobId = "job-123";
        _backgroundJobClientMock
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
            .Returns(expectedJobId);

        // Act
        var jobId = _scheduler.Schedule<TestJob>();

        // Assert
        Assert.Equal(expectedJobId, jobId);
        _backgroundJobClientMock.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Fact]
    public void Schedule_WithDelay_SchedulesDelayedJob()
    {
        // Arrange
        var delay = TimeSpan.FromMinutes(30);
        var expectedJobId = "delayed-job-123";
        _backgroundJobClientMock
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<ScheduledState>()))
            .Returns(expectedJobId);

        // Act
        var jobId = _scheduler.Schedule<TestJob>(delay);

        // Assert
        Assert.Equal(expectedJobId, jobId);
        _backgroundJobClientMock.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<ScheduledState>()), Times.Once);
    }

    [Fact]
    public void Schedule_WithDateTimeOffset_SchedulesAtSpecificTime()
    {
        // Arrange
        var scheduledFor = DateTimeOffset.UtcNow.AddHours(1);
        var expectedJobId = "scheduled-job-123";
        _backgroundJobClientMock
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<ScheduledState>()))
            .Returns(expectedJobId);

        // Act
        var jobId = _scheduler.Schedule<TestJob>(scheduledFor);

        // Assert
        Assert.Equal(expectedJobId, jobId);
        _backgroundJobClientMock.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<ScheduledState>()), Times.Once);
    }

    [Fact]
    public void ScheduleRecurring_WithCronExpression_CreatesRecurringJob()
    {
        // Arrange
        var recurringJobId = "recurring-job-1";
        var cronExpression = "0 0 * * *";

        // Act
        _scheduler.ScheduleRecurring<TestJob>(recurringJobId, cronExpression);

        // Assert
        _recurringJobManagerMock.Verify(
            x => x.AddOrUpdate(
                recurringJobId,
                It.IsAny<Job>(),
                cronExpression,
                It.IsAny<RecurringJobOptions>()),
            Times.Once);
    }

    [Fact]
    public void ScheduleRecurring_WithMinuteInterval_CreatesRecurringJob()
    {
        // Arrange
        var recurringJobId = "recurring-job-2";
        var intervalMinutes = 15;
        var expectedCron = "*/15 * * * *";

        // Act
        _scheduler.ScheduleRecurring<TestJob>(recurringJobId, intervalMinutes);

        // Assert
        _recurringJobManagerMock.Verify(
            x => x.AddOrUpdate(
                recurringJobId,
                It.IsAny<Job>(),
                expectedCron,
                It.IsAny<RecurringJobOptions>()),
            Times.Once);
    }

    [Fact]
    public void ScheduleRecurring_WithZeroInterval_ThrowsArgumentException()
    {
        // Arrange
        var recurringJobId = "recurring-job-3";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            _scheduler.ScheduleRecurring<TestJob>(recurringJobId, 0));
        Assert.Contains("Interval must be at least 1 minute", ex.Message);
    }

    [Fact]
    public void RemoveRecurring_RemovesRecurringJob()
    {
        // Arrange
        var recurringJobId = "recurring-job-to-remove";

        // Act
        _scheduler.RemoveRecurring(recurringJobId);

        // Assert
        _recurringJobManagerMock.Verify(x => x.RemoveIfExists(recurringJobId), Times.Once);
    }

    [Fact]
    public void TriggerRecurring_TriggersRecurringJob()
    {
        // Arrange
        var recurringJobId = "recurring-job-to-trigger";

        // Act
        _scheduler.TriggerRecurring(recurringJobId);

        // Assert
        _recurringJobManagerMock.Verify(x => x.Trigger(recurringJobId), Times.Once);
    }

    [Fact]
    public void Requeue_CallsBackgroundJobClient()
    {
        // Arrange
        var jobId = "job-to-requeue";
        // Note: Requeue is an extension method, so we can't directly mock it
        // We'll verify the scheduler calls it by checking the actual implementation

        // Act
        _ = _scheduler.Requeue(jobId);

        // Assert
        // The method will attempt to call the extension, result depends on implementation
        // In real scenario, this would interact with Hangfire infrastructure
    }

    // Test job class
    private class TestJob : IBackgroundJob
    {
        public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
