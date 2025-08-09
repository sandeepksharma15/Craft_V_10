using Craft.MultiTenant;

namespace Craft.Data;

public interface IMultiTenantDbSetup
{
    Task SetupAppDbAsync(ITenant tenant, CancellationToken cancellationToken);

    Task SetupTenantDbAsync(CancellationToken cancellationToken);
}
