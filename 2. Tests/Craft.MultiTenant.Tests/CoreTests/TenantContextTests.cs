namespace Craft.MultiTenant.Tests.CoreTests;

public class TenantContextTests
{
    [Fact]
    public void ReturnFalseInHasResolvedTenantIfTenantInfoIsNull()
    {
        ITenantContext<Tenant> context = new TenantContext<Tenant>();
        Assert.False(context.HasResolvedTenant);
    }

    [Fact]
    public void ReturnFalseInHasResolvedTenantIfTenantInfoIsNull_NonGeneric()
    {
        ITenantContext context = new TenantContext<Tenant>();
        Assert.False(context.HasResolvedTenant);
    }

    [Fact]
    public void ReturnTrueInHasResolvedTenantIfTenantInfoIsNotNull()
    {
        ITenantContext<Tenant> context = new TenantContext<Tenant>
        {
            Tenant = new Tenant()
        };
        Assert.True(context.HasResolvedTenant);
    }

    [Fact]
    public void ReturnTrueInHasResolvedTenantIfTenantInfoIsNotNull_NonGeneric()
    {
        var context = new TenantContext<Tenant>
        {
            Tenant = new Tenant()
        };

        ITenantContext iContext = context;
        Assert.True(iContext.HasResolvedTenant);
    }
}
