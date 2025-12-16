using Craft.Auditing;
using Craft.Domain;
using Craft.IntegrationTests.Entities;
using Craft.IntegrationTests.Fixtures;
using Craft.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.IntegrationTests.CrossModule;

/// <summary>
/// Integration tests for Repository + Auditing cross-module functionality.
/// Tests that repository operations correctly trigger audit trail creation.
/// </summary>
[Collection(nameof(DatabaseTestCollection))]
public class RepositoryAuditingIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IntegrationTestDbContext _dbContext = null!;
    private IChangeRepository<Customer> _customerRepository = null!;
    private readonly KeyType _testUserId = 1;

    public RepositoryAuditingIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
        _dbContext = _fixture.DbContext;

        var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<ChangeRepository<Customer, KeyType>>>();
        _customerRepository = new ChangeRepository<Customer>(_dbContext, logger);

        // Reset audit trail configuration
        AuditTrail.ConfigureSerializerOptions(null);
        AuditTrail.DefaultRetentionDays = null;
        AuditTrail.IncludeNavigationProperties = true;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Complete Audit Loop Tests

    [Fact]
    public async Task FullAuditLoop_CreateUpdateDelete_GeneratesAllAuditTrails()
    {
        // Step 1: Create entity - should generate Created audit
        var customer = new Customer
        {
            Name = "Audited Customer",
            Email = "audited@example.com",
            TenantId = 1
        };

        _dbContext.Customers.Add(customer);
        await CreateAuditTrailsAndSaveAsync();

        var createAudits = await _dbContext.AuditTrails
            .Where(a => a.TableName == "Customer" && a.ChangeType == EntityChangeType.Created)
            .ToListAsync();

        Assert.Single(createAudits);
        var createAudit = createAudits[0];
        Assert.Contains("Audited Customer", createAudit.NewValues!);
        Assert.Contains("audited@example.com", createAudit.NewValues!);

        // Step 2: Update entity - should generate Updated audit
        _dbContext.ChangeTracker.Clear();
        var customerToUpdate = await _dbContext.Customers.FindAsync(customer.Id);
        Assert.NotNull(customerToUpdate);

        customerToUpdate.Name = "Updated Audited Customer";
        customerToUpdate.Phone = "+1234567890";
        await CreateAuditTrailsAndSaveAsync();

        var updateAudits = await _dbContext.AuditTrails
            .Where(a => a.TableName == "Customer" && a.ChangeType == EntityChangeType.Updated)
            .ToListAsync();

        Assert.Single(updateAudits);
        var updateAudit = updateAudits[0];
        Assert.Contains("Audited Customer", updateAudit.OldValues!);
        Assert.Contains("Updated Audited Customer", updateAudit.NewValues!);

        var changedColumns = updateAudit.GetChangedColumnsAsList();
        Assert.Contains(changedColumns!, c => c.Contains("Name"));

        // Step 3: Delete entity (soft delete) - should generate Deleted audit
        _dbContext.ChangeTracker.Clear();
        var customerToDelete = await _dbContext.Customers
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Id == customer.Id);

        customerToDelete.IsDeleted = true;
        await CreateAuditTrailsAndSaveAsync();

        var deleteAudits = await _dbContext.AuditTrails
            .Where(a => a.TableName == "Customer" && a.ChangeType == EntityChangeType.Deleted)
            .ToListAsync();

        Assert.Single(deleteAudits);
    }

    #endregion

    #region DoNotAudit Attribute Tests

    [Fact]
    public async Task AuditTrail_WithDoNotAuditAttribute_ExcludesProperty()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "Customer With Sensitive Data",
            Email = "sensitive@example.com",
            SensitiveData = "This should not be audited",
            TenantId = 1
        };

        // Act
        _dbContext.Customers.Add(customer);
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audit = await _dbContext.AuditTrails
            .Where(a => a.TableName == "Customer")
            .FirstAsync();

        Assert.NotNull(audit.NewValues);
        Assert.DoesNotContain("SensitiveData", audit.NewValues);
        Assert.DoesNotContain("This should not be audited", audit.NewValues);
        Assert.Contains("Email", audit.NewValues);
    }

    #endregion

    #region Query Extension Tests

    [Fact]
    public async Task AuditTrail_ForEntity_RetrievesCorrectAudits()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "Query Test Customer",
            Email = "query@example.com",
            TenantId = 1
        };

        _dbContext.Customers.Add(customer);
        await CreateAuditTrailsAndSaveAsync();

        // Act
        var audits = await _dbContext.AuditTrails
            .ForEntity<Customer>(customer.Id)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(audits);
        Assert.All(audits, a => Assert.Equal("Customer", a.TableName));
        Assert.All(audits, a => Assert.NotNull(a.KeyValues));
        Assert.All(audits, a => Assert.Contains($"\"Id\":{customer.Id}", a.KeyValues));
    }

    [Fact]
    public async Task AuditTrail_ChainedQueries_WorkCorrectly()
    {
        // Arrange - Create multiple audits
        for (int i = 0; i < 3; i++)
        {
            var customer = new Customer
            {
                Name = $"Chain Test Customer {i}",
                Email = $"chain{i}@example.com",
                TenantId = 1
            };
            _dbContext.Customers.Add(customer);
            await CreateAuditTrailsAndSaveAsync();
        }

        // Act
        var audits = await _dbContext.AuditTrails
            .ForTable("Customer")
            .OfChangeType(EntityChangeType.Created)
            .ByUser(_testUserId)
            .OrderByMostRecent()
            .ToListAsync();

        // Assert
        Assert.True(audits.Count >= 3);
        Assert.True(audits[0].DateTimeUTC >= audits[^1].DateTimeUTC);
    }

    #endregion

    #region Property Changes Tests

    [Fact]
    public async Task AuditTrail_GetPropertyChanges_ReturnsCorrectChanges()
    {
        // Arrange - Create
        var customer = new Customer
        {
            Name = "Original Name",
            Email = "original@example.com",
            Phone = "111-111-1111",
            TenantId = 1
        };
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        // Update
        _dbContext.ChangeTracker.Clear();
        var toUpdate = await _dbContext.Customers.FindAsync(customer.Id);
        Assert.NotNull(toUpdate);

        toUpdate.Name = "Changed Name";
        toUpdate.Phone = "222-222-2222";
        await CreateAuditTrailsAndSaveAsync();

        // Act
        var audit = await _dbContext.AuditTrails
            .Where(a => a.ChangeType == EntityChangeType.Updated && a.TableName == "Customer")
            .OrderByDescending(a => a.DateTimeUTC)
            .FirstAsync();

        var changes = audit.GetPropertyChanges();

        // Assert
        Assert.NotEmpty(changes);
        Assert.Contains(changes, c => c.PropertyName == "Name" && c.HasChanged);
        Assert.Contains(changes, c => c.PropertyName == "Phone" && c.HasChanged);
    }

    #endregion

    #region Retention Policy Tests

    [Fact]
    public async Task AuditTrail_WithRetentionPolicy_SetsArchiveAfter()
    {
        // Arrange
        AuditTrail.DefaultRetentionDays = 30;

        var customer = new Customer
        {
            Name = "Retention Test Customer",
            Email = "retention@example.com",
            TenantId = 1
        };

        // Act
        _dbContext.Customers.Add(customer);
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audit = await _dbContext.AuditTrails
            .OrderByDescending(a => a.DateTimeUTC)
            .FirstAsync();

        Assert.NotNull(audit.ArchiveAfter);
        Assert.True(audit.ArchiveAfter.Value > audit.DateTimeUTC);
        Assert.True(audit.ArchiveAfter.Value <= DateTime.UtcNow.AddDays(31));

        // Cleanup
        AuditTrail.DefaultRetentionDays = null;
    }

    #endregion

    #region Multi-Entity Audit Tests

    [Fact]
    public async Task AuditTrail_MultipleEntitiesInSingleSave_CreatesMultipleAudits()
    {
        // Arrange
        var customers = new[]
        {
            new Customer { Name = "Batch 1", Email = "batch1@example.com", TenantId = 1 },
            new Customer { Name = "Batch 2", Email = "batch2@example.com", TenantId = 1 },
            new Customer { Name = "Batch 3", Email = "batch3@example.com", TenantId = 1 }
        };

        // Act
        _dbContext.Customers.AddRange(customers);
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audits = await _dbContext.AuditTrails
            .Where(a => a.TableName == "Customer" && a.ChangeType == EntityChangeType.Created)
            .OrderByDescending(a => a.DateTimeUTC)
            .Take(3)
            .ToListAsync();

        Assert.Equal(3, audits.Count);
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public async Task AuditTrail_CompareBetweenVersions_IdentifiesDifferences()
    {
        // Arrange - Create
        var customer = new Customer
        {
            Name = "Version 1",
            Email = "v1@example.com",
            TenantId = 1
        };
        _dbContext.Customers.Add(customer);
        await CreateAuditTrailsAndSaveAsync();

        // Update
        _dbContext.ChangeTracker.Clear();
        var toUpdate = await _dbContext.Customers.FindAsync(customer.Id);
        Assert.NotNull(toUpdate);
        toUpdate.Name = "Version 2";
        toUpdate.Email = "v2@example.com";
        await CreateAuditTrailsAndSaveAsync();

        // Act
        var audits = await _dbContext.AuditTrails
            .Where(a => a.TableName == "Customer")
            .OrderBy(a => a.DateTimeUTC)
            .Take(2)
            .ToListAsync();

        Assert.True(audits.Count >= 2);

        var comparison = audits[1].CompareTo(audits[0]);

        // Assert
        Assert.True(comparison.HasDifferences);
    }

    #endregion

    #region Helper Methods

    private async Task CreateAuditTrailsAndSaveAsync()
    {
        var auditEntries = new List<AuditTrail>();

        foreach (var entry in _dbContext.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted)
            {
                var auditEntry = await AuditTrail.CreateAsync(entry, _testUserId);
                auditEntries.Add(auditEntry);
            }
        }

        _dbContext.AuditTrails.AddRange(auditEntries);
        await _dbContext.SaveChangesAsync();
    }

    #endregion
}
