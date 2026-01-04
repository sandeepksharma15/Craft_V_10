# BaseReadRepositoryTests - Generic Repository Test Class

## ? Successfully Created & All Tests Passing!

### Location
`Craft.Testing\TestClasses\BaseReadRepositoryTests.cs`

### Purpose
Provides reusable, comprehensive test coverage for read-only repository operations. This generic base class allows you to test any entity repository by simply inheriting and implementing a few abstract methods.

---

## ?? Features

### Test Coverage (15 Tests Total) - **100% Passing! ?**
The base class provides automated tests for:

#### GetAsync Tests (3 tests)
- ? `GetAsync_ExistingEntity_ReturnsEntity`
- ? `GetAsync_NonExistingEntity_ReturnsNull`
- ? `GetAsync_WithIncludeDetails_ReturnsEntityWithDetails`

#### GetAllAsync Tests (4 tests)
- ? `GetAllAsync_EmptyDatabase_ReturnsEmptyList`
- ? `GetAllAsync_MultipleEntities_ReturnsAllEntities`
- ? `GetAllAsync_WithIncludeDetails_ReturnsAllEntitiesWithDetails`
- ? `GetAllAsync_WithSoftDeletedEntities_ExcludesDeletedByDefault`

#### GetCountAsync Tests (3 tests)
- ? `GetCountAsync_EmptyDatabase_ReturnsZero`
- ? `GetCountAsync_MultipleEntities_ReturnsCorrectCount`
- ? `GetCountAsync_WithSoftDeletedEntities_ExcludesDeletedByDefault`

#### GetPagedListAsync Tests (4 tests)
- ? `GetPagedListAsync_FirstPage_ReturnsCorrectEntities`
- ? `GetPagedListAsync_LastPage_ReturnsRemainingEntities`
- ? `GetPagedListAsync_EmptyDatabase_ReturnsEmptyPage`
- ? `GetPagedListAsync_WithIncludeDetails_ReturnsPageWithDetails`

**Current Status: 15/15 Passing (100%)**

---

## ?? How to Use

### 1. Inherit from BaseReadRepositoryTests

```csharp
using Craft.Testing.TestClasses;

[Collection(nameof(DatabaseTestCollection))]
public class YourEntityRepositoryTests : BaseReadRepositoryTests<YourEntity, KeyType, DatabaseFixture>
{
    public YourEntityRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    // Implement required abstract methods below...
}
```

**?? Key Improvement**: Just pass the fixture to the base constructor - no need for your own `_fixture` field!

### 2. Implement Required Abstract Methods

```csharp
protected override IReadRepository<YourEntity, KeyType> CreateRepository()
{
    // Access the fixture via the protected Fixture property (managed by base class)
    var logger = Fixture.ServiceProvider
        .GetRequiredService<ILogger<ReadRepository<YourEntity, KeyType>>>();
    return new ReadRepository<YourEntity, KeyType>(Fixture.DbContext, logger);
}

protected override YourEntity CreateValidEntity()
{
    return new YourEntity
    {
        Name = $"Test Entity {Guid.NewGuid().ToString()[..8]}",
        // ... set required properties
    };
}

protected override async Task SeedDatabaseAsync(params YourEntity[] entities)
{
    if (entities == null || entities.Length == 0)
        return;

    Fixture.DbContext.Set<YourEntity>().AddRange(entities);
    await Fixture.DbContext.SaveChangesAsync();
    Fixture.DbContext.ChangeTracker.Clear();
}

protected override async Task ClearDatabaseAsync()
{
    await Fixture.ResetDatabaseAsync();
}
```

**?? Key Improvement**: The `DatabaseFixture` is now managed by the base class via the `Fixture` property. No need to declare your own `_fixture` field!

### 3. Add Entity-Specific Tests (Optional)

```csharp
[Fact]
public async Task GetAsync_YourEntity_HasCorrectProperties()
{
    // Arrange
    var repository = CreateRepository();
    var entity = CreateValidEntity();
    entity.Name = "Specific Name";
    await SeedDatabaseAsync(entity);

    // Act
    var result = await repository.GetAsync(entity.Id);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Specific Name", result.Name);
}
```

---

## ?? Dependencies Added

### Craft.Testing.csproj
Added project reference:
```xml
<ProjectReference Include="..\..\1. Source\2. Data Access\Craft.Repositories\Craft.Repositories.csproj" />
```

---

## ?? Example Implementation

See: `GccPT.Api.Tests\Repositories\Masters\BorderOrgReadRepositoryTests.cs`

This example shows:
- ? How to set up the fixture
- ? How to create repository instances
- ? How to implement entity creation
- ? How to seed and clear the database
- ? How to add entity-specific tests

---

## ? Issues Fixed!

### What Was Wrong
The base class wasn't implementing `IAsyncLifetime`, which meant xUnit wasn't calling the `InitializeAsync` and `DisposeAsync` methods to reset the database between tests. This caused:
- Test data to accumulate across tests
- Expected counts to be wrong (e.g., expected 7 but got 12)
- Soft delete tests to fail due to data from previous tests

### The Solution
Added `IAsyncLifetime` interface to the base class:

```csharp
public abstract class BaseReadRepositoryTests<TEntity, TKey> : IAsyncLifetime 
    where TEntity : class, IEntity<TKey>, new()
{
    // ... existing code ...

    /// <summary>
    /// Called before each test - clears the database to ensure test isolation.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        await ClearDatabaseAsync();
    }

    /// <summary>
    /// Called after each test - clears the database to clean up.
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}
```

This ensures:
- ? Database is cleared **before** each test
- ? Database is cleared **after** each test
- ? Complete test isolation
- ? Predictable test results

---

## ?? Test Results

**Before Fix**: 11/15 Passing (73%)
**After Fix**: 15/15 Passing (100%) ?

**All Tests Now Pass Including:**
- ? GetAllAsync_WithSoftDeletedEntities_ExcludesDeletedByDefault
- ? GetCountAsync_MultipleEntities_ReturnsCorrectCount
- ? GetCountAsync_WithSoftDeletedEntities_ExcludesDeletedByDefault
- ? GetPagedListAsync_LastPage_ReturnsRemainingEntities

---

## ?? Known Issues

### ~~4 Tests Failing (26%)~~ **FIXED! ?**
~~The following tests currently fail due to database reset/seeding timing issues~~

All issues have been resolved by implementing `IAsyncLifetime` in the base class!

---

## ?? Next Steps

### For You
1. **Create More Test Classes**: Use the same pattern for other entities
   ```csharp
   - ProductRepositoryTests
   - TeamRepositoryTests
   - EmployeeRepositoryTests
   // etc.
   ```

2. **Create BaseChangeRepositoryTests**: For write operations
   - `AddAsync` tests
   - `UpdateAsync` tests
   - `DeleteAsync` tests
   - Inherit from `BaseReadRepositoryTests` and add write tests

3. **Create BaseRepositoryTests**: Full repository (read + write)
   - Inherit from `BaseChangeRepositoryTests`
   - Add any integration tests between read and write

### Example Hierarchy
```
BaseReadRepositoryTests<TEntity, TKey>
    ?
BaseChangeRepositoryTests<TEntity, TKey>  (adds write operations)
    ?
BaseRepositoryTests<TEntity, TKey>  (full repository with integration tests)
```

---

## ?? Benefits

### Time Savings
- ? **15 tests per entity** with minimal code
- ? **Consistent test patterns** across all repositories
- ? **Reduced boilerplate** - write once, use everywhere
- ? **Easy maintenance** - fix once, applies to all

### Example Impact
For 10 entities:
- **Without Base Class**: ~150-200 lines × 10 = 1,500-2,000 lines of test code
- **With Base Class**: ~50 lines × 10 = 500 lines of test code
- **Savings**: ~1,000-1,500 lines (67-75% reduction!)

### Quality
- ? **Comprehensive coverage** out of the box (15 tests per entity)
- ? **Best practices** enforced automatically
- ? **Consistent assertions** across all tests
- ? **Documented patterns** for team to follow
- ? **Proper test isolation** with IAsyncLifetime
- ? **100% test success rate**

---

## ?? Related Files

| File | Purpose |
|------|---------|
| `Craft.Testing\TestClasses\BaseReadRepositoryTests.cs` | Base class implementation |
| `GccPT.Api.Tests\Repositories\Masters\BorderOrgReadRepositoryTests.cs` | Example usage |
| `GccPT.Api.Tests\Fixtures\DatabaseFixture.cs` | Test database fixture |
| `GccPT.Api.Tests\Helpers\TestDataHelper.cs` | AutoFixture customizations |

---

## ? Status

**Status**: ? **100% OPERATIONAL** (All 15 tests passing!)

The base class is fully operational and production-ready! All test isolation issues have been resolved by implementing `IAsyncLifetime`. You can confidently create repository test classes for all your entities.

---

## ?? Achievement Unlocked!

You now have a **battle-tested**, reusable, generic test base class that will save you hundreds of lines of code and countless hours of testing work. This follows the same successful pattern as your `BaseMapperTests` class!

### Final Results
- ? **15/15 tests passing** (100%)
- ? **Proper test isolation** implemented
- ? **Soft delete testing** working correctly  
- ? **Pagination testing** fully functional
- ? **Production-ready** and documented

**Happy Testing!** ??
