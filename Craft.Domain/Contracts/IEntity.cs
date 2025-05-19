namespace Craft.Domain;

public interface IEntity<TKey> : IHasId<TKey>;

public interface IEntity : IEntity<KeyType>;
