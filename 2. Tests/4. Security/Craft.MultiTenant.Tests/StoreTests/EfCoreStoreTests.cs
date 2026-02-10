using Craft.MultiTenant.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Craft.MultiTenant.Tests.StoreTests;

public class EfCoreStoreTests : TenantStoreTestBase, IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override ITenantStore<Tenant> CreateTestStore()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        var dbContext = new TestEfCoreStoreDbContext(options);
        dbContext.Database.EnsureCreated();

        var store = new DbStore<TestEfCoreStoreDbContext, Tenant>(dbContext);
        return PopulateTestStore(store);
    }

    [Fact]
    public override void AddTenantToStore()
    {
        base.AddTenantToStore();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
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
    public override void UpdateTenantInStore()
    {
        base.UpdateTenantInStore();
    }

    public class TestEfCoreStoreDbContext(DbContextOptions options) :
        DbContext(options), ITenantDbContext<Tenant>
    {
        public DbSet<Tenant> Tenants { get; set; } = null!; // Suppress CS8618 by using null-forgiving operator
    }
}

