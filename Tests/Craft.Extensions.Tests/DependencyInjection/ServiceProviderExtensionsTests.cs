using Microsoft.Extensions.DependencyInjection;

namespace Craft.Extensions.Tests.DependencyInjection;

public class ServiceProviderExtensionsTests
{
    [Fact]
    public void AddService_ThrowsOnNullArguments()
    {
        // Arrange
        var services = new ServiceCollection();
        var type = typeof(ITestService);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceProviderExtensions.AddService(null!, type, type, ServiceLifetime.Singleton));
        Assert.Throws<ArgumentNullException>(() => services.AddService(null!, type, ServiceLifetime.Singleton));
        Assert.Throws<ArgumentNullException>(() => services.AddService(type, null!, ServiceLifetime.Singleton));
    }

    [Fact]
    public void AddService_ThrowsOnInvalidLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        var type = typeof(ITestService);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddService(type, typeof(TestService), (ServiceLifetime)999));
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    public void AddService_RegistersServiceWithCorrectLifetime(ServiceLifetime lifetime)
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceType = typeof(ITestService);
        var implementationType = typeof(TestService);

        // Act
        services.AddService(serviceType, implementationType, lifetime);
        var descriptor = services.FirstOrDefault(d => d.ServiceType == serviceType && d.ImplementationType == implementationType);

        // Assert
        Assert.NotNull(descriptor);
        Assert.Equal(lifetime, descriptor.Lifetime);
    }

    [Fact]
    public void AddServices_ThrowsOnNullArguments()
    {
        // Arrange
        var services = new ServiceCollection();
        var type = typeof(ITestService);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceProviderExtensions.AddServices(null!, type, ServiceLifetime.Singleton));
        Assert.Throws<ArgumentNullException>(() => services.AddServices(null!, ServiceLifetime.Singleton));
    }

    [Fact]
    public void AddServices_RegistersAllImplementationsOfInterface()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServices(typeof(ITestService), ServiceLifetime.Transient);

        // Assert
        Assert.Contains(services, d => d.ImplementationType == typeof(TestService1) && d.ServiceType == typeof(ITestService));
        Assert.Contains(services, d => d.ImplementationType == typeof(TestService2) && d.ServiceType == typeof(ITestService));
    }

    [Fact]
    public void AddServices_DoesNotRegisterForUnrelatedInterface()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServices(typeof(IDisposable), ServiceLifetime.Transient);

        // Assert
        Assert.DoesNotContain(services, d => d.ImplementationType == typeof(TestService1) && d.ServiceType == typeof(IDisposable));
        Assert.DoesNotContain(services, d => d.ImplementationType == typeof(TestService2) && d.ServiceType == typeof(IDisposable));
    }

    [Fact]
    public void GetSingletonInstance_ThrowsOnNullArgument()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceProviderExtensions.GetSingletonInstance<TestService>(null!));
    }

    [Fact]
    public void GetSingletonInstance_ThrowsIfNotSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        services.AddTransient<TestService>();

        // Assert
        Assert.Throws<InvalidOperationException>(() => services.GetSingletonInstance<TestService>());
    }

    [Fact]
    public void GetSingletonInstance_ReturnsSingletonInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var instance = new TestService();

        // Act
        services.AddSingleton(instance);
        var result = services.GetSingletonInstance<TestService>();

        // Assert
        Assert.Same(instance, result);
    }

    [Fact]
    public void GetSingletonInstanceOrNull_ThrowsOnNullArgument()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceProviderExtensions.GetSingletonInstanceOrNull<TestService>(null!));
    }

    [Fact]
    public void GetSingletonInstanceOrNull_ReturnsNullIfNotSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTransient<TestService>();
        var result = services.GetSingletonInstanceOrNull<TestService>();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetSingletonInstanceOrNull_ReturnsInstanceIfSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var instance = new TestService();

        // Act
        services.AddSingleton(instance);
        var result = services.GetSingletonInstanceOrNull<TestService>();

        // Assert
        Assert.Same(instance, result);
    }

    [Fact]
    public void IsAdded_ThrowsOnNullArgument()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceProviderExtensions.IsAdded(null!, typeof(TestService)));
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.IsAdded(null!));
    }

    [Fact]
    public void IsAdded_ReturnsFalseIfNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.False(services.IsAdded<TestService>());
#pragma warning disable CA2263 // Prefer generic overload when type is known
        Assert.False(services.IsAdded(typeof(TestService)));
#pragma warning restore CA2263 // Prefer generic overload when type is known
    }

    [Fact]
    public void IsAdded_ReturnsTrueIfRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSingleton<TestService>();

        // Assert
        Assert.True(services.IsAdded<TestService>());
#pragma warning disable CA2263 // Prefer generic overload when type is known
        Assert.True(services.IsAdded(typeof(TestService)));
#pragma warning restore CA2263 // Prefer generic overload when type is known
    }

    [Fact]
    public void IsImplementationAdded_ThrowsOnNullArgument()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceProviderExtensions.IsImplementationAdded(null!, typeof(TestService)));

        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.IsImplementationAdded(null!));
    }

    [Fact]
    public void IsImplementationAdded_ReturnsFalseIfNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.False(services.IsImplementationAdded<TestService>());
#pragma warning disable CA2263 // Prefer generic overload when type is known
        Assert.False(services.IsImplementationAdded(typeof(TestService)));
#pragma warning restore CA2263 // Prefer generic overload when type is known
    }

    [Fact]
    public void IsImplementationAdded_ReturnsTrueIfRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSingleton<TestService>();

        // Assert
        Assert.True(services.IsImplementationAdded<TestService>());
#pragma warning disable CA2263 // Prefer generic overload when type is known
        Assert.True(services.IsImplementationAdded(typeof(TestService)));
#pragma warning restore CA2263 // Prefer generic overload when type is known
    }

    [Fact]
    public void IsImplementationAdded_ReturnsFalseForInterfaceRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSingleton<ITestService>(new TestService1());

        // Assert
        Assert.False(services.IsImplementationAdded<TestService>());
    }

    [Fact]
    public void ResolveWith_ThrowsOnNullProvider()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceProviderExtensions.ResolveWith<TestService>(null!));
    }

    [Fact]
    public void ResolveWith_ResolvesInstanceWithoutParameters()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTransient<TestService>();
        var provider = services.BuildServiceProvider();
        var instance = provider.ResolveWith<TestService>();

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<TestService>(instance);
    }

    [Fact]
    public void ResolveWith_ResolvesInstanceWithParameters()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTransient<TestService3>();
        services.AddTransient<ITestService, TestService>();
        var provider = services.BuildServiceProvider();
        var dependency = provider.GetService<ITestService>();
        var instance = provider.ResolveWith<TestService3>(dependency!);

        // Assert
        Assert.NotNull(instance);
        Assert.Same(dependency, instance.TestService);
    }

    private interface ITestService { }
    private class TestService : ITestService { }
    private class TestService1 : ITestService { }
    private class TestService2 : ITestService { }
    private class TestService3
    {
        public ITestService TestService { get; }
        public TestService3(ITestService testService = null!) => TestService = testService;
    }
}
