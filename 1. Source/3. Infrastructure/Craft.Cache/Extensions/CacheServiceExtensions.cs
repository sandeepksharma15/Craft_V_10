using Craft.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for registering cache services with dependency injection.
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Adds cache services to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing CacheOptions section.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddCacheServices(configuration.GetSection(CacheOptions.SectionName));
    }

    /// <summary>
    /// Adds cache services to the service collection using a configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">The configuration section containing CacheOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);

        services.AddOptions<CacheOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMemoryCache();

        // Register cache providers
        services.TryAddSingleton<MemoryCacheProvider>();
        services.TryAddSingleton<RedisCacheProvider>();
        services.TryAddSingleton<NullCacheProvider>();

        // Register all providers for factory
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ICacheProvider, MemoryCacheProvider>(sp => 
            sp.GetRequiredService<MemoryCacheProvider>()));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ICacheProvider, RedisCacheProvider>(sp => 
            sp.GetRequiredService<RedisCacheProvider>()));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ICacheProvider, NullCacheProvider>(sp => 
            sp.GetRequiredService<NullCacheProvider>()));

        // Register factory and main service
        services.TryAddSingleton<ICacheProviderFactory, CacheProviderFactory>();
        services.TryAddSingleton<ICacheService, CacheService>();

        // Register key generator and invalidator
        services.TryAddSingleton<ICacheKeyGenerator, CacheKeyGenerator>();
        services.TryAddSingleton<ICacheInvalidator, CacheInvalidator>();

        return services;
    }

    /// <summary>
    /// Adds cache services to the service collection using a configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure cache options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCacheServices(this IServiceCollection services, Action<CacheOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddOptions<CacheOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMemoryCache();

        // Register cache providers
        services.TryAddSingleton<MemoryCacheProvider>();
        services.TryAddSingleton<RedisCacheProvider>();
        services.TryAddSingleton<NullCacheProvider>();

        // Register all providers for factory
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ICacheProvider, MemoryCacheProvider>(sp => 
            sp.GetRequiredService<MemoryCacheProvider>()));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ICacheProvider, RedisCacheProvider>(sp => 
            sp.GetRequiredService<RedisCacheProvider>()));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ICacheProvider, NullCacheProvider>(sp => 
            sp.GetRequiredService<NullCacheProvider>()));

        // Register factory and main service
        services.TryAddSingleton<ICacheProviderFactory, CacheProviderFactory>();
        services.TryAddSingleton<ICacheService, CacheService>();

        // Register key generator and invalidator
        services.TryAddSingleton<ICacheKeyGenerator, CacheKeyGenerator>();
        services.TryAddSingleton<ICacheInvalidator, CacheInvalidator>();

        return services;
    }

    /// <summary>
    /// Adds a custom cache provider.
    /// </summary>
    /// <typeparam name="TProvider">The provider type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCacheProvider<TProvider>(this IServiceCollection services) 
        where TProvider : class, ICacheProvider
    {
        services.TryAddSingleton<TProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ICacheProvider, TProvider>(sp => 
            sp.GetRequiredService<TProvider>()));
        return services;
    }
}

