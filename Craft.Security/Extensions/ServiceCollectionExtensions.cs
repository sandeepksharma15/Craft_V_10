using Craft.Security.Tokens;
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
}
