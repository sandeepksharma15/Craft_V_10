using Craft.Core;
using Craft.Core.Common;
using Craft.Domain;
using Microsoft.Extensions.Configuration;

namespace Craft.Hosting.Extensions;

/// <summary>
/// Provides convenience extension methods for common HTTP service registration scenarios.
/// These methods simplify the registration of HTTP services for typical use cases.
/// </summary>
public static class HttpServiceConvenienceExtensions
{
    /// <summary>
    /// Registers an HttpService with optimized interface registration for Blazor web applications.
    /// Registers IHttpService&lt;T, TView, TDto, KeyType&gt; for edit components and IHttpService&lt;T&gt; for list components.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TView">The view model type.</typeparam>
    /// <typeparam name="TDto">The data transfer object type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API.</param>
    /// <param name="apiPath">The API path.</param>
    /// <param name="lifetime">The service lifetime (default: Transient).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpServiceForBlazor<T, TView, TDto>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
    {
        return lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransientHttpService<T, TView, TDto>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,    // Not used
                registerWithKeyType: true,          // Used by BaseEditComponent
                registerSimplified: true),          // Used by List components

            ServiceLifetime.Scoped => services.AddScopedHttpService<T, TView, TDto>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: true,
                registerSimplified: true),

            ServiceLifetime.Singleton => services.AddSingletonHttpService<T, TView, TDto>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: true,
                registerSimplified: true),

            _ => throw new ArgumentException($"Unsupported service lifetime: {lifetime}", nameof(lifetime))
        };
    }

    /// <summary>
    /// Registers a custom HttpService implementation with optimized interface registration for Blazor web applications.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TView">The view model type.</typeparam>
    /// <typeparam name="TDto">The data transfer object type.</typeparam>
    /// <typeparam name="TService">The custom service implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API.</param>
    /// <param name="apiPath">The API path.</param>
    /// <param name="lifetime">The service lifetime (default: Transient).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCustomHttpServiceForBlazor<T, TView, TDto, TService>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
        where TService : Craft.QuerySpec.HttpService<T, TView, TDto, KeyType>
    {
        return lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransientCustomHttpService<T, TView, TDto, TService>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: true,
                registerSimplified: true),

            ServiceLifetime.Scoped => services.AddScopedCustomHttpService<T, TView, TDto, TService>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: true,
                registerSimplified: true),

            ServiceLifetime.Singleton => services.AddSingletonCustomHttpService<T, TView, TDto, TService>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: true,
                registerSimplified: true),

            _ => throw new ArgumentException($"Unsupported service lifetime: {lifetime}", nameof(lifetime))
        };
    }

    /// <summary>
    /// Registers an HttpService for API-only scenarios (no UI components).
    /// Registers only IHttpService&lt;T, TView, TDto, KeyType&gt; interface.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TView">The view model type.</typeparam>
    /// <typeparam name="TDto">The data transfer object type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API.</param>
    /// <param name="apiPath">The API path.</param>
    /// <param name="lifetime">The service lifetime (default: Transient).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpServiceForApi<T, TView, TDto>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, IEntity, IModel, new()
        where TView : class, IModel, new()
        where TDto : class, IModel, new()
    {
        return lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransientHttpService<T, TView, TDto>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: true,          // Primary interface for API
                registerSimplified: false),

            ServiceLifetime.Scoped => services.AddScopedHttpService<T, TView, TDto>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: true,
                registerSimplified: false),

            ServiceLifetime.Singleton => services.AddSingletonHttpService<T, TView, TDto>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: true,
                registerSimplified: false),

            _ => throw new ArgumentException($"Unsupported service lifetime: {lifetime}", nameof(lifetime))
        };
    }

    /// <summary>
    /// Registers an HttpService for list-only scenarios (read-only components).
    /// Registers only IHttpService&lt;T&gt; interface.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to get the HttpClient instance.</param>
    /// <param name="baseAddress">The base address for the API.</param>
    /// <param name="apiPath">The API path.</param>
    /// <param name="lifetime">The service lifetime (default: Transient).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpServiceForList<T>(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory,
        string baseAddress,
        string apiPath,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, IEntity, IModel, new()
    {
        return lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransientHttpService<T>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: false,
                registerSimplified: true),          // Only list interface

            ServiceLifetime.Scoped => services.AddScopedHttpService<T>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: false,
                registerSimplified: true),

            ServiceLifetime.Singleton => services.AddSingletonHttpService<T>(
                httpClientFactory, baseAddress, apiPath,
                registerPrimaryInterface: false,
                registerWithKeyType: false,
                registerSimplified: true),

            _ => throw new ArgumentException($"Unsupported service lifetime: {lifetime}", nameof(lifetime))
        };
    }
}
