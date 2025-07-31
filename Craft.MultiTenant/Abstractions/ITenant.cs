using Craft.Domain;

namespace Craft.MultiTenant;

public interface ITenant<TKey> : ISoftDelete, IHasConcurrency, IEntity<TKey>, IHasActive, IModel<TKey>
{
    public string AdminEmail { get; set; }
    string ConnectionString { get; set; }
    string DbProvider { get; set; }
    public string Identifier { get; set; }
    public string LogoUri { get; set; }
    string Name { get; set; }
    public TenantType Type { get; set; }
    public DateTime ValidUpTo { get; set; }
}

public interface ITenant : ITenant<KeyType>, IEntity, IModel;
