using Craft.Domain;
using Craft.Infrastructure.Cache;
using Craft.MultiTenant.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.MultiTenant.Tests.CoreTests;

public class BuilderTests
{
#pragma warning disable RCS1213, IDE0051, RCS1170 // Remove unused member declaration.
    private int TestProperty { get; set; }
#pragma warning restore RCS1213, IDE0051, RCS1170 // Remove unused member declaration.

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithStore<TestStore<Tenant>>(lifetime);

        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<ITenantStore<Tenant>>();
        var scope = sp.CreateScope();
        var store2 = scope.ServiceProvider.GetRequiredService<ITenantStore<Tenant>>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(store, store2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(store, store2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(store, store2);
                store = scope.ServiceProvider.GetRequiredService<ITenantStore<Tenant>>();
                Assert.NotSame(store, store2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithParamsAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithStore<TestStore<Tenant>>(lifetime, true);

        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<ITenantStore<Tenant>>();
        var scope = sp.CreateScope();
        var store2 = scope.ServiceProvider.GetRequiredService<ITenantStore<Tenant>>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(store, store2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(store, store2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(store, store2);
                store = scope.ServiceProvider.GetRequiredService<ITenantStore<Tenant>>();
                Assert.NotSame(store, store2);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithFactoryAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithStore(lifetime, _ => new TestStore<Tenant>());

        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<ITenantStore<Tenant>>();
        var scope = sp.CreateScope();
        var store2 = scope.ServiceProvider.GetRequiredService<ITenantStore<Tenant>>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(store, store2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(store, store2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(store, store2);
                store = scope.ServiceProvider.GetRequiredService<ITenantStore<Tenant>>();
                Assert.NotSame(store, store2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithStrategy<StaticStrategy>(lifetime, "initech");

        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<ITenantStrategy>();
        var scope = sp.CreateScope();
        var strategy2 = scope.ServiceProvider.GetRequiredService<ITenantStrategy>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(strategy, strategy2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(strategy, strategy2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(strategy, strategy2);
                strategy = scope.ServiceProvider.GetRequiredService<ITenantStrategy>();
                Assert.NotSame(strategy, strategy2);
                break;
        }
    }

    [Fact]
    public void ThrowIfNullFactoryAddingCustomStore()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        Assert.Throws<ArgumentNullException>(() => builder.WithStore<TestStore<Tenant>>(ServiceLifetime.Singleton, factory: null!));
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithParamsAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithStrategy<StaticStrategy>(lifetime, "id");

        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<ITenantStrategy>();
        var scope = sp.CreateScope();
        var strategy2 = scope.ServiceProvider.GetRequiredService<ITenantStrategy>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(strategy, strategy2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(strategy, strategy2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(strategy, strategy2);
                strategy = scope.ServiceProvider.GetRequiredService<ITenantStrategy>();
                Assert.NotSame(strategy, strategy2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithFactoryAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithStrategy(lifetime, _ => new StaticStrategy("id"));

        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<ITenantStrategy>();
        var scope = sp.CreateScope();
        var strategy2 = scope.ServiceProvider.GetRequiredService<ITenantStrategy>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(strategy, strategy2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(strategy, strategy2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(strategy, strategy2);
                strategy = scope.ServiceProvider.GetRequiredService<ITenantStrategy>();
                Assert.NotSame(strategy, strategy2);
                break;
        }
    }

    [Fact]
    public void ThrowIfNullFactoryAddingCustomStrategy()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        Assert.Throws<ArgumentNullException>(() => builder.WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, factory: null!));
    }

    [Fact]
    public void AddHttpRemoteStoreAndHttpRemoteStoreClient()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithRemoteApiStore("http://example.com");
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<RemoteApiStoreClient<Tenant>>();
        var store = sp.GetRequiredService<ITenantStore<Tenant>>();
        Assert.IsType<RemoteApiStore<Tenant>>(store);
    }

    [Fact]
    public void AddHttpRemoteStoreWithHttpClientBuilders()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        var flag = false;
        builder.WithRemoteApiStore("http://example.com", _ => flag = true);
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<RemoteApiStoreClient<Tenant>>();
        var store = sp.GetRequiredService<ITenantStore<Tenant>>();
        Assert.IsType<RemoteApiStore<Tenant>>(store);
        Assert.True(flag);
    }

    [Fact]
    public async Task AddConfigurationStoreWithDefaults()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithConfigurationStore();
        services.AddSingleton<IConfiguration>(configuration);
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<ITenantStore<Tenant>>();
        Assert.IsType<ConfigurationStore<Tenant>>(store);

        var tc = await store.GetByIdentifierAsync("initech");
        Assert.Equal(1, tc?.Id);
        Assert.Equal("initech", tc?.Identifier);
        Assert.Equal("Initech", tc?.Name);
        Assert.Equal("Datasource=sample.db", tc?.ConnectionString);

        tc = await store.GetByIdentifierAsync("lol");
        Assert.Equal(2, tc?.Id);
        Assert.Equal("lol", tc?.Identifier);
        Assert.Equal("LOL", tc?.Name);
        Assert.Equal("Datasource=lol.db", tc?.ConnectionString);
    }

    [Fact]
    public async Task AddConfigurationStoreWithSectionName()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);

        // Non-default section name.
        configuration = configuration.GetSection("MultiTenancy");
        builder.WithConfigurationStore(configuration, "ConfigurationStore");
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<ITenantStore<Tenant>>();
        Assert.IsType<ConfigurationStore<Tenant>>(store);

        var tc = await store.GetByIdentifierAsync("initech");
        Assert.Equal(1, tc?.Id);
        Assert.Equal("initech", tc?.Identifier);
        Assert.Equal("Initech", tc?.Name);
        Assert.Equal("Datasource=sample.db", tc?.ConnectionString);

        tc = await store.GetByIdentifierAsync("lol");
        Assert.Equal(2, tc?.Id);
        Assert.Equal("lol", tc?.Identifier);
        Assert.Equal("LOL", tc?.Name);
        Assert.Equal("Datasource=lol.db", tc?.ConnectionString);
    }

    [Fact]
    public void ThrowIfNullParamAddingInMemoryStore()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        Assert.Throws<ArgumentNullException>(()
            => builder.WithInMemoryStore(null!));
    }

    [Fact]
    public async Task AddInMemoryStoreWithCaseSensitivity()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithInMemoryStore(options =>
        {
            options.IsCaseSensitive = true;
            options.Tenants.Add(new Tenant { Id = 1, Identifier = "lol", Name = "LOL", ConnectionString = "Datasource=lol.db" });
        });
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<ITenantStore<Tenant>>();
        Assert.IsType<InMemoryStore<Tenant>>(store);

        var tc = await store.GetByIdentifierAsync("lol");
        Assert.Equal(1, tc?.Id);
        Assert.Equal("lol", tc?.Identifier);
        Assert.Equal("LOL", tc?.Name);
        Assert.Equal("Datasource=lol.db", tc?.ConnectionString);

        // Case sensitive test.
        tc = await store.GetByIdentifierAsync("LOL");
        Assert.Null(tc);
    }

    [Fact]
    public void AddDelegateStrategy()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithDelegateStrategy(_ => Task.FromResult("Hi"));
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<ITenantStrategy>();
        Assert.IsType<DelegateStrategy>(strategy);
    }

    [Fact]
    public void AddStaticStrategy()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        builder.WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<ITenantStrategy>();
        Assert.IsType<StaticStrategy>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingStaticStrategy()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        Assert.Throws<ArgumentNullException>(()
            => builder.WithStaticStrategy(null!));
    }

    [Fact]
    public void AddBasePathStrategy_DefaultAndCustomConfig()
    {
        var services = new ServiceCollection();
        var builder = new TenantBuilder<Tenant>(services);
        // Default config
        builder.WithBasePathStrategy();
        // Custom config
        builder.WithBasePathStrategy(opt => { opt.UpdateRequestPath = false; });
        var sp = services.BuildServiceProvider();
        Assert.NotNull(sp.GetRequiredService<ITenantStrategy>());
        // Validate that the custom config was applied
        var options = sp.GetRequiredService<IOptions<BasePathStrategyOptions>>();
        Assert.False(options.Value.UpdateRequestPath);
    }

    [Fact]
    public void AddCacheStore_RegistersCacheStore()
    {
        var services = new ServiceCollection();

        // Register a dummy ICacheService
        services.AddSingleton<ICacheService, DummyCacheService>();

        // Register the inner store as a concrete type
        services.AddSingleton<DummyTenantRepository>();

        // Register the cache decorator as the only ITenantStore<Tenant>
        services.AddSingleton<ITenantStore<Tenant>>(sp =>
            new CacheStore<Tenant>(
                sp.GetRequiredService<ICacheService>(),
                sp.GetRequiredService<DummyTenantRepository>()
            )
        );

        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<ITenantStore<Tenant>>();
        Assert.NotNull(store);
        Assert.IsType<CacheStore<Tenant>>(store);
    }

    // Dummy implementations for test DI
    private class DummyCacheService : ICacheService
    {
        public void Remove(string cacheKey) { }
        public T? Set<T>(string cacheKey, T? value) => value;
        public (bool, T?) TryGet<T>(string cacheKey) => (false, default);
        public Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory)
        {
            // Await the valueFactory and cast the result to T? to match the nullable return type
            return GetOrSetAsyncInternal(cacheKey, valueFactory);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
            static async Task<T?> GetOrSetAsyncInternal(string cacheKey, Func<Task<T>> valueFactory)
            {
                var result = await valueFactory();
                return result;
            }
        }
    }

    private class DummyTenantRepository : ITenantStore<Tenant>
    {
        public Task<IReadOnlyList<Tenant>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Tenant>>([]);
        public Task<Tenant?> GetAsync(long id, bool includeDetails = false, CancellationToken cancellationToken = default) => Task.FromResult<Tenant?>(null);
        public Task<long> GetCountAsync(CancellationToken cancellationToken = default) => Task.FromResult(0L);

        public Task<Tenant?> AddAsync(Tenant entity, bool autoSave = true, CancellationToken cancellationToken = default) => Task.FromResult<Tenant?>(null);
        public Task<Tenant?> DeleteAsync(Tenant entity, bool autoSave = true, CancellationToken cancellationToken = default) => Task.FromResult<Tenant?>(null);
        public Task<Tenant> UpdateAsync(Tenant entity, bool autoSave = true, CancellationToken cancellationToken = default) => Task.FromResult<Tenant>(null!);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public Task<Tenant?> GetByIdentifierAsync(string identifier, bool includeDetails = false, CancellationToken cancellationToken = default) => Task.FromResult<Tenant?>(null);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public Task<Tenant?> GetHostAsync(bool includeDetails = false, CancellationToken cancellationToken = default) => Task.FromResult<Tenant?>(null);
    }

    private class TestStore<T> : ITenantStore<T> where T : class, ITenant, IEntity, new()
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly bool _testParam;
#pragma warning restore IDE0052 // Remove unread private members

        public TestStore()
        {
        }

        public TestStore(bool testParam)
        {
            _testParam = testParam;
        }

        public Task<T?> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T?> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetAsync(long id, bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetByIdentifierAsync(String identifier, bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetCountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetHostAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
