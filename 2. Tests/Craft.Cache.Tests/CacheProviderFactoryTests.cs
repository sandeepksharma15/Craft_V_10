using Craft.Cache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Cache.Tests;

public class CacheProviderFactoryTests
{
    [Fact]
    public void GetDefaultProvider_ReturnsConfiguredProvider()
    {
        // Arrange
        var memoryProvider = new Mock<ICacheProvider>();
        memoryProvider.Setup(p => p.Name).Returns("memory");

        var providers = new List<ICacheProvider> { memoryProvider.Object };
        var options = new CacheOptions { Provider = "memory" };
        var optionsMock = new Mock<IOptions<CacheOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var logger = new Mock<ILogger<CacheProviderFactory>>();
        var factory = new CacheProviderFactory(providers, optionsMock.Object, logger.Object);

        // Act
        var provider = factory.GetDefaultProvider();

        // Assert
        Assert.NotNull(provider);
        Assert.Equal("memory", provider.Name);
    }

    [Fact]
    public void GetProvider_WithValidName_ReturnsProvider()
    {
        // Arrange
        var redisProvider = new Mock<ICacheProvider>();
        redisProvider.Setup(p => p.Name).Returns("redis");

        var providers = new List<ICacheProvider> { redisProvider.Object };
        var options = new CacheOptions { Provider = "memory" };
        var optionsMock = new Mock<IOptions<CacheOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var logger = new Mock<ILogger<CacheProviderFactory>>();
        var factory = new CacheProviderFactory(providers, optionsMock.Object, logger.Object);

        // Act
        var provider = factory.GetProvider("redis");

        // Assert
        Assert.NotNull(provider);
        Assert.Equal("redis", provider.Name);
    }

    [Fact]
    public void GetProvider_WithInvalidName_ThrowsInvalidOperationException()
    {
        // Arrange
        var providers = new List<ICacheProvider>();
        var options = new CacheOptions { Provider = "memory" };
        var optionsMock = new Mock<IOptions<CacheOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var logger = new Mock<ILogger<CacheProviderFactory>>();
        var factory = new CacheProviderFactory(providers, optionsMock.Object, logger.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => factory.GetProvider("nonexistent"));
    }

    [Fact]
    public void GetAllProviders_ReturnsAllRegisteredProviders()
    {
        // Arrange
        var memoryProvider = new Mock<ICacheProvider>();
        memoryProvider.Setup(p => p.Name).Returns("memory");
        var redisProvider = new Mock<ICacheProvider>();
        redisProvider.Setup(p => p.Name).Returns("redis");

        var providers = new List<ICacheProvider> { memoryProvider.Object, redisProvider.Object };
        var options = new CacheOptions { Provider = "memory" };
        var optionsMock = new Mock<IOptions<CacheOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var logger = new Mock<ILogger<CacheProviderFactory>>();
        var factory = new CacheProviderFactory(providers, optionsMock.Object, logger.Object);

        // Act
        var allProviders = factory.GetAllProviders().ToList();

        // Assert
        Assert.Equal(2, allProviders.Count);
        Assert.Contains(allProviders, p => p.Name == "memory");
        Assert.Contains(allProviders, p => p.Name == "redis");
    }

    [Fact]
    public void Constructor_WithNullProviders_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new CacheOptions();
        var optionsMock = new Mock<IOptions<CacheOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);
        var logger = new Mock<ILogger<CacheProviderFactory>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CacheProviderFactory(null!, optionsMock.Object, logger.Object));
    }

    [Fact]
    public void GetProvider_IsCaseInsensitive()
    {
        // Arrange
        var memoryProvider = new Mock<ICacheProvider>();
        memoryProvider.Setup(p => p.Name).Returns("memory");

        var providers = new List<ICacheProvider> { memoryProvider.Object };
        var options = new CacheOptions { Provider = "memory" };
        var optionsMock = new Mock<IOptions<CacheOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var logger = new Mock<ILogger<CacheProviderFactory>>();
        var factory = new CacheProviderFactory(providers, optionsMock.Object, logger.Object);

        // Act
        var provider1 = factory.GetProvider("MEMORY");
        var provider2 = factory.GetProvider("Memory");
        var provider3 = factory.GetProvider("memory");

        // Assert
        Assert.NotNull(provider1);
        Assert.NotNull(provider2);
        Assert.NotNull(provider3);
        Assert.Same(provider1, provider2);
        Assert.Same(provider2, provider3);
    }
}
