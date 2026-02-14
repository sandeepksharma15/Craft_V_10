using Craft.CryptKey;
using Craft.Hosting.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Hosting.Tests.Extensions.Security;

/// <summary>
/// Unit tests for CryptKeyExtensions.
/// </summary>
public class CryptKeyExtensionsTests
{
    [Fact]
    public void AddHashKeys_WithConfiguration_RegistersHashKeysService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.CryptKeyExtensions.AddHashKeys(services, options =>
        {
            options.Salt = "TestSalt";
            options.MinHashLength = 8;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHashKeys>());
    }

    [Fact]
    public void AddHashKeys_WithoutConfiguration_RegistersHashKeysService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.CryptKeyExtensions.AddHashKeys(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IHashKeys>());
    }

    [Fact]
    public void AddHashKeys_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        const string testSalt = "MySecretSalt";
        const int minLength = 10;

        // Act
        Craft.Hosting.Extensions.CryptKeyExtensions.AddHashKeys(services, options =>
        {
            options.Salt = testSalt;
            options.MinHashLength = minLength;
        });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HashKeyOptions>>().Value;

        // Assert
        Assert.Equal(testSalt, options.Salt);
        Assert.Equal(minLength, options.MinHashLength);
    }

    [Fact]
    public void AddHashKeys_RegistersOptionsAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.CryptKeyExtensions.AddHashKeys(services, options =>
        {
            options.Salt = "TestSalt";
        });

        var serviceProvider = services.BuildServiceProvider();
        var options1 = serviceProvider.GetRequiredService<HashKeyOptions>();
        var options2 = serviceProvider.GetRequiredService<HashKeyOptions>();

        // Assert
        Assert.Same(options1, options2);
    }

    [Fact]
    public void AddHashKeys_RegistersHashKeysWithSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.CryptKeyExtensions.AddHashKeys(services);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHashKeys));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddHashKeys_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = Craft.Hosting.Extensions.CryptKeyExtensions.AddHashKeys(services);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddHashKeys_CreatesHashKeysWithConfiguredOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        const string testSalt = "TestSalt123";

        // Act
        Craft.Hosting.Extensions.CryptKeyExtensions.AddHashKeys(services, options =>
        {
            options.Salt = testSalt;
        });

        var serviceProvider = services.BuildServiceProvider();
        var hashKeys = serviceProvider.GetRequiredService<IHashKeys>();

        // Assert - Verify the instance is created (testing actual functionality would require calling methods)
        Assert.NotNull(hashKeys);
    }
}
