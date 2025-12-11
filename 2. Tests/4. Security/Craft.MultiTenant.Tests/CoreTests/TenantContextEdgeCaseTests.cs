namespace Craft.MultiTenant.Tests.CoreTests;

public class TenantContextEdgeCaseTests
{
    [Fact]
    public void TenantContext_WithTenant_HasResolvedTenantReturnsTrue()
    {
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        var context = new TenantContext<Tenant>(tenant, null, null);
        ITenantContext<Tenant> iContext = context;
        
        Assert.True(iContext.HasResolvedTenant);
    }

    [Fact]
    public void TenantContext_WithoutTenant_HasResolvedTenantReturnsFalse()
    {
        var context = new TenantContext<Tenant>(null, null, null);
        ITenantContext<Tenant> iContext = context;
        
        Assert.False(iContext.HasResolvedTenant);
    }

    [Fact]
    public void NonGenericInterface_ReturnsGenericTenant()
    {
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        var context = new TenantContext<Tenant>(tenant, null, null);
        ITenantContext nonGeneric = context;
        
        Assert.NotNull(nonGeneric.Tenant);
        Assert.Same(tenant, nonGeneric.Tenant);
    }

    [Fact]
    public void DefaultConstructor_InitializesAllPropertiesToNull()
    {
        var context = new TenantContext<Tenant>();
        ITenantContext<Tenant> iContext = context;
        
        Assert.Null(context.Tenant);
        Assert.Null(context.Store);
        Assert.Null(context.Strategy);
        Assert.False(iContext.HasResolvedTenant);
    }

    [Fact]
    public void Properties_CanBeSetIndependently()
    {
        var context = new TenantContext<Tenant>();
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        
        context.Tenant = tenant;
        Assert.NotNull(context.Tenant);
        Assert.Null(context.Store);
        Assert.Null(context.Strategy);
    }
}
