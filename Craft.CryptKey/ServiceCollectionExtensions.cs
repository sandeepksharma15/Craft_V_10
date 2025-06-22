using Craft.CryptKey;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Domain.HashIdentityKey;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHashKeys(this IServiceCollection services, 
        Action<HashKeyOptions> configureOptions = null!)
    {
        services.Configure(configureOptions ?? (_ => { }));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<HashKeyOptions>>().Value);
        services.AddSingleton<IHashKeys>(sp =>
        {
            var options = sp.GetRequiredService<HashKeyOptions>();

            return new HashKeys(options);
        });

        return services;
    }
}
