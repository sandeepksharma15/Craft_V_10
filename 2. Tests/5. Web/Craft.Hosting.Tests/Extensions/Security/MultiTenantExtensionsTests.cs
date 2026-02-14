using Craft.Core;
using Craft.Domain;
using Craft.Hosting.Extensions;
using Craft.MultiTenant;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Hosting.Tests.Extensions.Security;

/// <summary>
/// Unit tests for MultiTenantExtensions.
/// </summary>
public class MultiTenantExtensionsTests
{
    [Fact]
    public void AddMultiTenant_WithCustomTenant_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services, options =>
        {
            // Empty configuration
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ITenantResolver<Tenant>>());
        Assert.NotNull(serviceProvider.GetService<ITenantResolver>());
        Assert.NotNull(serviceProvider.GetService<ITenantContextAccessor<Tenant>>());
        Assert.NotNull(serviceProvider.GetService<ITenantContextAccessor>());
    }

    [Fact]
    public void AddMultiTenant_WithDefaultConfiguration_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ITenantResolver<Tenant>>());
        Assert.NotNull(serviceProvider.GetService<ITenantResolver>());
    }

    [Fact]
    public void AddMultiTenant_WithDefaultTenantType_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ITenantResolver<Tenant>>());
        Assert.NotNull(serviceProvider.GetService<ITenantResolver>());
    }

    [Fact]
    public void AddMultiTenant_RegistersTenantResolverWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITenantResolver<Tenant>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddMultiTenant_RegistersTenantContextAccessorWithSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITenantContextAccessor<Tenant>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddMultiTenant_RegistersCurrentTenant()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ICurrentTenant>());
        Assert.NotNull(serviceProvider.GetService<ICurrentTenant<KeyType>>());
    }

    [Fact]
    public void AddMultiTenant_RegistersCurrentTenantWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICurrentTenant));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddMultiTenant_RegistersTenantType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(Tenant));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddMultiTenant_RegistersITenant()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITenant));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddMultiTenant_ReturnsTenantBuilder()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TenantBuilder<Tenant>>(result);
    }

    [Fact]
    public void AddMultiTenant_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.MultiTenantExtensions.AddMultiTenant<Tenant>(services, options =>
        {
            // Configuration action - just verify it's called
        });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<TenantOptions>>().Value;

        // Assert - Just verify options are available
        Assert.NotNull(options);
    }
}
