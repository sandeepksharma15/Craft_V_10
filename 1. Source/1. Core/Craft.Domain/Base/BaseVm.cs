namespace Craft.Domain;

[Serializable]
public abstract class BaseVm : BaseVm<KeyType>, IModel;

[Serializable]
public abstract class BaseVm<TKey> : IHasConcurrency, ISoftDelete, IModel<TKey>
{
    public virtual TKey Id { get; set; } = default!;

    public virtual string? ConcurrencyStamp { get; set; }

    public virtual bool IsDeleted { get; set; }
}
