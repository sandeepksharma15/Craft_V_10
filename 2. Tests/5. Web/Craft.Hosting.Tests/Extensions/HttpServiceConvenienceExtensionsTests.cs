using Craft.Domain;
using Craft.Hosting.Extensions;
using Craft.QuerySpec;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Hosting.Tests.Extensions;

/// <summary>
/// Unit tests for HttpServiceConvenienceExtensions.
/// </summary>
public class HttpServiceConvenienceExtensionsTests
{
    [Fact]
    public void AddHttpServiceForBlazor_WithTransientLifetime_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpServiceForBlazor<TestEntity, TestView, TestDto>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test",
            ServiceLifetime.Transient);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity, TestView, TestDto, KeyType>>());
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity>>());
    }

    [Fact]
    public void AddHttpServiceForBlazor_WithScopedLifetime_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpServiceForBlazor<TestEntity, TestView, TestDto>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test",
            ServiceLifetime.Scoped);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity, TestView, TestDto, KeyType>>());
    }

    [Fact]
    public void AddHttpServiceForBlazor_WithSingletonLifetime_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpServiceForBlazor<TestEntity, TestView, TestDto>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test",
            ServiceLifetime.Singleton);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity, TestView, TestDto, KeyType>>());
    }

    [Fact]
    public void AddHttpServiceForBlazor_DoesNotRegisterPrimaryInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpServiceForBlazor<TestEntity, TestView, TestDto>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        var serviceProvider = services.BuildServiceProvider();

        // Assert - IHttpService<T, TView, TDto> without KeyType should not be registered
        Assert.Null(serviceProvider.GetService<IHttpService<TestEntity, TestView, TestDto>>());
    }

    [Fact]
    public void AddHttpServiceForApi_RegistersKeyTypeInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpServiceForApi<TestEntity, TestView, TestDto>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity, TestView, TestDto, KeyType>>());
    }

    [Fact]
    public void AddHttpServiceForApi_DoesNotRegisterSimplifiedInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpServiceForApi<TestEntity, TestView, TestDto>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Null(serviceProvider.GetService<IHttpService<TestEntity>>());
    }

    [Fact]
    public void AddHttpServiceForList_RegistersSimplifiedInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpServiceForList<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHttpService<TestEntity>>());
    }

    [Fact]
    public void AddHttpServiceForList_DoesNotRegisterKeyTypeInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpServiceForList<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Null(serviceProvider.GetService<IHttpService<TestEntity, TestEntity, TestEntity, KeyType>>());
    }

    [Fact]
    public void AddHttpServiceForBlazor_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddHttpServiceForBlazor<TestEntity, TestView, TestDto>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddHttpServiceForApi_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddHttpServiceForApi<TestEntity, TestView, TestDto>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddHttpServiceForList_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddHttpServiceForList<TestEntity>(
            sp => new HttpClient(),
            "http://localhost",
            "/api/test");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddHttpServiceForBlazor_ThrowsException_ForInvalidLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddHttpServiceForBlazor<TestEntity, TestView, TestDto>(
                sp => new HttpClient(),
                "http://localhost",
                "/api/test",
                (ServiceLifetime)999));
    }

    private class TestEntity : IEntity, IModel
    {
        public KeyType Id { get; set; }
    }

    private class TestView : IModel
    {
    }

    private class TestDto : IModel
    {
    }
}
