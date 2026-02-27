using Craft.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Auditing;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers audit trail services for the API layer.
    /// Adds <see cref="IAuditTrailRepository"/> and auto-discovers <see cref="AuditTrailController"/>
    /// from this assembly via an application part.
    /// </summary>
    /// <remarks>
    /// <see cref="IAuditUserResolver"/> must be registered separately by the host application
    /// before calling this method, as it is an app-specific dependency.
    /// </remarks>
    public static IServiceCollection AddAuditTrailApi(this IServiceCollection services)
    {
        services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();
        services.AddControllers().AddApplicationPart(typeof(AuditTrailController).Assembly);

        return services;
    }

    /// <summary>
    /// Registers the audit trail HTTP service for the Blazor UI layer.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientFactory">Factory function to resolve the <see cref="HttpClient"/> from DI.</param>
    /// <param name="baseAddress">The base address of the API (e.g., "https+http://api").</param>
    public static IServiceCollection AddAuditTrailUI(this IServiceCollection services, Func<IServiceProvider, HttpClient> httpClientFactory, string baseAddress)
    {
        services.AddTransient<IAuditTrailHttpService>(sp =>
        {
            var apiUrl = new Uri(new Uri(baseAddress), "/api/audittrail");
            var httpClient = httpClientFactory(sp);
            var logger = sp.GetRequiredService<ILogger<AuditTrailHttpService>>();
            return new AuditTrailHttpService(apiUrl, httpClient, logger);
        });

        return services;
    }

    /// <summary>
    /// Registers audit trail services for the API layer using the built-in <see cref="CraftUserAuditResolver{TUser}"/>.
    /// Eliminates the need for a host-application-specific <see cref="IAuditUserResolver"/> implementation
    /// when the user entity inherits from <see cref="CraftUser"/>.
    /// </summary>
    /// <typeparam name="TUser">The application user entity type, which must implement <see cref="ICraftUser"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddAuditTrailApi<TUser>(this IServiceCollection services)
        where TUser : class, ICraftUser
    {
        services.AddScoped<IAuditUserResolver, CraftUserAuditResolver<TUser>>();
        return services.AddAuditTrailApi();
    }
}

