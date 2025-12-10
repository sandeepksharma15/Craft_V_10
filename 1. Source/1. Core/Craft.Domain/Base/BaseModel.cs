namespace Craft.Domain;

/// <summary>
/// Abstract base class for data transfer models with the default KeyType identifier.
/// Provides common properties for models including identity, concurrency control, and soft deletion.
/// </summary>
[Serializable]
public abstract class BaseModel : BaseModel<KeyType>, IModel;

/// <summary>
/// Abstract base class for data transfer models with a strongly-typed identifier.
/// Provides common properties for models including identity, concurrency control, and soft deletion.
/// </summary>
/// <typeparam name="TKey">The type of the model identifier.</typeparam>
[Serializable]
public abstract class BaseModel<TKey> : IHasConcurrency, ISoftDelete, IModel<TKey>
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the concurrency stamp used for optimistic concurrency control.
    /// </summary>
    public virtual string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the model represents a soft-deleted entity.
    /// </summary>
    public virtual bool IsDeleted { get; set; }
}
