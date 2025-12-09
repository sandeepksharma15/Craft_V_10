namespace Craft.MultiTenant;

[Flags]
public enum TenantType : byte
{
    None = 0,
    Tenant = 1,
    Host = 2,
    Both = Tenant | Host
}
