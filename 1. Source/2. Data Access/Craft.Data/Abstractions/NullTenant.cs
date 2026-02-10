using Craft.MultiTenant;

namespace Craft.Data;

/// <summary>
/// Null implementation of <see cref="ITenant"/> for single-tenant or non-multi-tenant scenarios.
/// Provides a singleton instance to avoid repeated allocations.
/// </summary>
public sealed class NullTenant : ITenant
{
    /// <summary>
    /// Singleton instance of NullTenant. Use this instead of creating new instances.
    /// </summary>
    public static readonly NullTenant Instance = new();

    // Private constructor to enforce singleton pattern
    private NullTenant() { }

    /// <inheritdoc/>
    public KeyType Id { get; set; } = default!;

    /// <inheritdoc/>
    public string Identifier { get; set; } = "default";

    /// <inheritdoc/>
    public string Name { get; set; } = "Default Tenant";

    /// <inheritdoc/>
    public string AdminEmail { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string LogoUri { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string ConnectionString { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string DbProvider { get; set; } = string.Empty;

    /// <inheritdoc/>
    public TenantType Type { get; set; } = TenantType.Host;

    /// <inheritdoc/>
    public TenantDbType DbType { get; set; } = TenantDbType.Shared;

    /// <inheritdoc/>
    public DateTime ValidUpTo { get; set; } = DateTime.MaxValue;

    /// <inheritdoc/>
    public bool IsDeleted { get; set; } = false;

    /// <inheritdoc/>
    public string? ConcurrencyStamp { get; set; } = string.Empty;

    /// <inheritdoc/>
    public bool IsActive { get; set; } = true;
}
