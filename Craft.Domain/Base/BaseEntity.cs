using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Craft.Domain;

[Serializable]
public abstract class BaseEntity : BaseEntity<KeyType>, IEntity, IModel
{
    protected BaseEntity() { }

    protected BaseEntity(KeyType id) : base(id) { }
}

[Serializable]
public abstract class BaseEntity<TKey> : IEntity<TKey>, IHasConcurrency, ISoftDelete, IModel<TKey>
{
    protected BaseEntity() { }

    protected BaseEntity(TKey id) { Id = id; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(Order = 0)]
    public virtual TKey Id { get; set; } = default!;

    [ConcurrencyCheck]
    [MaxLength(IHasConcurrency.MaxLength)]
    public virtual string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

    public virtual bool IsDeleted { get; set; } = false;

    public static bool operator !=(BaseEntity<TKey> left, BaseEntity<TKey> right)
        => !Equals(left, right);

    public static bool operator ==(BaseEntity<TKey> left, BaseEntity<TKey> right)
        => Equals(left, right);

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TKey>) return false;

        if (ReferenceEquals(this, obj)) return true;

        if (GetType() != obj.GetType()) return false;

        // Different tenants may have an entity with same Id.
        if (this is IHasTenant self && obj is IHasTenant other && self.TenantId != other.TenantId)
            return false;

        return Equals(Id, ((BaseEntity<TKey>)obj).Id);
    }

    public override int GetHashCode()
        => 2108858624 + EqualityComparer<TKey>.Default.GetHashCode(Id ?? default!);

    public virtual bool IsNew()
        => EqualityComparer<TKey>.Default.Equals(Id, default);

    public override string ToString()
        => $"[ENTITY: {GetType().Name}] Key = {Id}";
}
