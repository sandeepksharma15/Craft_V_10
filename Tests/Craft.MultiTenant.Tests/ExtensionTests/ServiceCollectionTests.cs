using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.MultiTenant.Tests.ExtensionTests;

public class ServiceCollectionTests
{
    [Fact]
    public void RegisterIMultiTenantContextAccessorGenericInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>();

        var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                    s.ServiceType == typeof(ITenantContextAccessor<Tenant>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Singleton, service!.Lifetime);
    }

    [Fact]
    public void RegisterIMultiTenantContextAccessorInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>();

        var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                    s.ServiceType == typeof(ITenantContextAccessor));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Singleton, service!.Lifetime);
    }

    [Fact]
    public void RegisterITenantContextInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>();

        var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Scoped &&
                                                    s.ServiceType == typeof(ITenantContext<Tenant>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
    }

    [Fact]
    public void RegisterITenantInfoInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>();

        var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Scoped &&
                                                    s.ServiceType == typeof(ITenant));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
    }

    [Fact]
    public void RegisterITenantResolverGenericInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>();

        var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver<Tenant>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
    }

    [Fact]
    public void RegisterITenantResolverInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>();

        var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
    }

    [Fact]
    public void RegisterMultiTenantOptionsInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>();

        var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                    s.ServiceType == typeof(IConfigureOptions<TenantOptions>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Singleton, service!.Lifetime);
    }

    [Fact]
    public void RegisterTenantInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<Tenant>();

        var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Scoped &&
                                                    s.ServiceType == typeof(Tenant));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
    }
}
