using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Craft.Domain;

/// <summary>
/// Abstract base class for domain entities with the default KeyType identifier.
/// Provides common entity functionality including identity, concurrency control, and soft deletion.
/// </summary>
[Serializable]
public abstract class BaseEntity : BaseEntity<KeyType>, IEntity, IModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class.
    /// </summary>
    protected BaseEntity() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected BaseEntity(KeyType id) : base(id) { }
}

/// <summary>
/// Abstract base class for domain entities with a strongly-typed identifier.
/// Implements entity identity, equality, concurrency control, and soft deletion.
/// </summary>
/// <typeparam name="TKey">The type of the entity identifier.</typeparam>
[Serializable]
public abstract class BaseEntity<TKey> : IEntity<TKey>, IHasConcurrency, ISoftDelete, IModel<TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity{TKey}"/> class.
    /// </summary>
    protected BaseEntity() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity{TKey}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected BaseEntity(TKey id) { Id = id; }

    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(Order = 0)]
    public virtual TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the concurrency stamp used for optimistic concurrency control.
    /// </summary>
    [ConcurrencyCheck]
    [MaxLength(IHasConcurrency.MaxLength)]
    public virtual string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets a value indicating whether the entity is soft-deleted.
    /// </summary>
    public virtual bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>True if the entities are not equal; otherwise, false.</returns>
    public static bool operator !=(BaseEntity<TKey> left, BaseEntity<TKey> right)
        => !Equals(left, right);

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>True if the entities are equal; otherwise, false.</returns>
    public static bool operator ==(BaseEntity<TKey> left, BaseEntity<TKey> right)
        => Equals(left, right);

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// For multi-tenant entities, equality considers both TenantId and Id.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>True if the specified object is equal to the current entity; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TKey>) return false;

        if (ReferenceEquals(this, obj)) return true;

        if (GetType() != obj.GetType()) return false;

        // Different tenants may have an entity with same Id.
        return (this is not IHasTenant self || obj is not IHasTenant other || self.TenantId == other.TenantId) && Equals(Id, ((BaseEntity<TKey>)obj).Id);
    }

    /// <summary>
    /// Returns the hash code for this entity based on its identifier.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode()
        => 2108858624 + EqualityComparer<TKey>.Default.GetHashCode(Id ?? default!);

    /// <summary>
    /// Determines whether the entity is new (has the default identifier value).
    /// </summary>
    /// <returns>True if the entity is new; otherwise, false.</returns>
    public virtual bool IsNew()
        => EqualityComparer<TKey>.Default.Equals(Id, default);

    /// <summary>
    /// Returns a string representation of the entity.
    /// </summary>
    /// <returns>A string containing the entity type name and identifier.</returns>
    public override string ToString()
        => $"[ENTITY: {GetType().Name}] Key = {Id}";
}

