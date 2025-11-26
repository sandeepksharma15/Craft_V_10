using Craft.Infrastructure.Emails;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Infrastructure.Tests.Emails;

public class EmailServiceExtensionsTests
{
    [Fact]
    public void AddEmailServices_WithConfiguration_RegistersAllServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailOptions:Provider"] = "mock",
                ["EmailOptions:From"] = "test@example.com",
                ["EmailOptions:EnableQueue"] = "false"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddEmailServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IMailService>());
        Assert.NotNull(serviceProvider.GetService<IEmailQueue>());
        Assert.NotNull(serviceProvider.GetService<IEmailTemplateRenderer>());
        Assert.NotNull(serviceProvider.GetService<IEmailProviderFactory>());
    }

    [Fact]
    public void AddEmailServices_WithAction_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEmailServices(options =>
        {
            options.Provider = "mock";
            options.From = "test@example.com";
            options.EnableQueue = false;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IMailService>());
        Assert.NotNull(serviceProvider.GetService<IEmailQueue>());
        Assert.NotNull(serviceProvider.GetService<IEmailTemplateRenderer>());
        Assert.NotNull(serviceProvider.GetService<IEmailProviderFactory>());
    }

    [Fact]
    public void AddEmailProvider_RegistersCustomProvider()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailOptions:Provider"] = "mock",
                ["EmailOptions:From"] = "test@example.com"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services
            .AddEmailServices(configuration)
            .AddEmailProvider<TestEmailProvider>();

        var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IEmailProvider>();

        // Assert
        Assert.Contains(providers, p => p is TestEmailProvider);
    }

    [Fact]
    public void AddEmailQueue_ReplacesDefaultQueue()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailOptions:Provider"] = "mock",
                ["EmailOptions:From"] = "test@example.com"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services
            .AddEmailServices(configuration)
            .AddEmailQueue<TestEmailQueue>();

        var serviceProvider = services.BuildServiceProvider();
        var queue = serviceProvider.GetService<IEmailQueue>();

        // Assert
        Assert.IsType<TestEmailQueue>(queue);
    }

    private class TestEmailProvider : IEmailProvider
    {
        public string Name => "test";
        public bool IsConfigured() => true;
        public Task<EmailResult> SendAsync(MailRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(EmailResult.Success());
    }

    private class TestEmailQueue : IEmailQueue
    {
        public Task EnqueueAsync(QueuedEmail email, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task<QueuedEmail?> DequeueAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<QueuedEmail?>(null);
        public Task UpdateAsync(QueuedEmail email, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task<QueuedEmail?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult<QueuedEmail?>(null);
        public Task<IEnumerable<QueuedEmail>> GetByStatusAsync(EmailStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult(Enumerable.Empty<QueuedEmail>());
        public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }
}
