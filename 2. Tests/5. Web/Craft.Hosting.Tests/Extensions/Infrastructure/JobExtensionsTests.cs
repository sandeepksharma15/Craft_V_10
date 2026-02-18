using Craft.Hosting.Extensions;
using Craft.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Hosting.Tests.Extensions.Infrastructure;

/// <summary>
/// Unit tests for JobExtensions.
/// </summary>
public class JobExtensionsTests
{
    [Fact]
    public void AddJobServices_WithConfiguration_RegistersJobScheduler()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobOptions:ConnectionString"] = "Host=localhost;Database=test",
                ["JobOptions:SchemaName"] = "hangfire",
                ["JobOptions:WorkerCount"] = "5"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IJobScheduler>());
    }

    [Fact]
    public void AddJobServices_WithConfigurationSection_RegistersJobScheduler()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobOptions:ConnectionString"] = "Host=localhost;Database=test",
                ["JobOptions:SchemaName"] = "hangfire"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        var section = configuration.GetSection("JobOptions");

        // Act
        Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, section);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IJobScheduler>());
    }

    [Fact]
    public void AddJobServices_WithAction_RegistersJobScheduler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, options =>
        {
            options.ConnectionString = "Host=localhost;Database=test";
            options.SchemaName = "hangfire";
            options.WorkerCount = 5;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IJobScheduler>());
    }

    [Fact]
    public void AddJobServices_ConfiguresOptionsWithValidation()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobOptions:ConnectionString"] = "Host=localhost;Database=test",
                ["JobOptions:SchemaName"] = "hangfire",
                ["JobOptions:WorkerCount"] = "10"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<JobOptions>>().Value;

        // Assert
        Assert.Equal("Host=localhost;Database=test", options.ConnectionString);
        Assert.Equal("hangfire", options.SchemaName);
        Assert.Equal(10, options.WorkerCount);
    }

    [Fact]
    public void AddJobServices_ThrowsException_WhenOptionsAreMissing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, configuration));
    }

    [Fact]
    public void AddJobServices_ReturnsSameServiceCollection()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobOptions:ConnectionString"] = "Host=localhost;Database=test"
            })
            .Build();
        var services = new ServiceCollection();

        // Act
        var result = Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddJobServices_RegistersJobSchedulerWithScopedLifetime()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobOptions:ConnectionString"] = "Host=localhost;Database=test"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, configuration);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IJobScheduler));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddJobServices_ConfiguresAutomaticRetry_WhenEnabled()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobOptions:ConnectionString"] = "Host=localhost;Database=test",
                ["JobOptions:EnableAutomaticRetry"] = "true",
                ["JobOptions:MaxRetryAttempts"] = "3"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<JobOptions>>().Value;

        // Assert
        Assert.True(options.EnableAutomaticRetry);
        Assert.Equal(3, options.MaxRetryAttempts);
    }

    [Fact]
    public void AddJobServices_ConfiguresMultiTenancy_WhenEnabled()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobOptions:ConnectionString"] = "Host=localhost;Database=test",
                ["JobOptions:EnableMultiTenancy"] = "true"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<JobOptions>>().Value;

        // Assert
        Assert.True(options.EnableMultiTenancy);
    }

    [Fact]
    public void AddJobServices_ConfiguresDashboard_WhenEnabled()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobOptions:ConnectionString"] = "Host=localhost;Database=test",
                ["JobOptions:EnableDashboard"] = "true",
                ["JobOptions:DashboardPath"] = "/jobs"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.JobExtensions.AddJobServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<JobOptions>>().Value;

        // Assert
        Assert.True(options.EnableDashboard);
        Assert.Equal("/jobs", options.DashboardPath);
    }
}
