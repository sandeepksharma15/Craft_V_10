using Craft.Controllers.ErrorHandling;
using Craft.Controllers.ErrorHandling.Strategies;

namespace Craft.Hosting.Extensions;

/// <summary>
/// Extension methods for configuring controller and web services.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Adds database error handling services to the service collection.
    /// Registers all error handling strategies and the DatabaseErrorHandler service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddDatabaseErrorHandling(this IServiceCollection services)
    {
        // Register all strategies as singletons (they are stateless)
        services.AddSingleton<IDatabaseErrorStrategy, PostgreSqlErrorStrategy>();
        services.AddSingleton<IDatabaseErrorStrategy, SqlServerErrorStrategy>();
        services.AddSingleton<IDatabaseErrorStrategy, GenericErrorStrategy>();

        // Register the error handler as a singleton
        services.AddSingleton<IDatabaseErrorHandler, DatabaseErrorHandler>();

        return services;
    }
}
