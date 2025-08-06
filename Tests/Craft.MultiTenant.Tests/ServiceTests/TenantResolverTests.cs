using Craft.MultiTenant.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.MultiTenant.Tests.ServiceTests;

public class TenantResolverTests
{
    [Fact]
    public async Task IgnoreSomeIdentifiersFromOptions()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.
            AddMultiTenant<Tenant>(options => options.IgnoredIdentifiers.Add("lol")).
            WithDelegateStrategy(_ => Task.FromResult("lol")). // should be ignored
            WithStaticStrategy("initech").
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        await sp.GetServices<ITenantStore<Tenant>>().
            Single(i => i.GetType() == typeof(InMemoryStore<Tenant>)).AddAsync(new Tenant { Id = 0, Identifier = "null" });

        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        var result = await resolver.ResolveAsync(new DefaultHttpContext());

        Assert.Equal("initech", result?.Tenant?.Identifier);
        Assert.IsType<StaticStrategy>(result?.Strategy);
        Assert.IsType<ConfigurationStore<Tenant>>(result.Store);
    }

    [Fact]
    public void InitializeSortedStrategiesFromDi()
    {
        var services = new ServiceCollection();
        services.
            AddMultiTenant<Tenant>().
            WithDelegateStrategy(_ => Task.FromResult("strategy1")).
            WithStaticStrategy("strategy2").
            WithDelegateStrategy(_ => Task.FromResult("strategy3")).
            WithInMemoryStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        var strategies = resolver.Strategies.ToArray();

        Assert.Equal(3, strategies.Length);
        Assert.IsType<DelegateStrategy>(strategies[0]);
        Assert.IsType<DelegateStrategy>(strategies[1]);
        Assert.IsType<StaticStrategy>(strategies[2]); // Note the Static strategy should be last due its priority.
    }

    [Fact]
    public void InitializeStoresFromDi()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.
            AddMultiTenant<Tenant>().
            WithStaticStrategy("strategy").
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        var stores = resolver.Stores.ToArray();

        Assert.Equal(2, stores.Length);
        Assert.IsType<InMemoryStore<Tenant>>(stores[0]);
        Assert.IsType<ConfigurationStore<Tenant>>(stores[1]);
    }

    [Fact]
    public async Task ReturnNullIfNoStoreSuccess()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddLogging().
            AddMultiTenant<Tenant>().
            WithDelegateStrategy(_ => Task.FromResult("not-found")).
            WithStaticStrategy("also-not-found").
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        await sp.
            GetServices<ITenantStore<Tenant>>().
            Single(i => i.GetType() == typeof(InMemoryStore<Tenant>)).AddAsync(new Tenant { Id = 0, Identifier = "null" });

        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        var result = await resolver.ResolveAsync(new DefaultHttpContext());

        Assert.Null(result);
    }

    [Fact]
    public async Task ReturnNullIfNoStrategySuccess()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.
            AddMultiTenant<Tenant>().
            WithDelegateStrategy(_ => Task.FromResult<string>(null!)).
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        await sp.GetServices<ITenantStore<Tenant>>().
            Single(i => i.GetType() == typeof(InMemoryStore<Tenant>)).AddAsync(new Tenant { Id = 0, Identifier = "null" });

        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        var result = await resolver.ResolveAsync(new DefaultHttpContext());

        Assert.Null(result);
    }

    [Fact]
    public async Task ReturnNullTenantContextGivenNoStrategies()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.
            AddMultiTenant<Tenant>().
            WithInMemoryStore().
            WithConfigurationStore();

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        var result = await resolver.ResolveAsync(new DefaultHttpContext());

        Assert.Null(result);
    }

    [Fact]
    public async Task ReturnTenantContext()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.
            AddMultiTenant<Tenant>().
            WithDelegateStrategy(_ => Task.FromResult("not-found")).
            WithStaticStrategy("initech").
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        await sp.GetServices<ITenantStore<Tenant>>().
            Single(i => i.GetType() == typeof(InMemoryStore<Tenant>)).AddAsync(new Tenant { Id = 0, Identifier = "null" });

        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        var result = await resolver.ResolveAsync(new DefaultHttpContext());

        Assert.Equal("initech", result?.Tenant?.Identifier);
        Assert.IsType<StaticStrategy>(result?.Strategy);
        Assert.IsType<ConfigurationStore<Tenant>>(result?.Store);
    }

    [Fact]
    public void ThrowGivenDelegateStrategyWithNullArgument()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.
            AddMultiTenant<Tenant>().
            WithDelegateStrategy(null!).
            WithInMemoryStore().
            WithConfigurationStore());
    }

    [Fact]
    public void ThrowGivenStaticStrategyWithNullIdentifierArgument()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.
            AddMultiTenant<Tenant>().
            WithStaticStrategy(null!).
            WithInMemoryStore().
            WithConfigurationStore());
    }
}
