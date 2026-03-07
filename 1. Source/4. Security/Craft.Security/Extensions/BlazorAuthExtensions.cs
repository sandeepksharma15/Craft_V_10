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
    /// Registers all Blazor Server auth services: <see cref="JwtCookieAuthenticationHandler"/> as the
    /// default authentication scheme, authorization core, cookie-based <see cref="AuthenticationStateProvider"/>,
    /// and the current-user pipeline backed by <see cref="UiUserProvider"/>.
    /// </summary>
    /// <typeparam name="TKey">The type used to represent the user's primary key.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="loginPath">The path to redirect unauthenticated users to. Defaults to <c>/login</c>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftJwtAuthBlazor<TKey>(this IServiceCollection services, string loginPath = "/login")
    {
        // Register the JwtCookie authentication handler as the default scheme so that
        // UseAuthentication / UseAuthorization and [Authorize] work at the middleware level.
        services.AddAuthentication(JwtCookieAuthenticationHandler.SchemeName)
            .AddScheme<JwtCookieAuthenticationOptions, JwtCookieAuthenticationHandler>(
                JwtCookieAuthenticationHandler.SchemeName,
                options => options.LoginPath = loginPath);

        services.AddAuthorizationCore();

        // Register auth state as a root-level cascading service — more reliable than the
        // <CascadingAuthenticationState> component wrapper and available before any render.
        services.AddCascadingAuthenticationState();

        // Register JwtCookieAuthStateProvider as both its concrete type (so callers can inject it
        // directly to call NotifyAuthChanged) and the abstract base that Blazor resolves
        services.AddScoped<JwtCookieAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtCookieAuthStateProvider>());

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
    /// <param name="loginPath">The path to redirect unauthenticated users to. Defaults to <c>/login</c>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftJwtAuthBlazor(this IServiceCollection services, string loginPath = "/login")
        => services.AddCraftJwtAuthBlazor<KeyType>(loginPath);
}
