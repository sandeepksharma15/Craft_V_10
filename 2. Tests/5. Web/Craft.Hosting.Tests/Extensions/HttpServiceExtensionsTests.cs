using Craft.Domain;
using Craft.Hosting.Extensions;
using Craft.QuerySpec;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Hosting.Tests.Extensions;

/// <summary>
/// Unit tests for HttpServiceExtensions.
/// </summary>
public class HttpServiceExtensionsTests
{
    [Fact]
    public void AddTransientHttpService_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTransientHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity, TestEntity, TestEntity>>());
    }

    [Fact]
    public void AddTransientHttpService_RegistersWithTransientLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTransientHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        // Assert
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(IHttpService<TestEntity, TestEntity, TestEntity>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddScopedHttpService_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddScopedHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity, TestEntity, TestEntity>>());
    }

    [Fact]
    public void AddScopedHttpService_RegistersWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddScopedHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        // Assert
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(IHttpService<TestEntity, TestEntity, TestEntity>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddSingletonHttpService_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddSingletonHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity, TestEntity, TestEntity>>());
    }

    [Fact]
    public void AddSingletonHttpService_RegistersWithSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddSingletonHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        // Assert
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(IHttpService<TestEntity, TestEntity, TestEntity>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddTransientHttpService_WithSeparateViewAndDto_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTransientHttpService<TestEntity, TestView, TestDto>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity, TestView, TestDto>>());
    }

    [Fact]
    public void AddTransientHttpService_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddTransientHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddScopedHttpService_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddScopedHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddSingletonHttpService_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddSingletonHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddTransientHttpService_WithRegisterSimplified_RegistersSimplifiedInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTransientHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test",
            registerSimplified: true);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity>>());
    }

    [Fact]
    public void AddTransientHttpService_WithRegisterWithKeyType_RegistersKeyTypeInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTransientHttpService<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test",
            registerWithKeyType: true);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity, TestEntity, TestEntity, KeyType>>());
    }

    private class TestEntity : IEntity, IModel
    {
        public KeyType Id { get; set; }
    }

    private class TestView : IModel
    {
        public long Id { get; set; }
    }

    private class TestDto : IModel
    {
        public long Id { get; set; }
    }
}
