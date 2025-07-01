using Craft.Domain;

namespace Craft.Security;

public interface ILoginHistory<TKey> : IHasId<TKey>, IHasUser<TKey>, ISoftDelete, IEntity<TKey>, IModel<TKey>
{
    public string? LastIpAddress { get; set; }

    public DateTime? LastLoginOn { get; set; }

    public string? Provider { get; set; }
}

public interface ILoginHistory : ILoginHistory<KeyType>, IHasId, IHasUser, IEntity, IModel;
