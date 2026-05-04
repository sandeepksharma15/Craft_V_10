using Microsoft.Extensions.DependencyInjection;

namespace Craft.Permissions;

/// <summary>
/// Fluent builder returned by <see cref="ServiceCollectionExtensions.AddCraftPermissions{TUser}"/>
/// and <see cref="ServiceCollectionExtensions.AddCraftPermissionsUi"/> to allow chaining
/// permission definition registration at startup.
/// </summary>
public sealed class PermissionBuilder(IServiceCollection services)
{
    /// <summary>Gets the underlying service collection.</summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Registers one or more <see cref="PermissionDefinition"/> entries.
    /// Throws <see cref="InvalidOperationException"/> at startup if any code is duplicated.
    /// </summary>
    public PermissionBuilder RegisterPermissions(params PermissionDefinition[] definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        Services.AddSingleton<IStartupValidator>(sp =>
        {
            var registry = sp.GetRequiredService<IPermissionDefinitionRegistry>();
            return new DeferredPermissionRegistration(registry, definitions);
        });

        return this;
    }
}
