using Craft.MultiTenant;

namespace Craft.Data;

public interface IDbSetup
{
    Task SetupAppDbAsync(ITenant tenant, CancellationToken cancellationToken);

    Task SetupTenantDbAsync(CancellationToken cancellationToken);
}
