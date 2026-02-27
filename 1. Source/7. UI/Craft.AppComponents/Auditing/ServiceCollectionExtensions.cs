using Microsoft.Extensions.DependencyInjection;

namespace Craft.AppComponents.Auditing;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditTrailUI(this IServiceCollection services)
    {

        return services;
    }
}
