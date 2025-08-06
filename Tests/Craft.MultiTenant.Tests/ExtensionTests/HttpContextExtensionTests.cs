using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Craft.MultiTenant.Tests.ExtensionTests;

public class HttpContextExtensionTests
{
    [Fact]
    public void GetTenantContextIfExists()
    {
        var ti = new Tenant { Id = 1 };
        var tc = new TenantContext<Tenant>
        {
            Tenant = ti
        };

        var services = new ServiceCollection();

        services.AddScoped<ITenantContextAccessor<Tenant>>(_ => new TenantContextAccessor<Tenant> { TenantContext = tc });
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var mtc = httpContextMock.Object.GetTenantContext<Tenant>();

        Assert.Same(tc, mtc);
    }

    [Fact]
    public void NotResetScopeIfNotApplicable()
    {
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.SetupProperty(c => c.RequestServices);

        var services = new ServiceCollection();

        services.AddScoped<object>(_ => DateTime.Now);
        services.AddMultiTenant<Tenant>();
        var sp = services.BuildServiceProvider();
        httpContextMock.Object.RequestServices = sp;

        var ti2 = new Tenant { Id = 1 };
        httpContextMock.Object.SetTenant(ti2, false);

        Assert.Same(sp, httpContextMock.Object.RequestServices);
        Assert.StrictEqual((DateTime?)sp.GetService<object>(),
            (DateTime?)httpContextMock.Object.RequestServices.GetService<object>());
    }

    [Fact]
    public void ResetScopeIfApplicable()
    {
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.SetupProperty(c => c.RequestServices);

        var services = new ServiceCollection();

        services.AddScoped<object>(_ => DateTime.Now);
        services.AddMultiTenant<Tenant>();
        var sp = services.BuildServiceProvider();
        httpContextMock.Object.RequestServices = sp;

        var ti2 = new Tenant { Id = 1 };
        httpContextMock.Object.SetTenant(ti2, true);

        Assert.NotSame(sp, httpContextMock.Object.RequestServices);
        Assert.NotStrictEqual((DateTime?)sp.GetService<object>(),
            (DateTime?)httpContextMock.Object.RequestServices.GetService<object>());
    }

    [Fact]
    public void ReturnNullIfNoTenantContext()
    {
        var services = new ServiceCollection();

        services.AddScoped<ITenantContextAccessor<Tenant>>(_ => new TenantContextAccessor<Tenant>());
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var ex = Assert.Throws<InvalidOperationException>(() => httpContextMock.Object.GetTenantContext<Tenant>());
        Assert.Equal("TenantContext<Tenant> is not available in the current HttpContext.", ex.Message);
    }

    [Fact]
    public void SetStoreAndStrategyNull()
    {
        var services = new ServiceCollection();

        services.AddMultiTenant<Tenant>();
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var context = httpContextMock.Object;

        var ti2 = new Tenant { Id = 1 };
        context.SetTenant(ti2, false);
        var mtc = context.GetTenantContext<Tenant>();

        Assert.Null(mtc?.Store);
        Assert.Null(mtc?.Strategy);
    }

    [Fact]
    public void SetTenantContextAcccessor()
    {
        var services = new ServiceCollection();

        services.AddMultiTenant<Tenant>();
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var context = httpContextMock.Object;

        var ti2 = new Tenant { Id = 1 };
        var res = context.SetTenant(ti2, false);
        var mtc = context.GetTenantContext<Tenant>();
        var accessor = context.RequestServices.GetRequiredService<ITenantContextAccessor<Tenant>>();

        Assert.True(res);
        Assert.Same(mtc, accessor.TenantContext);
    }

    [Fact]
    public void SetTenantInfo()
    {
        var services = new ServiceCollection();

        services.AddMultiTenant<Tenant>();
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var context = httpContextMock.Object;

        var ti2 = new Tenant { Id = 1 };
        var res = context.SetTenant<Tenant>(ti2, false);
        var mtc = context.GetTenantContext<Tenant>();

        Assert.True(res);
        Assert.Same(ti2, mtc!.Tenant);
    }
}
