using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Auditing.Tests.Contracts;

public class ContractTest
{
    [Fact]
    public void Properties_GetSet_WorkAsExpected()
    {
        var audit = new TestAuditTrail();
        var now = DateTime.UtcNow;
        audit.ChangedColumns = "Col1,Col2";
        audit.ChangeType = EntityChangeType.Updated;
        audit.DateTimeUTC = now;
        audit.KeyValues = "id=1";
        audit.NewValues = "{\"Col1\":2}";
        audit.OldValues = "{\"Col1\":1}";
        audit.ShowDetails = true;
        audit.TableName = "TestTable";

        Assert.Equal("Col1,Col2", audit.ChangedColumns);
        Assert.Equal(EntityChangeType.Updated, audit.ChangeType);
        Assert.Equal(now, audit.DateTimeUTC);
        Assert.Equal("id=1", audit.KeyValues);
        Assert.Equal("{\"Col1\":2}", audit.NewValues);
        Assert.Equal("{\"Col1\":1}", audit.OldValues);
        Assert.True(audit.ShowDetails);
        Assert.Equal("TestTable", audit.TableName);
    }

    [Fact]
    public void SoftDelete_Contract_Works()
    {
        var audit = new TestAuditTrail();
        Assert.False(audit.IsDeleted);
        audit.Delete();
        Assert.True(audit.IsDeleted);
        audit.Restore();
        Assert.False(audit.IsDeleted);
    }

    [Fact]
    public void HasUser_Contract_Works()
    {
        var audit = new TestAuditTrail();
        Assert.False(audit.IsUserIdSet());
        Assert.Equal(default, audit.GetUserId());

        var userId = 1;
        audit.SetUserId(userId);

        Assert.True(audit.IsUserIdSet());
        Assert.Equal(userId, audit.GetUserId());
    }

    [Fact]
    public void AuditTrails_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var context = new TestAuditTrailContext();
        var entity = new TestHasAuditTrail { AuditTrails = context.AuditTrails };

        // Act & Assert
        Assert.NotNull(entity.AuditTrails);
        Assert.Same(context.AuditTrails, entity.AuditTrails);
    }

    [Fact]
    public void AuditTrails_CanAddAndRetrieveEntities()
    {
        // Arrange
        var context = new TestAuditTrailContext();
        var entity = new TestHasAuditTrail { AuditTrails = context.AuditTrails };

        var auditTrail = new AuditTrail { Id = 1, TableName="Test" };

        // Act
        entity.AuditTrails.Add(auditTrail);
        context.SaveChanges();

        // Assert
        var retrieved = entity.AuditTrails.FirstOrDefault(a => a.Id == 1);
        Assert.NotNull(retrieved);
        Assert.Equal("Test", retrieved.TableName);
    }

    // Minimal DbContext for testing
    private class TestAuditTrailContext : DbContext, IHasAuditTrail
    {
        public TestAuditTrailContext()
            : base(new DbContextOptionsBuilder<TestAuditTrailContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options)
        { }

        public DbSet<AuditTrail> AuditTrails { get; set; } = null!;
    }

    // Test implementation of IHasAuditTrail
    private class TestHasAuditTrail : IHasAuditTrail
    {
        public DbSet<AuditTrail> AuditTrails { get; set; } = null!;
    }

    private class TestAuditTrail : IAuditTrail
    {
        // IAuditTrail properties
        public string? ChangedColumns { get; set; }
        public EntityChangeType ChangeType { get; set; }
        public DateTime DateTimeUTC { get; set; }
        public string? KeyValues { get; set; }
        public string? NewValues { get; set; }
        public string? OldValues { get; set; }
        public bool ShowDetails { get; set; }
        public string? TableName { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; }
        public void Delete() => IsDeleted = true;
        public void Restore() => IsDeleted = false;

        // IHasUser implementation
        public KeyType UserId { get; set; }
        public KeyType GetUserId() => UserId;
        public bool IsUserIdSet() => UserId != default;
        public void SetUserId(KeyType userId) => UserId = userId;
    }
}
