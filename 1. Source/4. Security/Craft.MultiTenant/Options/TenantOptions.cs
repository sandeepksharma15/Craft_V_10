namespace Craft.MultiTenant;

public class TenantOptions
{
    public IList<string> IgnoredIdentifiers = [];

    public TenantEvents Events { get; set; } = new TenantEvents();
}
