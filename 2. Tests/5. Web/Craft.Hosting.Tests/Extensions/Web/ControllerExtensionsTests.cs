using Craft.Controllers.ErrorHandling;
using Craft.Controllers.ErrorHandling.Strategies;
using Craft.Hosting.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Hosting.Tests.Extensions.Web;

/// <summary>
/// Unit tests for ControllerExtensions.
/// </summary>
public class ControllerExtensionsTests
{
    [Fact]
    public void AddDatabaseErrorHandling_RegistersAllErrorStrategies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDatabaseErrorHandling();
        var serviceProvider = services.BuildServiceProvider();
        var strategies = serviceProvider.GetServices<IDatabaseErrorStrategy>();

        // Assert
        Assert.Contains(strategies, s => s is PostgreSqlErrorStrategy);
        Assert.Contains(strategies, s => s is SqlServerErrorStrategy);
        Assert.Contains(strategies, s => s is GenericErrorStrategy);
    }

    [Fact]
    public void AddDatabaseErrorHandling_RegistersErrorHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddDatabaseErrorHandling();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IDatabaseErrorHandler>());
    }

    [Fact]
    public void AddDatabaseErrorHandling_RegistersErrorHandlerWithSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDatabaseErrorHandling();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDatabaseErrorHandler));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddDatabaseErrorHandling_RegistersStrategiesWithSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDatabaseErrorHandling();

        // Assert
        var descriptors = services.Where(d => d.ServiceType == typeof(IDatabaseErrorStrategy));
        Assert.NotEmpty(descriptors);
        Assert.All(descriptors, d => Assert.Equal(ServiceLifetime.Singleton, d.Lifetime));
    }

    [Fact]
    public void AddDatabaseErrorHandling_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDatabaseErrorHandling();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddDatabaseErrorHandling_RegistersCorrectNumberOfStrategies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDatabaseErrorHandling();
        var serviceProvider = services.BuildServiceProvider();
        var strategies = serviceProvider.GetServices<IDatabaseErrorStrategy>();

        // Assert
        Assert.Equal(3, strategies.Count());
    }
}
