using Craft.Core;
using Craft.Domain;
using Craft.Hosting.Extensions;
using Craft.Security;
using Craft.Security.Tokens;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Hosting.Tests.Extensions.Security;

/// <summary>
/// Unit tests for SecurityExtensions.
/// </summary>
public class SecurityExtensionsTests
{
    [Fact]
    public void AddCurrentApiUser_RegistersApiUserProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.SecurityExtensions.AddCurrentApiUser(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var userProvider = serviceProvider.GetService<ICurrentUserProvider>();
        Assert.NotNull(userProvider);
        Assert.IsType<ApiUserProvider>(userProvider);
    }

    [Fact]
    public void AddCurrentApiUser_RegistersCurrentUser()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.SecurityExtensions.AddCurrentApiUser(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ICurrentUser>());
    }

    [Fact]
    public void AddCurrentApiUser_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = Craft.Hosting.Extensions.SecurityExtensions.AddCurrentApiUser(services);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCurrentUiUser_RegistersUiUserProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.SecurityExtensions.AddCurrentUiUser(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var userProvider = serviceProvider.GetService<ICurrentUserProvider>();
        Assert.NotNull(userProvider);
        Assert.IsType<UiUserProvider>(userProvider);
    }

    [Fact]
    public void AddCurrentUiUser_RegistersCurrentUser()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.SecurityExtensions.AddCurrentUiUser(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ICurrentUser>());
    }

    [Fact]
    public void AddCurrentUiUser_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = Craft.Hosting.Extensions.SecurityExtensions.AddCurrentUiUser(services);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCraftSecurity_RegistersTokenManager()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.SecurityExtensions.AddCraftSecurity(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ITokenManager>());
    }

    [Fact]
    public void AddCraftSecurity_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = Craft.Hosting.Extensions.SecurityExtensions.AddCraftSecurity(services);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCurrentApiUser_RegistersWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.SecurityExtensions.AddCurrentApiUser(services);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICurrentUserProvider));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCurrentUiUser_RegistersWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.SecurityExtensions.AddCurrentUiUser(services);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICurrentUserProvider));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCraftSecurity_RegistersWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.SecurityExtensions.AddCraftSecurity(services);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITokenManager));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
