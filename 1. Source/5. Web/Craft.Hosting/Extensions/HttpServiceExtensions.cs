using Craft.Core;
using Craft.Core.Common;
using Craft.Domain;
using Craft.QuerySpec;

namespace Craft.Hosting.Extensions;

/// <summary>
/// Provides extension methods for registering HTTP services with dependency injection.
/// </summary>
public static class HttpServiceExtensions
{
    /// <summary>
    /// Registers an HttpService with Transient lifetime for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, T, T&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, T, T, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; using HttpService&lt;T&gt; constructor.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTransientHttpService<T>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
    {
        return services.AddTransientHttpService<T, T, T>(
            httpClientFactory,
            baseAddress,
            apiPath,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers an HttpService with Transient lifetime for the specified entity type with separate View and DTO types.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TView">The view model type.</typeparam>
    /// <typeparam name="TDto">The data transfer object type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, TView, TDto&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, TView, TDto, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; as a separate instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTransientHttpService<T, TView, TDto>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
    {
        return AddHttpService<T, TView, TDto>(
            services,
            httpClientFactory,
            baseAddress,
            apiPath,
            ServiceLifetime.Transient,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers an HttpService with Scoped lifetime for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, T, T&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, T, T, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; using HttpService&lt;T&gt; constructor.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddScopedHttpService<T>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
    {
        return services.AddScopedHttpService<T, T, T>(
            httpClientFactory,
            baseAddress,
            apiPath,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers an HttpService with Scoped lifetime for the specified entity type with separate View and DTO types.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TView">The view model type.</typeparam>
    /// <typeparam name="TDto">The data transfer object type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, TView, TDto&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, TView, TDto, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; as a separate instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddScopedHttpService<T, TView, TDto>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
    {
        return AddHttpService<T, TView, TDto>(
            services,
            httpClientFactory,
            baseAddress,
            apiPath,
            ServiceLifetime.Scoped,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers an HttpService with Singleton lifetime for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, T, T&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, T, T, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; using HttpService&lt;T&gt; constructor.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSingletonHttpService<T>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
    {
        return services.AddSingletonHttpService<T, T, T>(
            httpClientFactory,
            baseAddress,
            apiPath,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers an HttpService with Singleton lifetime for the specified entity type with separate View and DTO types.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TView">The view model type.</typeparam>
    /// <typeparam name="TDto">The data transfer object type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, TView, TDto&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, TView, TDto, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; as a separate instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSingletonHttpService<T, TView, TDto>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
    {
        return AddHttpService<T, TView, TDto>(
            services,
            httpClientFactory,
            baseAddress,
            apiPath,
            ServiceLifetime.Singleton,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers a custom HttpService implementation with Transient lifetime.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TService">The custom service implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, T, T&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, T, T, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; as a separate instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTransientCustomHttpService<T, TService>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
        where TService : HttpService<T, T, T, KeyType>
    {
        return services.AddTransientCustomHttpService<T, T, T, TService>(
            httpClientFactory,
            baseAddress,
            apiPath,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers a custom HttpService implementation with Transient lifetime with separate View and DTO types.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TView">The view model type.</typeparam>
    /// <typeparam name="TDto">The data transfer object type.</typeparam>
    /// <typeparam name="TService">The custom service implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, TView, TDto&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, TView, TDto, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; as a separate instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTransientCustomHttpService<T, TView, TDto, TService>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
        where TService : HttpService<T, TView, TDto, KeyType>
    {
        return AddCustomHttpService<T, TView, TDto, TService>(
            services,
            httpClientFactory,
            baseAddress,
            apiPath,
            ServiceLifetime.Transient,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers a custom HttpService implementation with Scoped lifetime.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TService">The custom service implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, T, T&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, T, T, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; as a separate instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddScopedCustomHttpService<T, TService>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
        where TService : HttpService<T, T, T, KeyType>
    {
        return services.AddScopedCustomHttpService<T, T, T, TService>(
            httpClientFactory,
            baseAddress,
            apiPath,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers a custom HttpService implementation with Scoped lifetime with separate View and DTO types.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TView">The view model type.</typeparam>
    /// <typeparam name="TDto">The data transfer object type.</typeparam>
    /// <typeparam name="TService">The custom service implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, TView, TDto&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, TView, TDto, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; as a separate instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddScopedCustomHttpService<T, TView, TDto, TService>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
        where TService : HttpService<T, TView, TDto, KeyType>
    {
        return AddCustomHttpService<T, TView, TDto, TService>(
            services,
            httpClientFactory,
            baseAddress,
            apiPath,
            ServiceLifetime.Scoped,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers a custom HttpService implementation with Singleton lifetime.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TService">The custom service implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, T, T&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, T, T, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; as a separate instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSingletonCustomHttpService<T, TService>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
        where TService : HttpService<T, T, T, KeyType>
    {
        return services.AddSingletonCustomHttpService<T, T, T, TService>(
            httpClientFactory,
            baseAddress,
            apiPath,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    /// <summary>
    /// Registers a custom HttpService implementation with Singleton lifetime with separate View and DTO types.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TView">The view model type.</typeparam>
    /// <typeparam name="TDto">The data transfer object type.</typeparam>
    /// <typeparam name="TService">The custom service implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API (e.g., "https+http://api").</param>
    /// <param name="apiPath">The API path (e.g., "/api/Product").</param>
    /// <param name="registerPrimaryInterface">If true, registers IHttpService&lt;T, TView, TDto&gt;.</param>
    /// <param name="registerWithKeyType">If true, registers IHttpService&lt;T, TView, TDto, KeyType&gt;.</param>
    /// <param name="registerSimplified">If true, registers IHttpService&lt;T&gt; as a separate instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSingletonCustomHttpService<T, TView, TDto, TService>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        bool registerPrimaryInterface = true,
        bool registerWithKeyType = false,
        bool registerSimplified = false)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
        where TService : HttpService<T, TView, TDto, KeyType>
    {
        return AddCustomHttpService<T, TView, TDto, TService>(
            services,
            httpClientFactory,
            baseAddress,
            apiPath,
            ServiceLifetime.Singleton,
            registerPrimaryInterface,
            registerWithKeyType,
            registerSimplified);
    }

    #region Private Helper Methods

    /// <summary>
    /// Core method to register an HttpService with specified lifetime and interface options.
    /// </summary>
    private static IServiceCollection AddHttpService<T, TView, TDto>(
        IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        ServiceLifetime lifetime,
        bool registerPrimaryInterface,
        bool registerWithKeyType,
        bool registerSimplified)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
    {
        var apiUrl = new Uri(new Uri(baseAddress), apiPath);

        // Create factory for HttpService<T, TView, TDto>
        Func<IServiceProvider, HttpService<T, TView, TDto>> factory = provider =>
        {
            var httpClient = httpClientFactory(provider);
            var logger = provider.GetRequiredService<ILogger<HttpService<T, TView, TDto>>>();
            return new HttpService<T, TView, TDto>(apiUrl, httpClient, logger);
        };

        // Register primary interface IHttpService<T, TView, TDto>
        if (registerPrimaryInterface)
            RegisterService<IHttpService<T, TView, TDto>>(services, factory, lifetime);

        // Register interface with explicit KeyType parameter IHttpService<T, TView, TDto, KeyType>
        if (registerWithKeyType)
            RegisterService<IHttpService<T, TView, TDto, KeyType>>(services, factory, lifetime);

        // Register simplified interface IHttpService<T> as separate instance
        if (registerSimplified)
        {
            Func<IServiceProvider, HttpService<T>> simplifiedFactory = provider =>
            {
                var httpClient = httpClientFactory(provider);
                var logger = provider.GetRequiredService<ILogger<HttpService<T>>>();
                return new HttpService<T>(apiUrl, httpClient, logger);
            };

            RegisterService<IHttpService<T>>(services, simplifiedFactory, lifetime);
        }

        return services;
    }

    /// <summary>
    /// Core method to register a custom HttpService implementation with specified lifetime and interface options.
    /// </summary>
    private static IServiceCollection AddCustomHttpService<T, TView, TDto, TService>(
        IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        ServiceLifetime lifetime,
        bool registerPrimaryInterface,
        bool registerWithKeyType,
        bool registerSimplified)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
        where TService : HttpService<T, TView, TDto, KeyType>
    {
        var apiUrl = new Uri(new Uri(baseAddress), apiPath);

        // Create factory for custom TService
        Func<IServiceProvider, TService> factory = provider =>
        {
            var httpClient = httpClientFactory(provider);
            var logger = provider.GetRequiredService<ILogger<HttpService<T, TView, TDto, KeyType>>>();
            return (TService)Activator.CreateInstance(typeof(TService), apiUrl, httpClient, logger)!;
        };

        // Register the custom service type itself
        RegisterService(services, factory, lifetime);

        // Register primary interface IHttpService<T, TView, TDto> (forwarding to TService)
        if (registerPrimaryInterface)
        {
            RegisterService<IHttpService<T, TView, TDto>>(
                services,
                provider => (IHttpService<T, TView, TDto>)provider.GetRequiredService<TService>(),
                lifetime);
        }

        // Register interface with explicit KeyType parameter IHttpService<T, TView, TDto, KeyType> (forwarding to TService)
        if (registerWithKeyType)
        {
            RegisterService<IHttpService<T, TView, TDto, KeyType>>(
                services,
                provider => (IHttpService<T, TView, TDto, KeyType>)provider.GetRequiredService<TService>(),
                lifetime);
        }

        // Register simplified interface IHttpService<T> as separate instance
        if (registerSimplified)
        {
            Func<IServiceProvider, HttpService<T>> simplifiedFactory = provider =>
            {
                var httpClient = httpClientFactory(provider);
                var logger = provider.GetRequiredService<ILogger<HttpService<T>>>();
                return new HttpService<T>(apiUrl, httpClient, logger);
            };

            RegisterService<IHttpService<T>>(services, simplifiedFactory, lifetime);
        }

        return services;
    }

    /// <summary>
    /// Helper method to register a service with specified lifetime.
    /// </summary>
    private static void RegisterService<TService>(
        IServiceCollection services,
        Func<IServiceProvider, TService> factory,
        ServiceLifetime lifetime)
        where TService : class
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                services.AddTransient(factory);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(factory);
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton(factory);
                break;
            default:
                throw new ArgumentException($"Unsupported service lifetime: {lifetime}", nameof(lifetime));
        }
    }

    #endregion
}
