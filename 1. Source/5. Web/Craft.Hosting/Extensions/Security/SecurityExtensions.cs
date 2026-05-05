using Craft.Core;
using Craft.Permissions;
using Craft.Security;
using Craft.Security.Tokens;
using Microsoft.AspNetCore.Identity;

namespace Craft.Hosting.Extensions;

/// <summary>
/// Extension methods for configuring security services.
/// </summary>
public static class SecurityExtensions
{
    /// <summary>
    /// Adds current user provider for API scenarios (uses claims from authentication).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCurrentApiUser(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserProvider, ApiUserProvider>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    /// <summary>
    /// Adds current user provider for UI scenarios (uses Blazor authentication state).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCurrentUiUser(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserProvider, UiUserProvider>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    /// <summary>
    /// Adds Craft security services including token management.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftSecurity(this IServiceCollection services)
    {
        services.AddScoped<ITokenManager, TokenManager>();

        return services;
    }

    /// <summary>
    /// Adds all server-side permission services: repository, API controller wiring, and
    /// startup duplicate-code validation.
    /// Call this in the API / server project alongside <c>AddCraftSecurity</c>.
    /// The app's DbContext must call <c>Features.AddPermissions()</c> to include the
    /// <c>ID_RolePermissions</c> table.
    /// </summary>
    /// <typeparam name="TUser">The application user type (must derive from <see cref="IdentityUser{TKey}"/>).</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional callback to register <see cref="PermissionDefinition"/> entries via <see cref="PermissionBuilder"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPermissionsApi<TUser>(this IServiceCollection services,
        Action<PermissionBuilder>? configure = null)
        where TUser : IdentityUser<KeyType>
    {
        var builder = services.AddCraftPermissions<TUser>();
        configure?.Invoke(builder);

        return services;
    }

    /// <summary>
    /// Adds all Blazor client-side permission services: per-user session cache, permission
    /// checker, auth-state listener, and HTTP services for the permissions API.
    /// Call this in the Blazor / UI project.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="permissionsApiBaseUrl">Base URL of the permissions API (e.g. <c>new Uri("https://myapi")</c>).</param>
    /// <param name="configure">Optional callback to register <see cref="PermissionDefinition"/> entries via <see cref="PermissionBuilder"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPermissionsUi(this IServiceCollection services,
        Uri permissionsApiBaseUrl,
        Action<PermissionBuilder>? configure = null)
    {
        var builder = services.AddCraftPermissionsUi(permissionsApiBaseUrl);
        configure?.Invoke(builder);

        return services;
    }
}
