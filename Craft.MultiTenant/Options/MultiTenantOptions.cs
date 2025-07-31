namespace Craft.MultiTenant;

public class MultiTenantOptions
{
    public TenantDbType DbType { get; set; } = TenantDbType.PerTenant;
    public bool IsEnabled { get; set; }
}
