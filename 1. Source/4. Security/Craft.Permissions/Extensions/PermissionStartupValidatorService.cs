using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Craft.Permissions;

/// <summary>
/// Hosted service that runs all <see cref="IStartupValidator"/> registrations at application
/// startup. If any permission code is duplicated <see cref="InvalidOperationException"/> is
/// thrown, preventing the application from serving requests with a misconfigured permission set.
/// Register via <see cref="ServiceCollectionExtensions.AddCraftPermissions{TUser}"/> or
/// <see cref="ServiceCollectionExtensions.AddCraftPermissionsUi"/>.
/// </summary>
public sealed class PermissionStartupValidatorService(
    IEnumerable<IStartupValidator> validators,
    ILogger<PermissionStartupValidatorService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating permission definitions...");

        foreach (var validator in validators)
            validator.Validate();

        logger.LogInformation("Permission definitions validated successfully.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
