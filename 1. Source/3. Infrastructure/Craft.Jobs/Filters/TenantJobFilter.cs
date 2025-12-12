using Craft.MultiTenant;
using Hangfire.Client;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Jobs;

/// <summary>
/// Hangfire filter for multi-tenant job execution.
/// Ensures tenant context is preserved across job execution.
/// </summary>
public class TenantJobFilter : IClientFilter, IServerFilter
{
    private readonly IServiceProvider _serviceProvider;
    private const string TenantIdKey = "TenantId";
    private const string TenantNameKey = "TenantName";

    public TenantJobFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // Called when creating a job (captures tenant context)
    public void OnCreating(CreatingContext filterContext)
    {
        using var scope = _serviceProvider.CreateScope();
        var tenant = scope.ServiceProvider.GetService<ITenant>();

        if (tenant?.Id != null)
        {
            filterContext.SetJobParameter(TenantIdKey, tenant.Id.ToString());
            filterContext.SetJobParameter(TenantNameKey, tenant.Name ?? string.Empty);
        }
    }

    public void OnCreated(CreatedContext filterContext)
    {
        // Not needed
    }

    // Called before executing a job (restores tenant context)
    public void OnPerforming(PerformingContext filterContext)
    {
        var tenantId = filterContext.GetJobParameter<string>(TenantIdKey);
        var tenantName = filterContext.GetJobParameter<string>(TenantNameKey);

        if (!string.IsNullOrEmpty(tenantId))
        {
            // Store tenant information in execution context
            filterContext.Items["TenantId"] = tenantId;
            filterContext.Items["TenantName"] = tenantName;

            var logger = _serviceProvider.GetService<ILogger<TenantJobFilter>>();
            logger?.LogInformation(
                "Executing job {JobId} for tenant {TenantId} ({TenantName})",
                filterContext.BackgroundJob.Id,
                tenantId,
                tenantName);
        }
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        // Cleanup if needed
        filterContext.Items.Remove("TenantId");
        filterContext.Items.Remove("TenantName");
    }
}
