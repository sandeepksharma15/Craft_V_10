using Craft.Core;
using Craft.Security;
using Craft.Security.Tokens;

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
}
