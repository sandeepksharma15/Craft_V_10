namespace Craft.Core;

/// <summary>
/// Provides access to essential information about the current tenant in a multi-tenant application.
/// This is a lightweight abstraction that can be used across all layers without requiring
/// a direct dependency on Craft.MultiTenant.
/// </summary>
/// <typeparam name="TKey">The type used to represent the tenant's ID.</typeparam>
public interface ICurrentTenant<TKey>
{
    /// <summary>
    /// Gets the unique identifier of the current tenant.
    /// </summary>
    TKey Id { get; }

    /// <summary>
    /// Gets the unique string identifier/code for the current tenant.
    /// </summary>
    string? Identifier { get; }

    /// <summary>
    /// Gets the display name of the current tenant.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the tenant ID.
    /// </summary>
    TKey GetId();

    /// <summary>
    /// Indicates whether a tenant has been resolved for the current context.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Gets a value indicating whether the current tenant is active.
    /// </summary>
    bool IsActive { get; }
}

/// <summary>
/// Provides access to current tenant information using the default KeyType.
/// </summary>
public interface ICurrentTenant : ICurrentTenant<KeyType>;
