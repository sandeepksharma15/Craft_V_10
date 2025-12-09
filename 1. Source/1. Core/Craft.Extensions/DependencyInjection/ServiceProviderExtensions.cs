using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Adds a service to the dependency injection container with the specified service lifetime.
    /// Provides a concise syntax for registering services with different lifetimes.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the service to.</param>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The type of the concrete implementation to register.</param>
    /// <param name="lifetime">The desired lifetime of the service (Transient, Scoped, or Singleton).</param>
    /// <returns>The IServiceCollection with the added service.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services, serviceType, or implementationType is null.</exception>
    /// <exception cref="ArgumentException">Thrown if an invalid lifetime is specified.</exception>
    public static IServiceCollection AddService(this IServiceCollection services,
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);

        return lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
            ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
            ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
            _ => throw new ArgumentException($"Invalid ServiceLifetime: {lifetime}", nameof(lifetime))
        };
    }

    /// <summary>
    /// Automatically registers all concrete implementations of a specified interface within the current domain as services in the dependency injection container.
    /// Uses reflection to discover and register services, promoting convention-based registration and reducing manual setup.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="interfaceType">The type of the interface to register implementations for.</param>
    /// <param name="lifetime">The desired lifetime of the registered services.</param>
    /// <returns>The IServiceCollection with the added services.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services or interfaceType is null.</exception>
    public static IServiceCollection AddServices(this IServiceCollection services,
        Type interfaceType,
        ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(interfaceType);

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
            })
            .Where(t => t is not null
                        && interfaceType.IsAssignableFrom(t)
                        && t.IsClass
                        && !t.IsAbstract)
            .ToList();

        foreach (var implType in types)
            // Register for each interface implemented that matches the requested interfaceType
            foreach (var service in implType!.GetInterfaces().Where(interfaceType.IsAssignableFrom))
                services.AddService(service, implType, lifetime);

        return services;
    }

    /// <summary>
    /// Retrieves a singleton instance of the specified service type from the dependency injection container,
    /// throwing an exception if the service is not registered as a singleton.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <param name="services">The IServiceCollection to get the service from.</param>
    /// <returns>The singleton instance of the service.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the service is not registered as a singleton.</exception>
    public static T GetSingletonInstance<T>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var instance = services.GetSingletonInstanceOrNull<T>()
            ?? throw new InvalidOperationException($"Could not find singleton service: {typeof(T).FullName}");

        return instance;
    }

    /// <summary>
    /// Retrieves a singleton instance of the specified service type from the dependency injection container,
    /// returning null if the service is not registered or is not a singleton.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <param name="services">The IServiceCollection to get the service from.</param>
    /// <returns>The singleton instance of the service, or null if not found or not registered as a singleton.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services is null.</exception>
    public static T? GetSingletonInstanceOrNull<T>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var descriptor = services
            .FirstOrDefault(d => d.ServiceType == typeof(T) && d.Lifetime == ServiceLifetime.Singleton);

        return descriptor?.ImplementationInstance is T instance ? instance : default;
    }

    /// <summary>
    /// Determines whether a service of the specified type has been added to the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The type of the service to check for registration.</typeparam>
    /// <param name="services">The IServiceCollection to check.</param>
    /// <returns>True if the service is registered, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services is null.</exception>
    public static bool IsAdded<T>(this IServiceCollection services)
        => services.IsAdded(typeof(T));

    /// <summary>
    /// Determines whether a service of the specified type has been added to the dependency injection container.
    /// </summary>
    /// <param name="services">The IServiceCollection to check.</param>
    /// <param name="type">The type of the service to check for registration.</param>
    /// <returns>True if the service is registered, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services or type is null.</exception>
    public static bool IsAdded(this IServiceCollection services, Type type)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(type);

        return services.Any(d => d.ServiceType == type);
    }

    /// <summary>
    /// Determines whether the concrete implementation type of the specified service type has been added to the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The type of the service to check for implementation registration.</typeparam>
    /// <param name="services">The IServiceCollection to check.</param>
    /// <returns>True if the implementation type is registered, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services is null.</exception>
    public static bool IsImplementationAdded<T>(this IServiceCollection services)
        => services.IsImplementationAdded(typeof(T));

    /// <summary>
    /// Determines whether the concrete implementation type of the specified Type has been added to the dependency injection container.
    /// </summary>
    /// <param name="services">The IServiceCollection to check.</param>
    /// <param name="type">The implementation type to check for registration.</param>
    /// <returns>True if the implementation type is registered, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services or type is null.</exception>
    public static bool IsImplementationAdded(this IServiceCollection services, Type type)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(type);

        return services.Any(d => d.ImplementationType == type);
    }

    /// <summary>
    /// Resolves and creates an instance of the specified service type using dependency injection,
    /// allowing for injection of constructor parameters.
    /// </summary>
    /// <typeparam name="T">The type of the service to resolve.</typeparam>
    /// <param name="provider">The IServiceProvider to use for dependency resolution.</param>
    /// <param name="parameters">The constructor parameters to inject into the service instance.</param>
    /// <returns>The resolved instance of the service.</returns>
    /// <exception cref="ArgumentNullException">Thrown if provider is null.</exception>
    public static T ResolveWith<T>(this IServiceProvider provider, params object[] parameters) where T : class
    {
        ArgumentNullException.ThrowIfNull(provider);
        return ActivatorUtilities.CreateInstance<T>(provider, parameters);
    }
}
