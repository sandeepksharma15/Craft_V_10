using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Craft.Permissions;

/// <summary>
/// Extension methods for registering <c>Craft.Permissions</c> services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the server-side permission services (repository, controller).
    /// Call this in the API / server project alongside <c>AddSecurityApi</c>.
    /// </summary>
    /// <typeparam name="TUser">The application user type (must derive from <see cref="IdentityUser{TKey}"/>).</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>A <see cref="PermissionBuilder"/> for further configuration (e.g. registering permission definitions).</returns>
    public static PermissionBuilder AddCraftPermissions<TUser>(this IServiceCollection services)
        where TUser : IdentityUser<KeyType>
    {
        services.TryAddSingleton<IPermissionDefinitionRegistry, PermissionDefinitionRegistry>();
        // RolePermissionRepository resolves DbContext directly — the consuming app must register
        // its DbContext and call Features.AddPermissions() to include the RolePermission entity.
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddHostedService<PermissionStartupValidatorService>();

        // Dynamically register PermissionsController<TUser> without requiring a concrete class in the host app
        services.AddControllers()
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new PermissionsControllerFeatureProvider<TUser>()));

        return new PermissionBuilder(services);
    }

    /// <summary>
    /// Registers the Blazor client-side permission services (session cache, permission checker,
    /// auth-state listener, HTTP services).
    /// Call this in the Blazor / UI project.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="permissionsApiBaseUrl">
    /// The base URL for the permissions API (e.g. <c>new Uri("https://myapi/api/permissions")</c>).
    /// </param>
    /// <returns>A <see cref="PermissionBuilder"/> for further configuration (e.g. registering permission definitions).</returns>
    public static PermissionBuilder AddCraftPermissionsUi(this IServiceCollection services, Uri permissionsApiBaseUrl)
    {
        ArgumentNullException.ThrowIfNull(permissionsApiBaseUrl);

        services.TryAddSingleton<IPermissionDefinitionRegistry, PermissionDefinitionRegistry>();

        // PermissionSessionCache is both the cache and the checker — single scoped instance
        services.AddScoped<PermissionSessionCache>();
        services.AddScoped<IPermissionSessionCache>(sp => sp.GetRequiredService<PermissionSessionCache>());
        services.AddScoped<IPermissionChecker>(sp => sp.GetRequiredService<PermissionSessionCache>());

        services.AddScoped<IUserPermissionsHttpService>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserPermissionsHttpService>>();
            return new UserPermissionsHttpService(permissionsApiBaseUrl, httpClient, logger);
        });

        services.AddScoped<IRolePermissionHttpService>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RolePermissionHttpService>>();
            return new RolePermissionHttpService(permissionsApiBaseUrl, httpClient, logger);
        });

        services.AddScoped<PermissionAuthStateListener>();
        services.AddHostedService<PermissionStartupValidatorService>();

        return new PermissionBuilder(services);
    }
}
