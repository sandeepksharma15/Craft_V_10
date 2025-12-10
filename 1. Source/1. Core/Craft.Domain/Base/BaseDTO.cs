// For pure DTOs without entity concerns:
namespace Craft.Domain;

[Serializable]
public abstract class BaseDto<TKey> : IModel<TKey>
{
    public virtual TKey Id { get; set; } = default!;
}

[Serializable]
public abstract class BaseDto : BaseDto<KeyType>, IModel;
