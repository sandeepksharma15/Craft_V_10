namespace Craft.MultiTenant.Tests.ServiceTests;

public class TenantContextAccessorTests
{
    [Fact]
    public void TenantContext_SetAndGet_WorksCorrectly()
    {
        var accessor = new TenantContextAccessor<Tenant>();
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test Tenant" };
        var context = new TenantContext<Tenant>(tenant, null, null);
        
        accessor.TenantContext = context;
        
        Assert.NotNull(accessor.TenantContext);
        Assert.Equal(tenant.Id, accessor.TenantContext.Tenant?.Id);
        Assert.Equal("test", accessor.TenantContext.Tenant?.Identifier);
    }

    [Fact]
    public void TenantContext_InitiallyNull()
    {
        var accessor = new TenantContextAccessor<Tenant>();
        
        Assert.Null(accessor.TenantContext);
    }

    [Fact]
    public void TenantContext_CanBeSetToNull()
    {
        var accessor = new TenantContextAccessor<Tenant>();
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test Tenant" };
        var context = new TenantContext<Tenant>(tenant, null, null);
        
        accessor.TenantContext = context;
        accessor.TenantContext = null;
        
        Assert.Null(accessor.TenantContext);
    }

    [Fact]
    public void TenantContext_Overwrite_UpdatesValue()
    {
        var accessor = new TenantContextAccessor<Tenant>();
        var tenant1 = new Tenant { Id = 1, Identifier = "tenant1", Name = "Tenant 1" };
        var tenant2 = new Tenant { Id = 2, Identifier = "tenant2", Name = "Tenant 2" };
        
        accessor.TenantContext = new TenantContext<Tenant>(tenant1, null, null);
        accessor.TenantContext = new TenantContext<Tenant>(tenant2, null, null);
        
        Assert.Equal(tenant2.Id, accessor.TenantContext?.Tenant?.Id);
    }

    [Fact]
    public void NonGenericInterface_WorksCorrectly()
    {
        ITenantContextAccessor accessor = new TenantContextAccessor<Tenant>();
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test Tenant" };
        var context = new TenantContext<Tenant>(tenant, null, null);
        
        accessor.TenantContext = context;
        
        Assert.NotNull(accessor.TenantContext);
        Assert.Equal(tenant.Id, accessor.TenantContext?.Tenant?.Id);
    }

    [Fact]
    public void NonGenericInterface_Get_ReturnsGenericContext()
    {
        var genericAccessor = new TenantContextAccessor<Tenant>();
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test Tenant" };
        var context = new TenantContext<Tenant>(tenant, null, null);
        
        genericAccessor.TenantContext = context;
        
        ITenantContextAccessor nonGenericAccessor = genericAccessor;
        
        Assert.NotNull(nonGenericAccessor.TenantContext);
        Assert.Same(context, nonGenericAccessor.TenantContext);
    }

    [Fact]
    public async Task AsyncLocal_IsolatesBetweenTasks()
    {
        var accessor = new TenantContextAccessor<Tenant>();
        var tenant1 = new Tenant { Id = 1, Identifier = "tenant1", Name = "Tenant 1" };
        var tenant2 = new Tenant { Id = 2, Identifier = "tenant2", Name = "Tenant 2" };
        
        var task1 = Task.Run(() =>
        {
            accessor.TenantContext = new TenantContext<Tenant>(tenant1, null, null);
            Thread.Sleep(50);
            return accessor.TenantContext?.Tenant?.Id;
        });
        
        var task2 = Task.Run(() =>
        {
            Thread.Sleep(25);
            accessor.TenantContext = new TenantContext<Tenant>(tenant2, null, null);
            return accessor.TenantContext?.Tenant?.Id;
        });
        
        var results = await Task.WhenAll(task1, task2);
        
        Assert.Equal(1, results[0]);
        Assert.Equal(2, results[1]);
    }
}
