using Craft.Auditing;
using Craft.Core;
using Craft.Domain;
using Craft.IntegrationTests.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.IntegrationTests.Fixtures;

/// <summary>
/// Provides a shared database fixture for integration tests using InMemory database.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    public IntegrationTestDbContext DbContext { get; private set; } = null!;
    public IServiceProvider ServiceProvider { get; private set; } = null!;
    private readonly string _dbName = $"IntegrationTestDb_{Guid.NewGuid()}";

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<IntegrationTestDbContext>(options =>
            options.UseInMemoryDatabase(_dbName));

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<IntegrationTestDbContext>());

        services.AddLogging(builder => builder
            .AddConsole()
            .SetMinimumLevel(LogLevel.Debug));

        ServiceProvider = services.BuildServiceProvider();

        DbContext = ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
        await DbContext.Database.EnsureCreatedAsync();

        // Seed test data
        await SeedTestDataAsync();
    }

    private async Task SeedTestDataAsync()
    {
        // Seed categories
        var categories = new[]
        {
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Books" },
            new Category { Id = 3, Name = "Clothing" }
        };
        DbContext.Categories.AddRange(categories);

        // Seed products
        var products = new[]
        {
            new Product { Id = 1, Name = "Laptop", Price = 999.99m, CategoryId = 1, TenantId = 1 },
            new Product { Id = 2, Name = "Smartphone", Price = 599.99m, CategoryId = 1, TenantId = 1 },
            new Product { Id = 3, Name = "C# Programming", Price = 49.99m, CategoryId = 2, TenantId = 1 },
            new Product { Id = 4, Name = "T-Shirt", Price = 29.99m, CategoryId = 3, TenantId = 2 },
            new Product { Id = 5, Name = "Deleted Product", Price = 19.99m, CategoryId = 1, TenantId = 1, IsDeleted = true }
        };
        DbContext.Products.AddRange(products);

        // Seed customers
        var customers = new[]
        {
            new Customer { Id = 1, Name = "John Doe", Email = "john@example.com", TenantId = 1 },
            new Customer { Id = 2, Name = "Jane Smith", Email = "jane@example.com", TenantId = 1 },
            new Customer { Id = 3, Name = "Bob Wilson", Email = "bob@example.com", TenantId = 2 }
        };
        DbContext.Customers.AddRange(customers);

        // Seed orders
        var orders = new[]
        {
            new Order { Id = 1, CustomerId = 1, OrderDate = DateTime.UtcNow.AddDays(-5), TotalAmount = 1099.98m, TenantId = 1 },
            new Order { Id = 2, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-3), TotalAmount = 49.99m, TenantId = 1 },
            new Order { Id = 3, CustomerId = 3, OrderDate = DateTime.UtcNow.AddDays(-1), TotalAmount = 29.99m, TenantId = 2 }
        };
        DbContext.Orders.AddRange(orders);

        await DbContext.SaveChangesAsync();

        // Detach all entities to avoid tracking issues
        DbContext.ChangeTracker.Clear();
    }

    public async Task ResetDatabaseAsync()
    {
        // Clear all data and re-seed
        DbContext.ChangeTracker.Clear();

        DbContext.OrderItems.RemoveRange(DbContext.OrderItems);
        DbContext.Orders.RemoveRange(DbContext.Orders);
        DbContext.Customers.RemoveRange(DbContext.Customers.IgnoreQueryFilters());
        DbContext.Products.RemoveRange(DbContext.Products.IgnoreQueryFilters());
        DbContext.Categories.RemoveRange(DbContext.Categories.IgnoreQueryFilters());
        DbContext.AuditTrails.RemoveRange(DbContext.AuditTrails);

        await DbContext.SaveChangesAsync();
        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null)
            await DbContext.DisposeAsync();
    }
}

/// <summary>
/// Alternative fixture using InMemory database for faster tests.
/// </summary>
public class InMemoryDatabaseFixture : IAsyncLifetime
{
    public IntegrationTestDbContext DbContext { get; private set; } = null!;
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<IntegrationTestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}"));

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<IntegrationTestDbContext>());

        services.AddLogging(builder => builder
            .AddConsole()
            .SetMinimumLevel(LogLevel.Debug));

        ServiceProvider = services.BuildServiceProvider();

        DbContext = ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
        await DbContext.Database.EnsureCreatedAsync();

        await SeedTestDataAsync();
    }

    private async Task SeedTestDataAsync()
    {
        var categories = new[]
        {
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Books" },
            new Category { Id = 3, Name = "Clothing" }
        };
        DbContext.Categories.AddRange(categories);

        var products = new[]
        {
            new Product { Id = 1, Name = "Laptop", Price = 999.99m, CategoryId = 1, TenantId = 1 },
            new Product { Id = 2, Name = "Smartphone", Price = 599.99m, CategoryId = 1, TenantId = 1 },
            new Product { Id = 3, Name = "C# Programming", Price = 49.99m, CategoryId = 2, TenantId = 1 },
            new Product { Id = 4, Name = "T-Shirt", Price = 29.99m, CategoryId = 3, TenantId = 2 }
        };
        DbContext.Products.AddRange(products);

        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null)
            await DbContext.DisposeAsync();
    }
}
