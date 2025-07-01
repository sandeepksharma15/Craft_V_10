namespace Craft.Domain;

public interface IModel<TKey> : IHasId<TKey>;

public interface IModel : IModel<KeyType>;
