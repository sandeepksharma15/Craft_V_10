using Craft.Core;
using Craft.Security.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Security;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrentApiUser(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserProvider, ApiUserProvider>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    public static IServiceCollection AddCurrentUiUser(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserProvider, UiUserProvider>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    public static IServiceCollection AddCraftSecurity(this IServiceCollection services)
    {
        services.AddScoped<ITokenManager, TokenManager>();

        return services;
    }

    /// <summary>
    /// Registers all API-side JWT authentication services in a single call: JWT bearer configuration,
    /// token management, and the current-user services backed by <see cref="ApiUserProvider"/>.
    /// </summary>
    /// <typeparam name="TUser">The application user entity type.</typeparam>
    /// <typeparam name="TKey">The type used to represent the user's primary key.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The application configuration used to bind <c>SecuritySettings:JwtSettings</c>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftJwtAuthApi<TUser, TKey>(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureJwt(config);
        services.AddTokenManagement();

        services.AddScoped<ICurrentUserProvider, ApiUserProvider>();
        services.AddScoped<ICurrentUser<TKey>, CurrentUser<TKey>>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    /// <summary>
    /// Registers all API-side JWT authentication services in a single call,
    /// using <see cref="KeyType"/> as the user key type.
    /// </summary>
    /// <typeparam name="TUser">The application user entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The application configuration used to bind <c>SecuritySettings:JwtSettings</c>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftJwtAuthApi<TUser>(this IServiceCollection services, IConfiguration config)
        => services.AddCraftJwtAuthApi<TUser, KeyType>(config);
}
