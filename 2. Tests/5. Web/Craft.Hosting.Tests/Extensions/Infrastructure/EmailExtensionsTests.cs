using Craft.Emails;
using Craft.Hosting.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Hosting.Tests.Extensions.Infrastructure;

/// <summary>
/// Unit tests for EmailExtensions.
/// </summary>
public class EmailExtensionsTests
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
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IMailService>());
        Assert.NotNull(serviceProvider.GetService<IEmailQueue>());
        Assert.NotNull(serviceProvider.GetService<IEmailTemplateRenderer>());
        Assert.NotNull(serviceProvider.GetService<IEmailProviderFactory>());
    }

    [Fact]
    public void AddEmailServices_WithConfigurationSection_RegistersAllServices()
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
        var section = configuration.GetSection("EmailOptions");

        // Act
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, section);
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
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, options =>
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
    public void AddEmailServices_RegistersEmailProviders()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, options =>
        {
            options.Provider = "mock";
            options.From = "test@example.com";
        });

        var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IEmailProvider>();

        // Assert
        Assert.Contains(providers, p => p is SmtpEmailProvider);
        Assert.Contains(providers, p => p is MockEmailProvider);
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
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, configuration);
        Craft.Hosting.Extensions.EmailExtensions.AddEmailProvider<TestEmailProvider>(services);

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
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, configuration);
        Craft.Hosting.Extensions.EmailExtensions.AddEmailQueue<TestEmailQueue>(services);

        var serviceProvider = services.BuildServiceProvider();
        var queue = serviceProvider.GetService<IEmailQueue>();

        // Assert
        Assert.IsType<TestEmailQueue>(queue);
    }

    [Fact]
    public void AddEmailTemplateRenderer_ReplacesDefaultRenderer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, options => options.Provider = "mock");
        Craft.Hosting.Extensions.EmailExtensions.AddEmailTemplateRenderer<TestTemplateRenderer>(services);

        var serviceProvider = services.BuildServiceProvider();
        var renderer = serviceProvider.GetService<IEmailTemplateRenderer>();

        // Assert
        Assert.IsType<TestTemplateRenderer>(renderer);
    }

    [Fact]
    public void AddEmailServices_ConfiguresOptionsWithValidation()
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
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;

        // Assert
        Assert.Equal("mock", options.Provider);
        Assert.Equal("test@example.com", options.From);
    }

    [Fact]
    public void AddEmailServices_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddEmailProvider_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, options => options.Provider = "mock");

        // Act
        var result = Craft.Hosting.Extensions.EmailExtensions.AddEmailProvider<TestEmailProvider>(services);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddEmailQueue_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, options => options.Provider = "mock");

        // Act
        var result = Craft.Hosting.Extensions.EmailExtensions.AddEmailQueue<TestEmailQueue>(services);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddEmailTemplateRenderer_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        Craft.Hosting.Extensions.EmailExtensions.AddEmailServices(services, options => options.Provider = "mock");

        // Act
        var result = Craft.Hosting.Extensions.EmailExtensions.AddEmailTemplateRenderer<TestTemplateRenderer>(services);

        // Assert
        Assert.Same(services, result);
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

    private class TestTemplateRenderer : IEmailTemplateRenderer
    {
        public Task<string> RenderAsync<T>(string templateName, T model, CancellationToken cancellationToken = default)
            => Task.FromResult(string.Empty);
        public Task<string> RenderAsync(string templateName, CancellationToken cancellationToken = default)
            => Task.FromResult(string.Empty);
        public bool TemplateExists(string templateName) => true;
        public void ClearCache() { }
    }
}
