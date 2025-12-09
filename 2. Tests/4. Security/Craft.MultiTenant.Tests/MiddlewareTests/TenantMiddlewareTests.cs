using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Craft.MultiTenant.Tests.MiddlewareTests;

public class TenantMiddlewareTests
{
    [Fact]
    public async Task SetMultiTenantContextAccessor()
    {
        var services = new ServiceCollection();

        services.AddMultiTenant<Tenant>().
            WithStaticStrategy("initech").
            WithInMemoryStore();

        var sp = services.BuildServiceProvider();

        var store = sp.GetService<ITenantStore<Tenant>>();
        await store!.AddAsync(new Tenant { Id = 1, Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);

        var mw = new TenantMiddleware(c =>
        {
            var accessor = c.RequestServices.GetRequiredService<ITenantContextAccessor<Tenant>>();
            Assert.NotNull(accessor.TenantContext);
            return Task.CompletedTask;
        });

        await mw.Invoke(context.Object);
    }

    [Fact]
    public async Task UseResolver()
    {
        var services = new ServiceCollection();

        services.AddMultiTenant<Tenant>().
            WithStaticStrategy("initech").
            WithInMemoryStore();

        var sp = services.BuildServiceProvider();
        var store = sp.GetService<ITenantStore<Tenant>>();

        await store!.AddAsync(new Tenant { Id = 1, Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);

        var mw = new TenantMiddleware(_ =>
        {
            Assert.Equal(1, context?.Object.RequestServices.GetService<ITenant>()?.Id);
            return Task.CompletedTask;
        });

        await mw.Invoke(context.Object);
    }
}
