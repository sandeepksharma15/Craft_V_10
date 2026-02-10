using Craft.Core;
using Craft.MultiTenant;
using Craft.Repositories;
using Craft.TestHost.Data;
using Craft.TestHost.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.IntegrationTests.Fixtures;

/// <summary>
/// WebApplicationFactory fixture for HTTP integration tests.
/// </summary>
public class TestHostFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    private readonly string _dbName = $"TestHostDb_{Guid.NewGuid()}";

    public Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    // Replace the DbContext with a unique in-memory database for each test run
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<TestHostDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<TestHostDbContext>(options =>
                        options.UseInMemoryDatabase(_dbName));
                });
            });

        Client = Factory.CreateClient();

        // Seed the database manually
        SeedDatabase();

        return Task.CompletedTask;
    }

    private void SeedDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestHostDbContext>();
        dbContext.Database.EnsureCreated();

        // Clear existing data
        if (!dbContext.Products.Any())
        {
            dbContext.Products.AddRange(
                new TestProduct { Id = 1, Name = "Laptop", Price = 999.99m, TenantId = 1 },
                new TestProduct { Id = 2, Name = "Phone", Price = 599.99m, TenantId = 1 },
                new TestProduct { Id = 3, Name = "Tablet", Price = 399.99m, TenantId = 2 }
            );
            dbContext.SaveChanges();
        }
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (Factory != null)
            await Factory.DisposeAsync();
    }

    /// <summary>
    /// Creates a client with a specific tenant header.
    /// </summary>
    public HttpClient CreateClientWithTenant(string tenantIdentifier)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("__TENANT__", tenantIdentifier);
        return client;
    }
}

/// <summary>
/// Collection definition for HTTP integration tests.
/// </summary>
[CollectionDefinition(nameof(HttpTestCollection))]
public class HttpTestCollection : ICollectionFixture<TestHostFixture>;

