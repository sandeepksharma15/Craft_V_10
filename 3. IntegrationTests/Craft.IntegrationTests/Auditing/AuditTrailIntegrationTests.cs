using Craft.Auditing;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.IntegrationTests.Auditing;

public class AuditTrailIntegrationTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly KeyType _userId = 1;

    public AuditTrailIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _context.Database.EnsureCreated();

        AuditTrail.ConfigureSerializerOptions(null);
        AuditTrail.DefaultRetentionDays = null;
        AuditTrail.IncludeNavigationProperties = true;
    }

    [Fact]
    public async Task SaveChanges_WithNewEntity_CreatesAuditTrail()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Price = 99.99m,
            Description = "Test Description"
        };

        // Act
        _context.Products.Add(product);
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audits = await _context.AuditTrails
            .Where(a => a.TableName == "Product")
            .ToListAsync();

        Assert.Single(audits);
        var audit = audits[0];
        Assert.Equal(EntityChangeType.Created, audit.ChangeType);
        Assert.NotNull(audit.NewValues);
        Assert.Null(audit.OldValues);
        Assert.Contains("Test Product", audit.NewValues);
    }

    [Fact]
    public async Task SaveChanges_WithUpdatedEntity_CreatesUpdateAuditTrail()
    {
        // Arrange
        var product = new Product { Name = "Original", Price = 100m };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        product.Name = "Updated";
        product.Price = 150m;
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audits = await _context.AuditTrails
            .Where(a => a.TableName == "Product" && a.ChangeType == EntityChangeType.Updated)
            .ToListAsync();

        Assert.Single(audits);
        var audit = audits[0];
        Assert.Contains("Original", audit.OldValues!);
        Assert.Contains("Updated", audit.NewValues!);

        var changedColumns = audit.GetChangedColumnsAsList();
        Assert.NotNull(changedColumns);
        Assert.Contains(changedColumns, c => c.Contains("Name"));
        Assert.Contains(changedColumns, c => c.Contains("Price"));
    }

    [Fact]
    public async Task SaveChanges_WithDeletedEntity_CreatesDeleteAuditTrail()
    {
        // Arrange
        var product = new Product { Name = "To Delete", Price = 50m };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        _context.Products.Remove(product);
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audits = await _context.AuditTrails
            .Where(a => a.TableName == "Product" && a.ChangeType == EntityChangeType.Deleted)
            .ToListAsync();

        Assert.Single(audits);
        var audit = audits[0];
        Assert.NotNull(audit.OldValues);
        Assert.Contains("To Delete", audit.OldValues);
    }

    [Fact]
    public async Task SaveChanges_WithSoftDelete_CreatesDeleteAuditTrail()
    {
        // Arrange
        var document = new Document { Title = "Test Document" };
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // Act
        document.IsDeleted = true;
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audits = await _context.AuditTrails
            .Where(a => a.TableName == "Document" && a.ChangeType == EntityChangeType.Deleted)
            .ToListAsync();

        Assert.Single(audits);
        Assert.True(document.IsDeleted);
    }

    [Fact]
    public async Task SaveChanges_WithDoNotAuditProperty_ExcludesProperty()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "secret123"
        };

        // Act
        _context.Users.Add(user);
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audits = await _context.AuditTrails
            .Where(a => a.TableName == "User")
            .ToListAsync();

        Assert.Single(audits);
        var audit = audits[0];
        Assert.NotNull(audit.NewValues);
        Assert.DoesNotContain("PasswordHash", audit.NewValues);
        Assert.Contains("Email", audit.NewValues);
        Assert.Contains("Username", audit.NewValues);
    }

    [Fact]
    public async Task SaveChanges_WithNavigationProperty_TracksRelatedEntity()
    {
        // Arrange
        var category = new Category { Name = "Electronics" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Laptop",
            Price = 1000m,
            Category = category
        };

        // Act
        _context.Products.Add(product);
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audits = await _context.AuditTrails
            .Where(a => a.TableName == "Product")
            .ToListAsync();

        Assert.Single(audits);
        var audit = audits[0];
        Assert.NotNull(audit.NewValues);
        Assert.Contains("Category_Navigation", audit.NewValues);
    }

    [Fact]
    public async Task QueryExtensions_ForEntity_RetrievesCorrectAudits()
    {
        // Arrange
        var product = new Product { Name = "Test", Price = 100m };
        _context.Products.Add(product);
        await CreateAuditTrailsAndSaveAsync();

        // Act
        var audits = await _context.AuditTrails
            .ForEntity<Product>(product.Id)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(audits);
        Assert.All(audits, a => Assert.Equal("Product", a.TableName));
    }

    [Fact]
    public async Task QueryExtensions_ChainedQueries_WorkCorrectly()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var product = new Product { Name = $"Product {i}", Price = 100m * i };
            _context.Products.Add(product);
            await CreateAuditTrailsAndSaveAsync();
        }

        // Act
        var audits = await _context.AuditTrails
            .ForTable("Product")
            .OfChangeType(EntityChangeType.Created)
            .ByUser(_userId)
            .OrderByMostRecent()
            .ToListAsync();

        // Assert
        Assert.Equal(5, audits.Count);
        Assert.True(audits[0].DateTimeUTC >= audits[^1].DateTimeUTC);
    }

    [Fact]
    public async Task AuditExtensions_GetPropertyChanges_ReturnsCorrectChanges()
    {
        // Arrange
        var product = new Product { Name = "Original", Price = 100m };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        product.Name = "Updated";
        product.Price = 200m;
        await CreateAuditTrailsAndSaveAsync();

        // Act
        var audit = await _context.AuditTrails
            .Where(a => a.ChangeType == EntityChangeType.Updated)
            .FirstAsync();

        var changes = audit.GetPropertyChanges();

        // Assert
        Assert.NotEmpty(changes);
        Assert.Contains(changes, c => c.PropertyName == "Name" && c.HasChanged);
        Assert.Contains(changes, c => c.PropertyName == "Price" && c.HasChanged);
    }

    [Fact]
    public async Task RetentionPolicy_WithDefaultRetention_SetsArchiveAfter()
    {
        // Arrange
        AuditTrail.DefaultRetentionDays = 30;
        var product = new Product { Name = "Test", Price = 100m };

        // Act
        _context.Products.Add(product);
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audit = await _context.AuditTrails.FirstAsync();
        Assert.NotNull(audit.ArchiveAfter);
        Assert.True(audit.ArchiveAfter.Value > audit.DateTimeUTC);

        // Cleanup
        AuditTrail.DefaultRetentionDays = null;
    }

    [Fact]
    public async Task Archival_PendingArchival_ReturnsEligibleEntries()
    {
        // Arrange
        var oldAudit = new AuditTrail
        {
            TableName = "Test",
            ChangeType = EntityChangeType.Created,
            DateTimeUTC = DateTime.UtcNow.AddDays(-40),
            ArchiveAfter = DateTime.UtcNow.AddDays(-10),
            UserId = _userId
        };

        _context.AuditTrails.Add(oldAudit);
        await _context.SaveChangesAsync();

        // Act
        var pending = await _context.AuditTrails
            .PendingArchival()
            .ToListAsync();

        // Assert
        Assert.Single(pending);
        Assert.True(pending[0].ShouldBeArchived());
    }

    [Fact]
    public async Task Validation_WithValidAudit_Succeeds()
    {
        // Arrange
        var product = new Product { Name = "Test", Price = 100m };
        _context.Products.Add(product);
        await CreateAuditTrailsAndSaveAsync();

        // Act
        var audit = await _context.AuditTrails.FirstAsync();
        var validation = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public async Task Comparison_BetweenAudits_IdentifiesDifferences()
    {
        // Arrange
        var product = new Product { Name = "Version1", Price = 100m };
        _context.Products.Add(product);
        await CreateAuditTrailsAndSaveAsync();

        _context.Entry(product).State = EntityState.Detached;
        
        var updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(updatedProduct);
        
        updatedProduct.Name = "Version2";
        updatedProduct.Price = 200m;
        await CreateAuditTrailsAndSaveAsync();

        // Act
        var audits = await _context.AuditTrails
            .OrderBy(a => a.DateTimeUTC)
            .ToListAsync();

        Assert.True(audits.Count >= 2, $"Expected at least 2 audits but got {audits.Count}");
        
        var comparison = audits[1].CompareTo(audits[0]);

        // Assert
        Assert.True(comparison.HasDifferences);
        Assert.Contains(comparison.Differences, d => d.PropertyName == "Name");
        Assert.Contains(comparison.Differences, d => d.PropertyName == "Price");
    }

    [Fact]
    public async Task MultipleEntities_SaveChanges_CreatesMultipleAudits()
    {
        // Arrange
        var products = new[]
        {
            new Product { Name = "Product1", Price = 100m },
            new Product { Name = "Product2", Price = 200m },
            new Product { Name = "Product3", Price = 300m }
        };

        // Act
        _context.Products.AddRange(products);
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audits = await _context.AuditTrails
            .Where(a => a.TableName == "Product")
            .ToListAsync();

        Assert.Equal(3, audits.Count);
    }

    [Fact]
    public async Task ConcurrentUpdates_CreatesSeparateAudits()
    {
        // Arrange
        var product = new Product { Name = "Test", Price = 100m };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act - First update
        product.Name = "Update1";
        await CreateAuditTrailsAndSaveAsync();

        // Act - Second update
        product.Name = "Update2";
        await CreateAuditTrailsAndSaveAsync();

        // Assert
        var audits = await _context.AuditTrails
            .Where(a => a.ChangeType == EntityChangeType.Updated)
            .OrderBy(a => a.DateTimeUTC)
            .ToListAsync();

        Assert.Equal(2, audits.Count);
        Assert.Contains("Update1", audits[0].NewValues!);
        Assert.Contains("Update2", audits[1].NewValues!);
    }

    private async Task CreateAuditTrailsAndSaveAsync()
    {
        var auditEntries = new List<AuditTrail>();

        foreach (var entry in _context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted)
            {
                var auditEntry = await AuditTrail.CreateAsync(entry, _userId);
                auditEntries.Add(auditEntry);
            }
        }

        _context.AuditTrails.AddRange(auditEntries);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class TestDbContext : DbContext, IAuditTrailDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<AuditTrail> AuditTrails { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAuditTrail();

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId);
    }
}

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public long? CategoryId { get; set; }
    public Category? Category { get; set; }
}

public class Document : BaseEntity, ISoftDelete
{
    public string Title { get; set; } = string.Empty;
    public override bool IsDeleted { get; set; }
}

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    [DoNotAudit]
    public string PasswordHash { get; set; } = string.Empty;
}

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
