namespace Craft.MultiTenant;

[Flags]
public enum TenantDbType
{
    None = 0,
    Shared = 1,
    PerTenant = 2,
    Hybrid = Shared | PerTenant
}
