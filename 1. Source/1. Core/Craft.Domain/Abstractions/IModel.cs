namespace Craft.Domain;

/// <summary>
/// Defines a base contract for data transfer models with a strongly-typed identifier.
/// </summary>
/// <typeparam name="TKey">The type of the model identifier.</typeparam>
public interface IModel<TKey> : IHasId<TKey>;

/// <summary>
/// Defines a base contract for data transfer models with the default KeyType identifier.
/// </summary>
public interface IModel : IModel<KeyType>;
