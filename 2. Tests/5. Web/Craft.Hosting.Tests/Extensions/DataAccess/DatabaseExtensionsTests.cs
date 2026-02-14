using Craft.Data;
using Craft.Hosting.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Hosting.Tests.Extensions.DataAccess;

/// <summary>
/// Unit tests for DatabaseExtensions.
/// </summary>
public class DatabaseExtensionsTests
{
    [Fact]
    public void AddCraftDbContext_RegistersDbContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftDbContext<TestDbContext>((sp, options) =>
        {
            options.UseInMemoryDatabase("TestDb");
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<TestDbContext>());
    }

    [Fact]
    public void AddCraftDbContext_RegistersWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftDbContext<TestDbContext>((sp, options) =>
        {
            options.UseInMemoryDatabase("TestDb");
        });

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TestDbContext));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCraftDbContext_ConfiguresNoTrackingBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        DbContextOptionsBuilder? capturedOptions = null;

        // Act
        services.AddCraftDbContext<TestDbContext>((sp, options) =>
        {
            capturedOptions = options;
            options.UseInMemoryDatabase("TestDb");
        });

        var serviceProvider = services.BuildServiceProvider();
        var context = serviceProvider.GetRequiredService<TestDbContext>();

        // Assert
        Assert.Equal(QueryTrackingBehavior.NoTracking, context.ChangeTracker.QueryTrackingBehavior);
    }

    [Fact]
    public void AddCraftDbContextPool_RegistersDbContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftDbContextPool<TestDbContext>((sp, options) =>
        {
            options.UseInMemoryDatabase("TestDb");
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<TestDbContext>());
    }

    [Fact]
    public void AddCraftDbContextPool_UsesSpecifiedPoolSize()
    {
        // Arrange
        var services = new ServiceCollection();
        const int poolSize = 512;

        // Act
        services.AddCraftDbContextPool<TestDbContext>((sp, options) =>
        {
            options.UseInMemoryDatabase("TestDb");
        }, poolSize);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<TestDbContext>());
    }

    [Fact]
    public void ConfigureDatabases_RegistersAllServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:Provider"] = "PostgreSql",
                ["DatabaseOptions:ConnectionString"] = "Host=localhost;Database=test"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.ConfigureDatabases(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ConnectionStringService>());
        var providers = serviceProvider.GetServices<IDatabaseProvider>();
        Assert.Contains(providers, p => p is SqlServerDatabaseProvider);
        Assert.Contains(providers, p => p is PostgreSqlDatabaseProvider);
    }

    [Fact]
    public void ConfigureDatabases_RegistersConnectionStringHandlers()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:Provider"] = "PostgreSql"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.ConfigureDatabases(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handlers = serviceProvider.GetServices<IConnectionStringHandler>();
        Assert.Contains(handlers, h => h is SqlServerConnectionStringHandler);
        Assert.Contains(handlers, h => h is PostgreSqlConnectionStringHandler);
    }

    [Fact]
    public void ConfigureDatabases_RegistersCustomSeederRunner()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:Provider"] = "PostgreSql"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.ConfigureDatabases(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<Craft.Data.Helpers.CustomSeederRunner>());
    }

    [Fact]
    public void ConfigureDatabases_ConfiguresOptionsWithValidation()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:Provider"] = "PostgreSql",
                ["DatabaseOptions:ConnectionString"] = "Host=localhost;Database=test"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.ConfigureDatabases(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        // Assert
        Assert.Equal("PostgreSql", options.Provider);
        Assert.Equal("Host=localhost;Database=test", options.ConnectionString);
    }

    [Fact]
    public void ConfigureDatabases_ReturnsSameServiceCollection()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        // Act
        var result = services.ConfigureDatabases(configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCraftDbContext_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCraftDbContext<TestDbContext>((sp, options) =>
        {
            options.UseInMemoryDatabase("TestDb");
        });

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCraftDbContextPool_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCraftDbContextPool<TestDbContext>((sp, options) =>
        {
            options.UseInMemoryDatabase("TestDb");
        });

        // Assert
        Assert.Same(services, result);
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }
}
