using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Craft.Auditing.Tests.Model;

public class AuditTrailTests
{
    public AuditTrailTests()
    {
        AuditTrail.ConfigureSerializerOptions(null);
        AuditTrail.DefaultRetentionDays = null;
        AuditTrail.IncludeNavigationProperties = true;
    }

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

    [Fact]
    public void Create_StaticFactory_CreatesAuditTrailCorrectly()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var userId = 123L;

        // Act
        var auditLog = AuditTrail.Create(entityEntry, userId);

        // Assert
        Assert.NotNull(auditLog);
        Assert.Equal(EntityChangeType.Created, auditLog.ChangeType);
        Assert.Equal(userId, auditLog.UserId);
        Assert.NotNull(auditLog.TableName);
    }

    [Fact]
    public void Create_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => AuditTrail.Create(null!, 1));
    }

    [Fact]
    public async Task CreateAsync_StaticFactory_CreatesAuditTrailCorrectly()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var userId = 456L;

        // Act
        var auditLog = await AuditTrail.CreateAsync(entityEntry, userId);

        // Assert
        Assert.NotNull(auditLog);
        Assert.Equal(EntityChangeType.Created, auditLog.ChangeType);
        Assert.Equal(userId, auditLog.UserId);
        Assert.NotNull(auditLog.TableName);
    }

    [Fact]
    public async Task CreateAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => AuditTrail.CreateAsync(null!, 1));
    }

    [Fact]
    public async Task CreateAsync_WithCancellation_CompletesSuccessfully()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var cts = new CancellationTokenSource();

        // Act
        var auditLog = await AuditTrail.CreateAsync(entityEntry, 1, cts.Token);

        // Assert
        Assert.NotNull(auditLog);
    }

    [Fact]
    public void Archive_MarksAuditTrailAsArchived()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var auditLog = new AuditTrail(entityEntry, default);
        Assert.False(auditLog.IsArchived);

        // Act
        auditLog.Archive();

        // Assert
        Assert.True(auditLog.IsArchived);
    }

    [Fact]
    public void Unarchive_RestoresAuditTrailFromArchived()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var auditLog = new AuditTrail(entityEntry, default);
        auditLog.Archive();
        Assert.True(auditLog.IsArchived);

        // Act
        auditLog.Unarchive();

        // Assert
        Assert.False(auditLog.IsArchived);
    }

    [Fact]
    public void ShouldBeArchived_WithArchiveDateInPast_ReturnsTrue()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var auditLog = new AuditTrail(entityEntry, default)
        {
            ArchiveAfter = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        Assert.True(auditLog.ShouldBeArchived());
    }

    [Fact]
    public void ShouldBeArchived_WithArchiveDateInFuture_ReturnsFalse()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var auditLog = new AuditTrail(entityEntry, default)
        {
            ArchiveAfter = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        Assert.False(auditLog.ShouldBeArchived());
    }

    [Fact]
    public void ShouldBeArchived_WithNoArchiveDate_ReturnsFalse()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var auditLog = new AuditTrail(entityEntry, default);

        // Act & Assert
        Assert.False(auditLog.ShouldBeArchived());
    }

    [Fact]
    public void ShouldBeArchived_AlreadyArchived_ReturnsFalse()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var auditLog = new AuditTrail(entityEntry, default)
        {
            ArchiveAfter = DateTime.UtcNow.AddDays(-1),
            IsArchived = true
        };

        // Act & Assert
        Assert.False(auditLog.ShouldBeArchived());
    }

    [Fact]
    public void SetRetentionPolicy_SetsArchiveAfterCorrectly()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var auditLog = new AuditTrail(entityEntry, default);
        var retentionDays = 90;

        // Act
        auditLog.SetRetentionPolicy(retentionDays);

        // Assert
        Assert.NotNull(auditLog.ArchiveAfter);
        Assert.True(auditLog.ArchiveAfter.Value > auditLog.DateTimeUTC);
        var expectedDate = auditLog.DateTimeUTC.AddDays(retentionDays);
        Assert.Equal(expectedDate, auditLog.ArchiveAfter.Value);
    }

    [Fact]
    public void SetRetentionPolicy_WithZeroDays_ThrowsArgumentException()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var auditLog = new AuditTrail(entityEntry, default);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => auditLog.SetRetentionPolicy(0));
    }

    [Fact]
    public void SetRetentionPolicy_WithNegativeDays_ThrowsArgumentException()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var auditLog = new AuditTrail(entityEntry, default);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => auditLog.SetRetentionPolicy(-5));
    }

    [Fact]
    public void DefaultRetentionDays_AppliesAutomaticallyOnCreation()
    {
        // Arrange
        AuditTrail.DefaultRetentionDays = 30;
        var entityEntry = CreateEntityEntry(EntityState.Added);

        // Act
        var auditLog = new AuditTrail(entityEntry, default);

        // Assert
        Assert.NotNull(auditLog.ArchiveAfter);
        var expectedDate = auditLog.DateTimeUTC.AddDays(30);
        Assert.Equal(expectedDate, auditLog.ArchiveAfter.Value);

        // Cleanup
        AuditTrail.DefaultRetentionDays = null;
    }

    [Fact]
    public void DefaultRetentionDays_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        AuditTrail.DefaultRetentionDays = 60;

        // Assert
        Assert.Equal(60, AuditTrail.DefaultRetentionDays);

        // Cleanup
        AuditTrail.DefaultRetentionDays = null;
    }

    [Fact]
    public void DefaultRetentionDays_CanBeSetToNull()
    {
        // Arrange
        AuditTrail.DefaultRetentionDays = 30;

        // Act
        AuditTrail.DefaultRetentionDays = null;

        // Assert
        Assert.Null(AuditTrail.DefaultRetentionDays);
    }

    [Fact]
    public void DefaultRetentionDays_WithZeroValue_ThrowsArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => AuditTrail.DefaultRetentionDays = 0);
    }

    [Fact]
    public void DefaultRetentionDays_WithNegativeValue_ThrowsArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => AuditTrail.DefaultRetentionDays = -10);
    }

    [Fact]
    public void IncludeNavigationProperties_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        AuditTrail.IncludeNavigationProperties = false;

        // Assert
        Assert.False(AuditTrail.IncludeNavigationProperties);

        // Cleanup
        AuditTrail.IncludeNavigationProperties = true;
    }

    [Fact]
    public void IncludeNavigationProperties_DefaultsToTrue()
    {
        // Arrange
        AuditTrail.IncludeNavigationProperties = true;

        // Act & Assert
        Assert.True(AuditTrail.IncludeNavigationProperties);
    }

    [Fact]
    public void ConfigureSerializerOptions_AcceptsCustomOptions()
    {
        // Arrange
        var customOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // Act
        AuditTrail.ConfigureSerializerOptions(customOptions);

        // Assert
        Assert.Same(customOptions, AuditTrail.SerializerOptions);

        // Cleanup
        AuditTrail.ConfigureSerializerOptions(null);
    }

    [Fact]
    public void ConfigureSerializerOptions_WithNull_ResetsToDefault()
    {
        // Arrange
        var customOptions = new JsonSerializerOptions { WriteIndented = true };
        AuditTrail.ConfigureSerializerOptions(customOptions);

        // Act
        AuditTrail.ConfigureSerializerOptions(null);

        // Assert
        Assert.NotSame(customOptions, AuditTrail.SerializerOptions);
        Assert.False(AuditTrail.SerializerOptions.WriteIndented);
    }

    [Fact]
    public void SerializerOptions_ReturnsDefaultWhenNotConfigured()
    {
        // Arrange
        AuditTrail.ConfigureSerializerOptions(null);

        // Act
        var options = AuditTrail.SerializerOptions;

        // Assert
        Assert.NotNull(options);
        Assert.False(options.WriteIndented);
    }

    [Fact]
    public void Constructor_WithEntityEntry_PopulatesProperties()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Added);
        var userId = 789L;

        // Act
        var auditLog = new AuditTrail(entityEntry, userId);

        // Assert
        Assert.Equal(userId, auditLog.UserId);
        Assert.NotNull(auditLog.TableName);
        Assert.True(auditLog.DateTimeUTC <= DateTime.UtcNow);
        Assert.True(auditLog.DateTimeUTC > DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void Constructor_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AuditTrail(null!, 1));
    }

    [Fact]
    public void ParameterlessConstructor_ForEFCore_CreatesInstance()
    {
        // Arrange & Act
        var auditLog = new AuditTrail();

        // Assert
        Assert.NotNull(auditLog);
    }

    [Fact]
    public void AuditTrail_WithDoNotAuditProperty_ExcludesFromAudit()
    {
        // Arrange
        var entityEntry = CreateEntityEntryWithDoNotAudit(EntityState.Added);

        // Act
        var auditLog = new AuditTrail(entityEntry, default);

        // Assert
        Assert.NotNull(auditLog.NewValues);
        var newValuesDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(auditLog.NewValues);
        Assert.NotNull(newValuesDict);
        Assert.DoesNotContain("Password", newValuesDict.Keys);
        Assert.Contains("Name", newValuesDict.Keys);
    }

    [Fact]
    public void AuditTrail_WithEmptyCollections_SerializesAsNull()
    {
        // Arrange
        var entityEntry = CreateEntityEntry(EntityState.Unchanged);

        // Act
        var auditLog = new AuditTrail(entityEntry, default);

        // Assert
        Assert.Null(auditLog.OldValues);
    }

    [Fact]
    public void MaxConstant_Values_AreCorrect()
    {
        // Arrange & Act & Assert
        Assert.Equal(2000, AuditTrail.MaxChangedColumnsLength);
        Assert.Equal(1000, AuditTrail.MaxKeyValuesLength);
        Assert.Equal(255, AuditTrail.MaxTableNameLength);
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
            modelBuilder.Entity<EntityWithDoNotAudit>();
        }
    }

    private class TestEntity : ISoftDelete
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string? Name { get; set; }
    }

    private class EntityWithDoNotAudit
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        [DoNotAudit]
        public string? Password { get; set; }
    }

    private static EntityEntry CreateAnotherEntityEntry(EntityState state, bool modifiedProperties = false)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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

    private static EntityEntry CreateEntityEntryWithDoNotAudit(EntityState state)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using (var context = new TestDbContext(options))
        {
            var entity = new EntityWithDoNotAudit { Id = 1, Name = "Test", Password = "secret" };
            context.Entry(entity).State = state;

            if (state == EntityState.Modified)
            {
                context.Entry(entity).Property(e => e.Name).IsModified = true;
                context.Entry(entity).Property(e => e.Password).IsModified = true;
            }

            return context.Entry(entity);
        }
    }
}
