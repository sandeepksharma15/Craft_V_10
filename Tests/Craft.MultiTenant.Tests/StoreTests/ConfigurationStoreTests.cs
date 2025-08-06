using Craft.MultiTenant.Stores;
using Microsoft.Extensions.Configuration;

namespace Craft.MultiTenant.Tests.StoreTests;

public class ConfigurationStoreTests : TenantStoreTestBase
{
    protected override ITenantStore<Tenant> CreateTestStore()
    {
        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");

        var configuration = configBuilder.Build();

        return new ConfigurationStore<Tenant>(configuration);
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
    public async Task IgnoreCaseWhenGettingTenantInfoFromStoreByIdentifier()
    {
        var store = CreateTestStore();

        Assert.Equal("initech", (await store!.GetByIdentifierAsync("INITECH"))?.Identifier);
    }

    [Fact]
    public void NotThrowIfNoDefaultSection()
    {
        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddJsonFile("ConfigurationStoreTestSettings_NoDefaults.json");

        IConfiguration configuration = configBuilder.Build();

        _ = new ConfigurationStore<Tenant>(configuration);
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
    public void ThrowIfEmptyOrNullSection()
    {
        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");

        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<ArgumentException>(() => new ConfigurationStore<Tenant>(configuration, ""));
        Assert.Throws<ArgumentNullException>(() => new ConfigurationStore<Tenant>(configuration, null!));
    }

    [Fact]
    public void ThrowIfInvalidSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<MultiTenantException>(() => new ConfigurationStore<Tenant>(configuration, "invalid"));
    }

    [Fact]
    public void ThrowIfNullConfiguration()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfigurationStore<Tenant>(null!));
    }

    [Fact]
    public async Task ThrowWhenTryingToGetIdentifierGivenNullIdentifier()
    {
        var store = CreateTestStore();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.GetByIdentifierAsync(null!));
    }
}
