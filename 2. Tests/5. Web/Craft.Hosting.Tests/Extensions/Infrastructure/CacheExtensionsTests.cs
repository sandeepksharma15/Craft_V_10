using Craft.Cache;
using Craft.Hosting.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Hosting.Tests.Extensions.Infrastructure;

/// <summary>
/// Unit tests for CacheExtensions.
/// </summary>
public class CacheExtensionsTests
{
    [Fact]
    public void AddCacheServices_WithConfiguration_RegistersAllServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CacheOptions:Provider"] = "memory",
                ["CacheOptions:DefaultExpiration"] = "01:00:00"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ICacheService>());
        Assert.NotNull(serviceProvider.GetService<ICacheProviderFactory>());
        Assert.NotNull(serviceProvider.GetService<ICacheKeyGenerator>());
        Assert.NotNull(serviceProvider.GetService<ICacheInvalidator>());
    }

    [Fact]
    public void AddCacheServices_WithConfigurationSection_RegistersAllServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CacheOptions:Provider"] = "memory",
                ["CacheOptions:DefaultExpiration"] = "01:00:00"
            })
            .Build();

        var services = new ServiceCollection();
        var section = configuration.GetSection("CacheOptions");

        // Act
        Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, section);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ICacheService>());
        Assert.NotNull(serviceProvider.GetService<ICacheProviderFactory>());
        Assert.NotNull(serviceProvider.GetService<ICacheKeyGenerator>());
        Assert.NotNull(serviceProvider.GetService<ICacheInvalidator>());
    }

    [Fact]
    public void AddCacheServices_WithAction_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, options =>
        {
            options.Provider = "memory";
            options.DefaultExpiration = TimeSpan.FromHours(1);
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ICacheService>());
        Assert.NotNull(serviceProvider.GetService<ICacheProviderFactory>());
        Assert.NotNull(serviceProvider.GetService<ICacheKeyGenerator>());
        Assert.NotNull(serviceProvider.GetService<ICacheInvalidator>());
    }

    [Fact]
    public void AddCacheServices_RegistersAllProviders()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, options =>
        {
            options.Provider = "memory";
        });

        var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<ICacheProvider>();

        // Assert
        Assert.Contains(providers, p => p is MemoryCacheProvider);
        Assert.Contains(providers, p => p is RedisCacheProvider);
        Assert.Contains(providers, p => p is NullCacheProvider);
    }

    [Fact]
    public void AddCacheServices_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection services = null!;
        var configuration = new ConfigurationBuilder().Build().GetSection("CacheOptions");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, configuration));
    }

    [Fact]
    public void AddCacheServices_ThrowsArgumentNullException_WhenConfigurationSectionIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationSection configurationSection = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, configurationSection));
    }

    [Fact]
    public void AddCacheServices_ThrowsArgumentNullException_WhenConfigureOptionsIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<CacheOptions> configureOptions = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, configureOptions));
    }

    [Fact]
    public void AddCacheProvider_RegistersCustomProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, options => options.Provider = "memory");
        Craft.Hosting.Extensions.CacheExtensions.AddCacheProvider<TestCacheProvider>(services);

        var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<ICacheProvider>();

        // Assert
        Assert.Contains(providers, p => p is TestCacheProvider);
    }

    [Fact]
    public void AddCacheServices_ConfiguresOptions_WithValidation()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CacheOptions:Provider"] = "memory"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CacheOptions>>().Value;

        // Assert
        Assert.Equal("memory", options.Provider);
    }

    [Fact]
    public void AddCacheServices_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = Craft.Hosting.Extensions.CacheExtensions.AddCacheServices(services, configuration);

        // Assert
        Assert.Same(services, result);
    }

    private class TestCacheProvider : ICacheProvider
    {
        public string Name => "test";
        public bool IsConfigured() => true;

        public Task<CacheResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(CacheResult<T>.Success());

        public Task<CacheResult> SetAsync<T>(string key, T? value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult(CacheResult.Success());

        public Task<CacheResult> RemoveAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(CacheResult.Success());

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
            => Task.FromResult<IDictionary<string, T?>>(new Dictionary<string, T?>());

        public Task<CacheResult> SetManyAsync<T>(IDictionary<string, T?> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult(CacheResult.Success());

        public Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<CacheStats> GetStatsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new CacheStats());

        public Task<CacheResult> ClearAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CacheResult.Success());

        public Task<CacheResult> RefreshAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(CacheResult.Success());
    }
}
