namespace Craft.Domain;

public interface IHasTenant<TKey>
{
    public const string ColumnName = "TenantId";

    TKey TenantId { get; set; }

    TKey GetTenantId() => TenantId;

    bool IsTenantIdSet() => TenantId != null && !EqualityComparer<TKey>.Default.Equals(TenantId, default);

    void SetTenantId(TKey tenantId) => TenantId = tenantId;
}

public interface IHasTenant : IHasTenant<KeyType>;
