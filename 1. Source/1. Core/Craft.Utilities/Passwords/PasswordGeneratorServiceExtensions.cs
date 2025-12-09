using Microsoft.Extensions.DependencyInjection;

namespace Craft.Utilities.Passwords;

public static class PasswordGeneratorServiceExtensions
{
    /// <summary>
    /// Registers the PasswordGeneratorService as a singleton for IPasswordGeneratorService.
    /// </summary>
    public static IServiceCollection AddPasswordGeneratorService(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordGeneratorService, PasswordGeneratorService>();

        return services;
    }
}
