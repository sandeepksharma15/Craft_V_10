using Craft.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Security;

/// <summary>
/// Extension methods for configuring Blazor Server JWT cookie authentication services.
/// </summary>
public static class BlazorAuthExtensions
{
    /// <summary>
    /// Registers all Blazor Server auth services: cookie-based <see cref="AuthenticationStateProvider"/>,
    /// authorization core, and the current-user pipeline backed by <see cref="UiUserProvider"/>.
    /// </summary>
    /// <typeparam name="TKey">The type used to represent the user's primary key.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftJwtAuthBlazor<TKey>(this IServiceCollection services)
    {
        // Cookie reading is handled in the state provider; no bearer challenge scheme required
        services.AddAuthentication();
        services.AddAuthorizationCore();

        // Register JwtCookieAuthStateProvider as both its concrete type (so callers can inject it
        // directly to call NotifyAuthChanged) and the abstract base that Blazor resolves
        services.AddScoped<JwtCookieAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(
            sp => sp.GetRequiredService<JwtCookieAuthStateProvider>());

        // UiUserProvider reads the principal from the AuthenticationStateProvider
        services.AddScoped<ICurrentUserProvider, UiUserProvider>();

        // CurrentUser<TKey> satisfies the generic contract; non-generic CurrentUser satisfies ICurrentUser
        services.AddScoped<ICurrentUser<TKey>, CurrentUser<TKey>>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    /// <summary>
    /// Registers Blazor Server auth services using <see cref="KeyType"/> as the user key type.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftJwtAuthBlazor(this IServiceCollection services)
        => services.AddCraftJwtAuthBlazor<KeyType>();
}
