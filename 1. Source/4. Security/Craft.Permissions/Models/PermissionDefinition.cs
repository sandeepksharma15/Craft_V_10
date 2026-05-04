namespace Craft.Permissions;

/// <summary>
/// Describes a permission for use in the management UI.
/// Instances are registered at startup via <see cref="IPermissionDefinitionRegistry.Register"/>
/// and are never persisted to the database — they exist only to give friendly names and
/// groupings to the integer codes stored in <see cref="RolePermission"/>.
/// </summary>
/// <param name="Code">The unique integer permission code. Must not be reused across the application.</param>
/// <param name="Name">A short human-readable name shown in the UI (e.g. "Delete Student").</param>
/// <param name="Group">An optional group / module name used to cluster permissions (e.g. "Students").</param>
public record PermissionDefinition(int Code, string Name, string? Group = null);
