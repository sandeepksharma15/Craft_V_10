using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.MultiTenant.Tests.ExtensionTests;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetTenantContext_ReturnsContextWhenAvailable()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>()
            .WithStaticStrategy("test")
            .WithInMemoryStore();
        
        var sp = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = sp };
        
        var accessor = sp.GetRequiredService<ITenantContextAccessor<Tenant>>();
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        accessor.TenantContext = new TenantContext<Tenant>(tenant, null, null);
        
        var context = httpContext.GetTenantContext<Tenant>();
        
        Assert.NotNull(context);
        Assert.Equal("test", context.Tenant?.Identifier);
    }

    [Fact]
    public void GetTenantContext_NonGeneric_ReturnsContextWhenAvailable()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>()
            .WithStaticStrategy("test")
            .WithInMemoryStore();
        
        var sp = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = sp };
        
        var accessor = sp.GetRequiredService<ITenantContextAccessor<Tenant>>();
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        accessor.TenantContext = new TenantContext<Tenant>(tenant, null, null);
        
        var context = httpContext.GetTenantContext();
        
        Assert.NotNull(context);
        Assert.Equal("test", context.Tenant?.Identifier);
    }

    [Fact]
    public void GetTenantContext_ThrowsWhenContextNotAvailable()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>()
            .WithStaticStrategy("test")
            .WithInMemoryStore();
        
        var sp = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = sp };
        
        var exception = Assert.Throws<InvalidOperationException>(
            () => httpContext.GetTenantContext<Tenant>());
        
        Assert.Contains("TenantContext", exception.Message);
    }

    [Fact]
    public void GetTenantContext_ThrowsWhenHttpContextIsNull()
    {
        HttpContext? httpContext = null;
        
        Assert.Throws<ArgumentNullException>(
            () => httpContext!.GetTenantContext<Tenant>());
    }

    [Fact]
    public void SetTenant_SetsTenantInContext()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>()
            .WithStaticStrategy("test")
            .WithInMemoryStore();
        
        var sp = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        
        var result = httpContext.SetTenant(tenant, resetServiceProviderScope: false);
        
        Assert.True(result);
        
        var accessor = sp.GetRequiredService<ITenantContextAccessor<Tenant>>();
        Assert.NotNull(accessor.TenantContext);
        Assert.Equal("test", accessor.TenantContext?.Tenant?.Identifier);
    }

    [Fact]
    public void SetTenant_NonGeneric_SetsTenantInContext()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>()
            .WithStaticStrategy("test")
            .WithInMemoryStore();
        
        var sp = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        
        var result = httpContext.SetTenant(tenant, resetServiceProviderScope: false);
        
        Assert.True(result);
        
        var accessor = sp.GetRequiredService<ITenantContextAccessor<Tenant>>();
        Assert.NotNull(accessor.TenantContext);
        Assert.Equal("test", accessor.TenantContext?.Tenant?.Identifier);
    }

    [Fact]
    public void SetTenant_ResetsServiceProviderScope_WhenRequested()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>()
            .WithStaticStrategy("test")
            .WithInMemoryStore();
        
        var sp = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var originalServiceProvider = httpContext.RequestServices;
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        
        httpContext.SetTenant(tenant, resetServiceProviderScope: true);
        
        Assert.NotSame(originalServiceProvider, httpContext.RequestServices);
    }

    [Fact]
    public void SetTenant_DoesNotResetServiceProviderScope_WhenNotRequested()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>()
            .WithStaticStrategy("test")
            .WithInMemoryStore();
        
        var sp = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var originalServiceProvider = httpContext.RequestServices;
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        
        httpContext.SetTenant(tenant, resetServiceProviderScope: false);
        
        Assert.Same(originalServiceProvider, httpContext.RequestServices);
    }

    [Fact]
    public void SetTenant_ThrowsWhenHttpContextIsNull()
    {
        HttpContext? httpContext = null;
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        
        Assert.Throws<ArgumentNullException>(
            () => httpContext!.SetTenant(tenant, resetServiceProviderScope: false));
    }

    [Fact]
    public void SetTenant_ClearsStrategyAndStore()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>()
            .WithStaticStrategy("test")
            .WithInMemoryStore();
        
        var sp = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        
        httpContext.SetTenant(tenant, resetServiceProviderScope: false);
        
        var accessor = sp.GetRequiredService<ITenantContextAccessor<Tenant>>();
        Assert.Null(accessor.TenantContext?.Strategy);
        Assert.Null(accessor.TenantContext?.Store);
    }
}
