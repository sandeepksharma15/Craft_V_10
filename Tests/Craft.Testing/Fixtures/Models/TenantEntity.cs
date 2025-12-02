using Craft.Domain;

namespace Craft.Testing.Fixtures;

public class TenantEntity : BaseEntity, IHasTenant
{
    public KeyType TenantId { get; set; }

    public string TenantName { get; set; } = string.Empty;
}


public static class TenantEntitySeed
{
    public const int TENANT_ID_1 = 1;
    public const int TENANT_ID_2 = 2;
    public const string TENANT_NAME_1 = "Tenant 1";
    public const string TENANT_NAME_2 = "Tenant 2";

    public static List<TenantEntity> Get()
    {
        return [
            new() { Id = 1, TenantId = TENANT_ID_1, TenantName = TENANT_NAME_1 },
            new() { Id = 2, TenantId = TENANT_ID_2, TenantName = TENANT_NAME_2 }
        ];
    }
}
