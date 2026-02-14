using Craft.Hosting.Extensions;
using Craft.UiBuilders.Services.Theme;
using Craft.UiBuilders.Services.UserPreference;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Hosting.Tests.Extensions.Web;

/// <summary>
/// Unit tests for UiBuilderExtensions.
/// </summary>
public class UiBuilderExtensionsTests
{
    [Fact]
    public void AddUserPreferences_RegistersUserPreferencesManager()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddUserPreferences();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IUserPreferencesManager>());
    }

    [Fact]
    public void AddUserPreferences_RegistersUserPreferences()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddUserPreferences();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IUserPreferences>());
    }

    [Fact]
    public void AddUserPreferences_WithApplicationName_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        const string appName = "TestApp";

        // Act
        services.AddUserPreferences(appName);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IUserPreferencesManager>());
        Assert.NotNull(serviceProvider.GetService<IUserPreferences>());
    }

    [Fact]
    public void AddUserPreferences_RegistersWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddUserPreferences();

        // Assert
        var managerDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IUserPreferencesManager));
        var preferencesDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IUserPreferences));
        
        Assert.NotNull(managerDescriptor);
        Assert.NotNull(preferencesDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, managerDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, preferencesDescriptor.Lifetime);
    }

    [Fact]
    public void AddUserPreferences_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddUserPreferences();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddThemeManager_RegistersThemeManager()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddThemeManager();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IThemeManager>());
    }

    [Fact]
    public void AddThemeManager_RegistersWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddThemeManager();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IThemeManager));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddThemeManager_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddThemeManager();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddUiBuilders_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddUiBuilders();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IUserPreferencesManager>());
        Assert.NotNull(serviceProvider.GetService<IUserPreferences>());
        Assert.NotNull(serviceProvider.GetService<IThemeManager>());
    }

    [Fact]
    public void AddUiBuilders_WithApplicationName_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        const string appName = "MyApplication";

        // Act
        services.AddUiBuilders(appName);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IUserPreferencesManager>());
        Assert.NotNull(serviceProvider.GetService<IUserPreferences>());
        Assert.NotNull(serviceProvider.GetService<IThemeManager>());
    }

    [Fact]
    public void AddUiBuilders_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddUiBuilders();

        // Assert
        Assert.Same(services, result);
    }
}
