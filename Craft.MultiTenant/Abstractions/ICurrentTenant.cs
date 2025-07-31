namespace Craft.MultiTenant;

public interface ICurrentTenant<TKey> : ITenant<TKey>;

public interface ICurrentTenant : ICurrentTenant<KeyType>;
