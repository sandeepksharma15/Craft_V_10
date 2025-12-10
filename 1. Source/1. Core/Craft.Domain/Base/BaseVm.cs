namespace Craft.Domain;

/// <summary>
/// Abstract base class for View Models (VMs) with the default KeyType identifier.
/// Provides common properties for view models including identity, concurrency control, and soft deletion status.
/// </summary>
[Serializable]
public abstract class BaseVm : BaseVm<KeyType>, IModel;

/// <summary>
/// Abstract base class for View Models (VMs) with a strongly-typed identifier.
/// Provides common properties for view models including identity, concurrency control, and soft deletion status.
/// </summary>
/// <typeparam name="TKey">The type of the view model identifier.</typeparam>
[Serializable]
public abstract class BaseVm<TKey> : IHasConcurrency, ISoftDelete, IModel<TKey>
{
    /// <summary>
    /// Gets or sets the view model identifier.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the concurrency stamp used for optimistic concurrency control.
    /// </summary>
    public virtual string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the view model represents a soft-deleted entity.
    /// </summary>
    public virtual bool IsDeleted { get; set; }
}
