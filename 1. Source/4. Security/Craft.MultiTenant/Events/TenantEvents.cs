namespace Craft.MultiTenant;

public class TenantEvents
{
    public Func<NotResolvedContext, Task> OnTenantNotResolved { get; set; } = _ => Task.CompletedTask;
    public Func<ResolvedContext, Task> OnTenantResolved { get; set; } = _ => Task.CompletedTask;
}
