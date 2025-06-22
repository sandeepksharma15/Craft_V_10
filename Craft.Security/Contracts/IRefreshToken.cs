using Craft.Domain;

namespace Craft.Security;

public interface IRefreshToken<TKey> : IHasId<TKey>, IHasUser<TKey>, ISoftDelete, IEntity<TKey>, IModel<TKey>
{
    public DateTime ExpiryTime { get; set; }
    public string? Token { get; set; }
}

public interface IRefreshToken : IRefreshToken<KeyType>, IHasId, IHasUser, IEntity, IModel;
