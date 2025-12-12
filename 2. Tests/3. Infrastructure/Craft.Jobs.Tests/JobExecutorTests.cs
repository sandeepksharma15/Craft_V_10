using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Jobs.Tests;

public class JobExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_WithSimpleJob_ExecutesSuccessfully()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<JobExecutor<SimpleTestJob>>>();
        var scopeMock = new Mock<IAsyncDisposable>();
        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        var jobMock = new Mock<SimpleTestJob>();

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

        var asyncServiceScopeMock = serviceScopeMock.As<IAsyncDisposable>();

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        scopeServiceProviderMock.Setup(x => x.GetService(typeof(SimpleTestJob)))
            .Returns(jobMock.Object);

        var executor = new JobExecutor<SimpleTestJob>(serviceProviderMock.Object, loggerMock.Object);
        var context = new JobExecutionContext { JobId = "test-job", JobType = "SimpleTestJob" };

        // Act
        await executor.ExecuteAsync(null, context, CancellationToken.None);

        // Assert
        jobMock.Verify(x => x.ExecuteAsync(context, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithParameterizedJob_ExecutesWithParameters()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<JobExecutor<ParameterizedTestJob>>>();
        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        var jobMock = new Mock<ParameterizedTestJob>();

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

        var asyncServiceScopeMock = serviceScopeMock.As<IAsyncDisposable>();

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        scopeServiceProviderMock.Setup(x => x.GetService(typeof(ParameterizedTestJob)))
            .Returns(jobMock.Object);

        var executor = new JobExecutor<ParameterizedTestJob>(serviceProviderMock.Object, loggerMock.Object);
        var context = new JobExecutionContext { JobId = "test-job", JobType = "ParameterizedTestJob" };
        var parameters = new { Value = "test" };

        // Act
        await executor.ExecuteAsync(parameters, context, CancellationToken.None);

        // Assert
        jobMock.Verify(x => x.ExecuteAsync(parameters, context, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenJobThrowsException_PropagatesException()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<JobExecutor<ThrowingTestJob>>>();
        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        var job = new ThrowingTestJob();

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

        var asyncServiceScopeMock = serviceScopeMock.As<IAsyncDisposable>();

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        scopeServiceProviderMock.Setup(x => x.GetService(typeof(ThrowingTestJob)))
            .Returns(job);

        var executor = new JobExecutor<ThrowingTestJob>(serviceProviderMock.Object, loggerMock.Object);
        var context = new JobExecutionContext { JobId = "test-job", JobType = "ThrowingTestJob" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            executor.ExecuteAsync(null, context, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_SetsStartedAt_InContext()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<JobExecutor<SimpleTestJob>>>();
        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        var jobMock = new Mock<SimpleTestJob>();

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

        var asyncServiceScopeMock = serviceScopeMock.As<IAsyncDisposable>();

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        scopeServiceProviderMock.Setup(x => x.GetService(typeof(SimpleTestJob)))
            .Returns(jobMock.Object);

        var executor = new JobExecutor<SimpleTestJob>(serviceProviderMock.Object, loggerMock.Object);
        var context = new JobExecutionContext { JobId = "test-job", JobType = "SimpleTestJob" };

        // Act
        await executor.ExecuteAsync(null, context, CancellationToken.None);

        // Assert
        Assert.NotNull(context.StartedAt);
    }

    // Test job classes
    public class SimpleTestJob : IBackgroundJob
    {
        public virtual Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class ParameterizedTestJob : IBackgroundJob<object>
    {
        public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public virtual Task ExecuteAsync(object parameters, JobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class ThrowingTestJob : IBackgroundJob
    {
        public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test exception");
        }
    }
}
