using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Auditing.Tests.Extensions;

public class AuditTrailQueryExtensionsTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public void ForEntity_WithValidEntityId_ReturnsMatchingAudits()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { TableName = "TestEntity", KeyValues = "{\"Id\":1}", DateTimeUTC = DateTime.UtcNow };
        var audit2 = new AuditTrail { TableName = "TestEntity", KeyValues = "{\"Id\":2}", DateTimeUTC = DateTime.UtcNow };
        var audit3 = new AuditTrail { TableName = "OtherEntity", KeyValues = "{\"Id\":1}", DateTimeUTC = DateTime.UtcNow };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.ForEntity<TestEntity>(1).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(audit1.Id, result[0].Id);
    }

    [Fact]
    public void ForTable_WithValidTableName_ReturnsMatchingAudits()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { TableName = "Products", DateTimeUTC = DateTime.UtcNow };
        var audit2 = new AuditTrail { TableName = "Products", DateTimeUTC = DateTime.UtcNow };
        var audit3 = new AuditTrail { TableName = "Orders", DateTimeUTC = DateTime.UtcNow };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.ForTable("Products").ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ForTable_WithNullTableName_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateContext();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.AuditTrails.ForTable(null!).ToList());
    }

    [Fact]
    public void InDateRange_WithValidRange_ReturnsMatchingAudits()
    {
        // Arrange
        using var context = CreateContext();
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var audit1 = new AuditTrail { DateTimeUTC = from.AddDays(1), TableName = "Test" };
        var audit2 = new AuditTrail { DateTimeUTC = from.AddDays(3), TableName = "Test" };
        var audit3 = new AuditTrail { DateTimeUTC = from.AddDays(-10), TableName = "Test" };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.InDateRange(from, to).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void InDateRange_WithInvalidRange_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateContext();
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(-7);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.AuditTrails.InDateRange(from, to).ToList());
    }

    [Fact]
    public void ByUser_WithValidUserId_ReturnsMatchingAudits()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { UserId = 1, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit2 = new AuditTrail { UserId = 1, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit3 = new AuditTrail { UserId = 2, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.ByUser(1).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void PendingArchival_ReturnsOnlyEligibleEntries()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { ArchiveAfter = DateTime.UtcNow.AddDays(-1), IsArchived = false, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit2 = new AuditTrail { ArchiveAfter = DateTime.UtcNow.AddDays(1), IsArchived = false, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit3 = new AuditTrail { ArchiveAfter = DateTime.UtcNow.AddDays(-1), IsArchived = true, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit4 = new AuditTrail { ArchiveAfter = null, IsArchived = false, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        context.AuditTrails.AddRange(audit1, audit2, audit3, audit4);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.PendingArchival().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(audit1.Id, result[0].Id);
    }

    [Fact]
    public void OfChangeType_WithValidType_ReturnsMatchingAudits()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { ChangeType = EntityChangeType.Created, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit2 = new AuditTrail { ChangeType = EntityChangeType.Updated, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit3 = new AuditTrail { ChangeType = EntityChangeType.Created, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.OfChangeType(EntityChangeType.Created).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Archived_ReturnsOnlyArchivedEntries()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { IsArchived = true, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit2 = new AuditTrail { IsArchived = false, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit3 = new AuditTrail { IsArchived = true, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.Archived().ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void NotArchived_ReturnsOnlyNonArchivedEntries()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { IsArchived = true, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit2 = new AuditTrail { IsArchived = false, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        var audit3 = new AuditTrail { IsArchived = false, TableName = "Test", DateTimeUTC = DateTime.UtcNow };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.NotArchived().ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Recent_WithDefaultDays_ReturnsRecentEntries()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-3), TableName = "Test" };
        var audit2 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-10), TableName = "Test" };
        var audit3 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-1), TableName = "Test" };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.Recent().ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Recent_WithCustomDays_ReturnsRecentEntries()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-2), TableName = "Test" };
        var audit2 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-5), TableName = "Test" };
        context.AuditTrails.AddRange(audit1, audit2);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.Recent(3).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void Recent_WithInvalidDays_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateContext();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.AuditTrails.Recent(0).ToList());
    }

    [Fact]
    public void OrderByMostRecent_OrdersDescending()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-3), TableName = "Test" };
        var audit2 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-1), TableName = "Test" };
        var audit3 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-2), TableName = "Test" };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.OrderByMostRecent().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result[0].DateTimeUTC >= result[1].DateTimeUTC);
        Assert.True(result[1].DateTimeUTC >= result[2].DateTimeUTC);
    }

    [Fact]
    public void OrderByOldest_OrdersAscending()
    {
        // Arrange
        using var context = CreateContext();
        var audit1 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-1), TableName = "Test" };
        var audit2 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-3), TableName = "Test" };
        var audit3 = new AuditTrail { DateTimeUTC = DateTime.UtcNow.AddDays(-2), TableName = "Test" };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails.OrderByOldest().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result[0].DateTimeUTC <= result[1].DateTimeUTC);
        Assert.True(result[1].DateTimeUTC <= result[2].DateTimeUTC);
    }

    [Fact]
    public void ChainedQueries_WorkCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var userId = 1L;
        var audit1 = new AuditTrail 
        { 
            TableName = "Products", 
            UserId = userId, 
            DateTimeUTC = DateTime.UtcNow.AddDays(-2),
            IsArchived = false
        };
        var audit2 = new AuditTrail 
        { 
            TableName = "Products", 
            UserId = userId, 
            DateTimeUTC = DateTime.UtcNow.AddDays(-10),
            IsArchived = false
        };
        var audit3 = new AuditTrail 
        { 
            TableName = "Orders", 
            UserId = userId, 
            DateTimeUTC = DateTime.UtcNow.AddDays(-2),
            IsArchived = false
        };
        context.AuditTrails.AddRange(audit1, audit2, audit3);
        context.SaveChanges();

        // Act
        var result = context.AuditTrails
            .ForTable("Products")
            .ByUser(userId)
            .NotArchived()
            .Recent(7)
            .OrderByMostRecent()
            .ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(audit1.Id, result[0].Id);
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IAuditTrailDbContext
    {
        public DbSet<AuditTrail> AuditTrails { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureAuditTrail();
            modelBuilder.Entity<TestEntity>();
        }
    }

    private class TestEntity : BaseEntity
    {
        public string? Name { get; set; }
    }
}
