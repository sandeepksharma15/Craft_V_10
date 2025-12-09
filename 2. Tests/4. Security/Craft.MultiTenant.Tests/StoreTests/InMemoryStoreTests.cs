using Craft.MultiTenant.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.MultiTenant.Tests.StoreTests;

public class InMemoryStoreTests : TenantStoreTestBase
{
    private static ITenantStore<Tenant> CreateCaseSensitiveTestStore()
    {
        var services = new ServiceCollection();
        services.AddOptions().Configure<InMemoryStoreOptions<Tenant>>(o => o.IsCaseSensitive = true);
        var sp = services.BuildServiceProvider();

        var store = new InMemoryStore<Tenant>(sp.GetRequiredService<IOptions<InMemoryStoreOptions<Tenant>>>());

        store.AddAsync(new Tenant { Id = 1, Identifier = "initech", Name = "initech" }).Wait();
        store.AddAsync(new Tenant { Id = 2, Identifier = "lol", Name = "lol" }).Wait();

        return store;
    }

    protected override ITenantStore<Tenant> CreateTestStore()
    {
        var optionsMock = new Mock<IOptions<InMemoryStoreOptions<Tenant>>>();
        var options = new InMemoryStoreOptions<Tenant>
        {
            IsCaseSensitive = false,
            Tenants = []
        };
        optionsMock.Setup(o => o.Value).Returns(options);
        var store = new InMemoryStore<Tenant>(optionsMock.Object);

        return PopulateTestStore(store);
    }

    [Fact]
    public override void AddTenantToStore()
    {
        base.AddTenantToStore();
    }

    [Fact]
    public async Task FailIfAddingWithoutTenantIdentifier()
    {
        var store = CreateCaseSensitiveTestStore();
        var count = await store.GetCountAsync();

        await store.AddAsync(new Tenant { Id = default, Name = "null" });

        Assert.Equal(count, await store.GetCountAsync());
    }

    [Fact]
    public override void GetAllTenantsFromStore()
    {
        base.GetAllTenantsFromStore();
    }

    [Fact]
    public override void GetTenantFromStoreById()
    {
        base.GetTenantFromStoreById();
    }

    [Fact]
    public override void GetTenantFromStoreByIdentifier()
    {
        base.GetTenantFromStoreByIdentifier();
    }

    [Fact]
    public async Task GetTenantInfoFromStoreCaseInsensitiveByDefault()
    {
        var store = CreateTestStore();
        Assert.Equal("initech", (await store!.GetByIdentifierAsync("iNitEch"))?.Identifier);
    }

    [Fact]
    public async Task GetTenantInfoFromStoreCaseSensitive()
    {
        var store = CreateCaseSensitiveTestStore();
        Assert.Equal("initech", (await store!.GetByIdentifierAsync("initech"))?.Identifier);
        Assert.Null(await store.GetByIdentifierAsync("iNitEch"));
    }

    [Fact]
    public override void RemoveTenantFromStore()
    {
        base.RemoveTenantFromStore();
    }

    [Fact]
    public override void ReturnNullWhenGettingByIdentifierIfTenantNotFound()
    {
        base.ReturnNullWhenGettingByIdentifierIfTenantNotFound();
    }

    [Fact]
    public override void ReturnNullWhenGettingByIdIfTenantNotFound()
    {
        base.ReturnNullWhenGettingByIdIfTenantNotFound();
    }

    [Fact]
    public void ThrowIfDuplicateIdentifierInOptionsTenants()
    {
        var services = new ServiceCollection();
        services.AddOptions().Configure<InMemoryStoreOptions<Tenant>>(options =>
        {
            options.Tenants.Add(new Tenant { Id = 1, Identifier = "lol", Name = "LOL", ConnectionString = "Datasource=lol.db" });
            options.Tenants.Add(new Tenant { Id = 2, Identifier = "lol", Name = "LOL", ConnectionString = "Datasource=lol.db" });
        });
        var sp = services.BuildServiceProvider();

        Assert.Throws<MultiTenantException>(() =>
            new InMemoryStore<Tenant>(sp.GetRequiredService<IOptions<InMemoryStoreOptions<Tenant>>>()));
    }

    [Theory]
    [InlineData(1L, "")]
    [InlineData(1, null)]
    [InlineData(0, "")]
    [InlineData(1L, null)]
    public void ThrowIfMissingIdOrIdentifierInOptionsTenants(long id, string? identifier)
    {
        var services = new ServiceCollection();
        services.AddOptions().Configure<InMemoryStoreOptions<Tenant>>(options =>
            options.Tenants.Add(new Tenant { Id = id, Identifier = identifier!, Name = "LOL", ConnectionString = "Datasource=lol.db" }));
        var sp = services.BuildServiceProvider();

        Assert.Throws<MultiTenantException>(() => new InMemoryStore<Tenant>(
            sp.GetRequiredService<IOptions<InMemoryStoreOptions<Tenant>>>()));
    }

    [Fact]
    public override void UpdateTenantInStore()
    {
        base.UpdateTenantInStore();
    }
}
