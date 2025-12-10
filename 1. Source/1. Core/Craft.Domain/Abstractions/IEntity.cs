namespace Craft.Domain;

/// <summary>
/// Defines a base contract for domain entities with a strongly-typed identifier.
/// </summary>
/// <typeparam name="TKey">The type of the entity identifier.</typeparam>
public interface IEntity<TKey> : IHasId<TKey>;

/// <summary>
/// Defines a base contract for domain entities with the default KeyType identifier.
/// </summary>
public interface IEntity : IEntity<KeyType>;
