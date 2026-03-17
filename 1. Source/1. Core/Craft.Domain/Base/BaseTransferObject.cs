namespace Craft.Domain;

/// <summary>
/// Internal abstract base record that consolidates the shared properties of all data transfer types.
/// </summary>
/// <remarks>
/// Consumers should derive from <see cref="BaseDto{TKey}"/>, <see cref="BaseVm{TKey}"/>,
/// or <see cref="BaseModel{TKey}"/> depending on the transfer direction rather than from this
/// type directly.
/// </remarks>
/// <typeparam name="TKey">The type of the identifier.</typeparam>
[Serializable]
public abstract record BaseTransferObject<TKey> : IDataTransferObject<TKey>
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the concurrency stamp used for optimistic concurrency control.
    /// </summary>
    public virtual string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the record represents a soft-deleted entity.
    /// </summary>
    public virtual bool IsDeleted { get; set; }
}
