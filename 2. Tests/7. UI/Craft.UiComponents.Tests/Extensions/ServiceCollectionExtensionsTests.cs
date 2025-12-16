using Craft.UiConponents;
using Craft.UiConponents.Enums;
using Craft.UiConponents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.UiComponents.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCraftUiComponents_ShouldRegisterThemeService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftUiComponents();
        var provider = services.BuildServiceProvider();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IThemeService));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCraftUiComponents_WithConfiguration_ShouldRegisterOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftUiComponents(options =>
        {
            options.DefaultTheme = Theme.Dark;
            options.EnableLogging = true;
            options.DefaultAnimationDuration = AnimationDuration.Fast;
            options.CssPrefix = "custom";
        });

        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<CraftUiOptions>();

        Assert.NotNull(options);
        Assert.Equal(Theme.Dark, options.DefaultTheme);
        Assert.True(options.EnableLogging);
        Assert.Equal(AnimationDuration.Fast, options.DefaultAnimationDuration);
        Assert.Equal("custom", options.CssPrefix);
    }

    [Fact]
    public void AddCraftUiComponents_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCraftUiComponents();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCraftUiComponents_WithConfiguration_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCraftUiComponents(o => { });

        // Assert
        Assert.Same(services, result);
    }
}
