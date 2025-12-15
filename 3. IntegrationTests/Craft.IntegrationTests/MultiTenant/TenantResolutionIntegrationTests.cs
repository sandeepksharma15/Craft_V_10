using Craft.Domain;
using Craft.MultiTenant;
using Craft.MultiTenant.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.IntegrationTests.MultiTenant;

/// <summary>
/// Integration tests for multi-tenant resolution and store operations.
/// </summary>
public class TenantResolutionIntegrationTests
{
    private readonly List<Tenant> _testTenants;

    public TenantResolutionIntegrationTests()
    {
        _testTenants =
        [
            new Tenant(1, "Alpha Corp", "alpha-db-connection", "alpha") { IsActive = true },
            new Tenant(2, "Beta Inc", "beta-db-connection", "beta") { IsActive = true },
            new Tenant(3, "Gamma LLC", "gamma-db-connection", "gamma") { IsActive = false },
            new Tenant(4, "Host", string.Empty, "host", string.Empty) { Type = TenantType.Host, IsActive = true }
        ];
    }

    #region InMemoryStore Tests

    [Fact]
    public async Task InMemoryStore_GetByIdentifier_ReturnsCorrectTenant()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);

        // Act
        var tenant = await store.GetByIdentifierAsync("alpha");

        // Assert
        Assert.NotNull(tenant);
        Assert.Equal("Alpha Corp", tenant.Name);
        Assert.Equal("alpha", tenant.Identifier);
    }

    [Fact]
    public async Task InMemoryStore_GetByIdentifier_CaseInsensitive_ReturnsCorrectTenant()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);

        // Act
        var tenant = await store.GetByIdentifierAsync("ALPHA");

        // Assert
        Assert.NotNull(tenant);
        Assert.Equal("Alpha Corp", tenant.Name);
    }

    [Fact]
    public async Task InMemoryStore_GetByIdentifier_NotFound_ReturnsNull()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);

        // Act
        var tenant = await store.GetByIdentifierAsync("nonexistent");

        // Assert
        Assert.Null(tenant);
    }

    [Fact]
    public async Task InMemoryStore_GetAsync_ById_ReturnsCorrectTenant()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);

        // Act
        var tenant = await store.GetAsync(1);

        // Assert
        Assert.NotNull(tenant);
        Assert.Equal("Alpha Corp", tenant.Name);
    }

    [Fact]
    public async Task InMemoryStore_GetAllAsync_ReturnsAllTenants()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);

        // Act
        var tenants = await store.GetAllAsync();

        // Assert
        Assert.Equal(4, tenants.Count);
    }

    [Fact]
    public async Task InMemoryStore_GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);

        // Act
        var count = await store.GetCountAsync();

        // Assert
        Assert.Equal(4, count);
    }

    [Fact]
    public async Task InMemoryStore_GetHostAsync_ReturnsHostTenant()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);

        // Act
        var host = await store.GetHostAsync();

        // Assert
        Assert.NotNull(host);
        Assert.Equal(TenantType.Host, host.Type);
        Assert.Equal("host", host.Identifier);
    }

    [Fact]
    public async Task InMemoryStore_AddAsync_AddsNewTenant()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);
        var newTenant = new Tenant(0, "Delta Corp", "delta-connection", "delta") { IsActive = true };

        // Act
        var added = await store.AddAsync(newTenant);

        // Assert
        Assert.NotNull(added);
        Assert.NotEqual(default, added.Id);

        var retrieved = await store.GetByIdentifierAsync("delta");
        Assert.NotNull(retrieved);
        Assert.Equal("Delta Corp", retrieved.Name);
    }

    [Fact]
    public async Task InMemoryStore_UpdateAsync_UpdatesTenant()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);
        var tenant = await store.GetAsync(1);
        Assert.NotNull(tenant);

        tenant.Name = "Alpha Corporation Updated";

        // Act
        var updated = await store.UpdateAsync(tenant);

        // Assert
        Assert.Equal("Alpha Corporation Updated", updated.Name);

        var retrieved = await store.GetAsync(1);
        Assert.NotNull(retrieved);
        Assert.Equal("Alpha Corporation Updated", retrieved.Name);
    }

    [Fact]
    public async Task InMemoryStore_DeleteAsync_RemovesTenant()
    {
        // Arrange
        var tenants = new List<Tenant>
        {
            new Tenant(1, "ToDelete", "conn", "to-delete") { IsActive = true }
        };
        var store = CreateInMemoryStore(tenants);

        var tenant = await store.GetByIdentifierAsync("to-delete");
        Assert.NotNull(tenant);

        // Act
        await store.DeleteAsync(tenant);

        // Assert
        var afterDelete = await store.GetByIdentifierAsync("to-delete");
        Assert.Null(afterDelete);
    }

    #endregion

    #region HeaderStrategy Tests

    [Fact]
    public async Task HeaderStrategy_WithTenantHeader_ReturnsIdentifier()
    {
        // Arrange
        var strategy = new HeaderStrategy(TenantConstants.TenantToken);
        var httpContext = CreateHttpContext(headers: new Dictionary<string, string>
        {
            { TenantConstants.TenantToken, "alpha" }
        });

        // Act
        var identifier = await strategy.GetIdentifierAsync(httpContext);

        // Assert
        Assert.Equal("alpha", identifier);
    }

    [Fact]
    public async Task HeaderStrategy_WithoutTenantHeader_ReturnsNull()
    {
        // Arrange
        var strategy = new HeaderStrategy(TenantConstants.TenantToken);
        var httpContext = CreateHttpContext();

        // Act
        var identifier = await strategy.GetIdentifierAsync(httpContext);

        // Assert
        Assert.Null(identifier);
    }

    [Fact]
    public async Task HeaderStrategy_WithCustomHeaderKey_ReturnsIdentifier()
    {
        // Arrange
        var customKey = "X-Custom-Tenant";
        var strategy = new HeaderStrategy(customKey);
        var httpContext = CreateHttpContext(headers: new Dictionary<string, string>
        {
            { customKey, "beta" }
        });

        // Act
        var identifier = await strategy.GetIdentifierAsync(httpContext);

        // Assert
        Assert.Equal("beta", identifier);
    }

    #endregion

    #region TenantResolver Integration Tests

    [Fact]
    public async Task TenantResolver_ResolvesFromHeader_ReturnsTenantContext()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);
        var strategy = new HeaderStrategy(TenantConstants.TenantToken);

        var resolver = new TenantResolver<Tenant>(
            [strategy],
            [store],
            CreateTenantOptions());

        var httpContext = CreateHttpContext(headers: new Dictionary<string, string>
        {
            { TenantConstants.TenantToken, "alpha" }
        });

        // Act
        var context = await resolver.ResolveAsync(httpContext);

        // Assert
        Assert.NotNull(context);
        Assert.True(context.HasResolvedTenant);
        Assert.NotNull(context.Tenant);
        Assert.Equal("Alpha Corp", context.Tenant.Name);
    }

    [Fact]
    public async Task TenantResolver_WithInvalidIdentifier_ReturnsNull()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);
        var strategy = new HeaderStrategy(TenantConstants.TenantToken);

        var resolver = new TenantResolver<Tenant>(
            [strategy],
            [store],
            CreateTenantOptions());

        var httpContext = CreateHttpContext(headers: new Dictionary<string, string>
        {
            { TenantConstants.TenantToken, "nonexistent" }
        });

        // Act
        var context = await resolver.ResolveAsync(httpContext);

        // Assert
        Assert.Null(context);
    }

    [Fact]
    public async Task TenantResolver_WithIgnoredIdentifier_ReturnsNull()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);
        var strategy = new HeaderStrategy(TenantConstants.TenantToken);

        var options = CreateTenantOptions(ignoredIdentifiers: ["ignored-tenant"]);
        var resolver = new TenantResolver<Tenant>(
            [strategy],
            [store],
            options);

        var httpContext = CreateHttpContext(headers: new Dictionary<string, string>
        {
            { TenantConstants.TenantToken, "ignored-tenant" }
        });

        // Act
        var context = await resolver.ResolveAsync(httpContext);

        // Assert
        Assert.Null(context);
    }

    [Fact]
    public async Task TenantResolver_WithMultipleStrategies_UsesFirstMatch()
    {
        // Arrange
        var store = CreateInMemoryStore(_testTenants);
        var headerStrategy = new HeaderStrategy(TenantConstants.TenantToken);
        var customHeaderStrategy = new HeaderStrategy("X-Alt-Tenant");

        var resolver = new TenantResolver<Tenant>(
            [headerStrategy, customHeaderStrategy],
            [store],
            CreateTenantOptions());

        var httpContext = CreateHttpContext(headers: new Dictionary<string, string>
        {
            { TenantConstants.TenantToken, "alpha" },
            { "X-Alt-Tenant", "beta" }
        });

        // Act
        var context = await resolver.ResolveAsync(httpContext);

        // Assert
        Assert.NotNull(context);
        Assert.Equal("Alpha Corp", context.Tenant?.Name);
    }

    #endregion

    #region Complete Tenant Resolution Loop

    [Fact]
    public async Task CompleteTenantLoop_CreateResolveUpdateDelete_WorksCorrectly()
    {
        // Arrange
        var tenants = new List<Tenant>(_testTenants);
        var store = CreateInMemoryStore(tenants);
        var strategy = new HeaderStrategy(TenantConstants.TenantToken);
        var resolver = new TenantResolver<Tenant>(
            [strategy],
            [store],
            CreateTenantOptions());

        // Step 1: Create new tenant
        var newTenant = new Tenant(0, "New Tenant", "new-connection", "new-tenant") { IsActive = true };
        var created = await store.AddAsync(newTenant);
        Assert.NotNull(created);
        Assert.NotEqual(default, created.Id);

        // Step 2: Resolve the new tenant
        var httpContext = CreateHttpContext(headers: new Dictionary<string, string>
        {
            { TenantConstants.TenantToken, "new-tenant" }
        });
        var resolvedContext = await resolver.ResolveAsync(httpContext);
        Assert.NotNull(resolvedContext);
        Assert.Equal("New Tenant", resolvedContext.Tenant?.Name);

        // Step 3: Update the tenant
        var tenantToUpdate = await store.GetByIdentifierAsync("new-tenant");
        Assert.NotNull(tenantToUpdate);
        tenantToUpdate.Name = "Updated Tenant Name";
        tenantToUpdate.IsActive = false;
        await store.UpdateAsync(tenantToUpdate);

        // Verify update
        var updatedTenant = await store.GetByIdentifierAsync("new-tenant");
        Assert.NotNull(updatedTenant);
        Assert.Equal("Updated Tenant Name", updatedTenant.Name);
        Assert.False(updatedTenant.IsActive);

        // Step 4: Delete the tenant
        await store.DeleteAsync(updatedTenant);

        // Verify deletion
        var deletedTenant = await store.GetByIdentifierAsync("new-tenant");
        Assert.Null(deletedTenant);

        // Verify resolution fails after deletion
        var postDeleteContext = await resolver.ResolveAsync(httpContext);
        Assert.Null(postDeleteContext);
    }

    #endregion

    #region Helper Methods

    private static InMemoryStore<Tenant> CreateInMemoryStore(IEnumerable<Tenant> tenants)
    {
        var options = Options.Create(new InMemoryStoreOptions<Tenant>
        {
            Tenants = tenants.ToList(),
            IsCaseSensitive = false
        });
        return new InMemoryStore<Tenant>(options);
    }

    private static IOptionsMonitor<TenantOptions> CreateTenantOptions(List<string>? ignoredIdentifiers = null)
    {
        var options = new TenantOptions
        {
            IgnoredIdentifiers = ignoredIdentifiers ?? []
        };

        var mockOptionsMonitor = new Mock<IOptionsMonitor<TenantOptions>>();
        mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(options);
        return mockOptionsMonitor.Object;
    }

    private static HttpContext CreateHttpContext(Dictionary<string, string>? headers = null)
    {
        var context = new DefaultHttpContext();

        if (headers != null)
        {
            foreach (var header in headers)
                context.Request.Headers[header.Key] = header.Value;
        }

        return context;
    }

    #endregion
}
