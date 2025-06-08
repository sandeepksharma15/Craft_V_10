using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Craft.Auditing.Tests.Model;

public class AuditTrailTests
{
    [Fact]
    public void CreateAuditEntry_AddedState_SetsAuditTypeToCreate()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);

        // Act
        var auditLog = new AuditTrail(entityEntry, default);

        // Assert
        Assert.Equal(EntityChangeType.Created, auditLog.ChangeType);
    }

    [Fact]
    public void CreateAuditEntry_DeletedState_SetsAuditTypeToDeleteAndEntityStateToDelete()
    {
        // Arrange
        var entityEntry = CreateAnotherEntityEntry(EntityState.Deleted);

        // Act
        var auditLog = new AuditTrail(entityEntry, default);

        // Assert
        Assert.Equal(EntityChangeType.Deleted, auditLog.ChangeType);
        Assert.Equal(EntityState.Deleted, entityEntry.State);
    }

    [Fact]
    public void CreateAuditEntry_DeletedState_SetsAuditTypeToDeleteButEntityStateToModified()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Deleted);

        // Act
        var auditLog = new AuditTrail(entityEntry, default);

        // Assert
        Assert.Equal(EntityChangeType.Deleted, auditLog.ChangeType);
        Assert.Equal(EntityState.Modified, entityEntry.State);
    }

    [Fact]
    public void CreateAuditEntry_ModifiedStateWithModifiedProperties_SetsAuditTypeToUpdate()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Modified, modifiedProperties: true);

        // Act
        var auditLog = new AuditTrail(entityEntry, default);

        // Assert
        Assert.Equal(EntityChangeType.Updated, auditLog.ChangeType);
    }

    [Fact]
    public void CreateAuditEntry_ModifiedStateWithNoModifiedProperties_SetsAuditTypeToUpdated()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Modified, modifiedProperties: false);

        // Act
        var auditLog = new AuditTrail(entityEntry, default);

        // Assert
        Assert.Equal(EntityChangeType.Updated, auditLog.ChangeType);
        Assert.Equal(auditLog.OldValues, auditLog.NewValues);
    }

    [Fact]
    public void CreateAuditEntry_ModifiedStateWithSoftDelete_SetsAuditTypeToDeleted()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Modified, modifiedProperties: true, isSoftDelete: true);

        // Act
        var auditLog = new AuditTrail(entityEntry, default);

        // Assert
        Assert.Equal(EntityChangeType.Deleted, auditLog.ChangeType);
        Assert.Equal(EntityState.Modified, entityEntry.State);
    }

    private class AnotherEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>();
            modelBuilder.Entity<AnotherEntity>();
        }
    }


    private class TestEntity : ISoftDelete
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string? Name { get; set; }
    }

    private static EntityEntry CreateAnotherEntityEntry(EntityState state, bool modifiedProperties = false)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new TestDbContext(options))
        {
            var entity = new AnotherEntity { Id = 1, Name = "Test" };
            context.Entry(entity).State = state;

            if (state == EntityState.Modified && modifiedProperties)
                context.Entry(entity).Property(e => e.Name).IsModified = true;

            return context.Entry(entity);
        }
    }

    private static EntityEntry CreateEntityEntry(EntityState state, bool modifiedProperties = false, bool isSoftDelete = false)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new TestDbContext(options))
        {
            var entity = new TestEntity { Id = 1, Name = "Test", IsDeleted = isSoftDelete };
            context.Entry(entity).State = state;

            if (state == EntityState.Modified && modifiedProperties)
                context.Entry(entity).Property(e => e.Name).IsModified = true;

            return context.Entry(entity);
        }
    }
}
