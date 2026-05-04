namespace Craft.Permissions;

/// <summary>
/// Deferred registration of <see cref="PermissionDefinition"/> entries into the
/// <see cref="IPermissionDefinitionRegistry"/>. Executed by <see cref="PermissionStartupValidatorService"/>
/// during application startup so duplicate codes are caught before any requests are served.
/// </summary>
internal sealed class DeferredPermissionRegistration(
    IPermissionDefinitionRegistry registry,
    PermissionDefinition[] definitions) : IStartupValidator
{
    public void Validate()
    {
        foreach (var definition in definitions)
            registry.Register(definition);
    }
}
