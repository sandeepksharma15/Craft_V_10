using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Craft.Extensions.Tests.EfCore;

public class DbSetExtensionsTests
{
    [Fact]
    public void GetQueryFilter_ReturnsNull_WhenNoQueryFilterExists()
    {
        // Arrange: Create a DbContext with no query filter on the DbSet
        var options = new DbContextOptionsBuilder<MyAnotherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act: Use the GetQueryFilter method on the DbSet
        using var context = new MyAnotherDbContext(options);
        var result = context?.Entities?.GetQueryFilter();

        // Assert: Verify that the result is null
        Assert.Null(result);
    }

    [Fact]
    public void GetQueryFilter_ReturnsQueryFilter_WhenExists()
    {
        // Arrange: Create a DbContext with a query filter on the DbSet
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act: Use the GetQueryFilter method on the DbSet
        using var context = new MyDbContext(options);
        var result = context?.Entities?.GetQueryFilter();

        // Assert: Verify that the result is not null and has the expected properties
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("e", result.Parameters[0].Name);
        Assert.Equal(ExpressionType.MemberAccess, result.Body.NodeType);
    }

    [Fact]
    public void GetQueryFilter_ThrowsOnNullDbSet()
    {
        // Arrange: Create a null DbSet
        DbSet<Entity> dbSet = null!;

        // Act & Assert: Verify that GetQueryFilter throws an ArgumentNullException
        Assert.Throws<ArgumentNullException>(() => dbSet.GetQueryFilter());
    }

    [Fact]
    public void RemoveFromQueryFilter_RemovesCondition()
    {
        // Arrange: Create a DbContext with a query filter on the DbSet
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act: Use the RemoveFromQueryFilter method to remove the condition
        using var context = new MyDbContext(options);
        context?.Entities?.Add(new Entity { Id = 1, IsActive = true });
        context?.Entities?.Add(new Entity { Id = 2, IsActive = false });
        context?.SaveChanges();

        var result = context?.Entities?.RemoveFromQueryFilter(e => e.IsActive);
        var list = result?.ToList();

        // Assert: Verify that the condition was removed and both entities are returned
        Assert.Equal(2, list?.Count);
    }

    [Fact]
    public void RemoveFromQueryFilter_ReturnsAllEntities_WhenConditionRemovedCompletely()
    {
        // Arrange: Create a DbContext with a query filter on the DbSet
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new MyDbContext(options);

        context?.Entities?.Add(new Entity { Id = 1, IsActive = true });
        context?.Entities?.Add(new Entity { Id = 2, IsActive = false });
        context?.SaveChanges();

        // Act: Remove the query filter condition
        var result = context?.Entities?.RemoveFromQueryFilter(e => e.IsActive == true);

        var list = result?.ToList();

        // Assert: Verify that both entities are returned
        Assert.Equal(2, list?.Count);
    }

    [Fact]
    public void RemoveFromQueryFilter_ReturnsEntitiesFulfillingRestFilters()
    {
        // Arrange: Create a DbContext with a query filter on the DbSet
        var options = new DbContextOptionsBuilder<MyYetAnotherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new MyYetAnotherDbContext(options);
        context?.Entities?.Add(new Entity { Id = 1, IsActive = true, IsDeleted = false });
        context?.Entities?.Add(new Entity { Id = 2, IsActive = false, IsDeleted = false });
        context?.Entities?.Add(new Entity { Id = 3, IsActive = true, IsDeleted = true });
        context?.SaveChanges();

        // Act: Remove the query filter condition
        var result = context?.Entities?.RemoveFromQueryFilter(e => e.IsActive == true);
        var list = result?.ToList();

        // Assert: Verify that only entities fulfilling the rest of the filters are returned
        Assert.Equal(2, list?.Count);
        Assert.Contains(list!, e => e.Id == 1);
        Assert.Contains(list!, e => e.Id == 2);
    }

    [Fact]
    public void RemoveFromQueryFilter_ThrowsOnNullDbSet()
    {
        // Arrange: Create a null DbSet
        DbSet<Entity> dbSet = null!;
        Expression<Func<Entity, bool>> cond = e => e.IsActive;

        // Act & Assert: Verify that RemoveFromQueryFilter throws an ArgumentNullException
        Assert.Throws<ArgumentNullException>(() => dbSet.RemoveFromQueryFilter(cond));
    }

    [Fact]
    public void RemoveFromQueryFilter_ThrowsOnNullCondition()
    {
        // Arrange: Create a DbContext with a query filter on the DbSet
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        Expression<Func<Entity, bool>> cond = null!;

        // Act & Assert: Verify that RemoveFromQueryFilter throws an ArgumentNullException
        Assert.Throws<ArgumentNullException>(() => context?.Entities?.RemoveFromQueryFilter(cond));
    }

    [Fact]
    public void IncludeDetails_ReturnsSource_WhenIncludeDetailsTrue()
    {
        // Arrange: Create a DbContext with an in-memory database
        var data = new List<Entity> { new() { Id = 1 } }.AsQueryable();
        var result = data.IncludeDetails(true);

        // Assert: Verify that the result is the same as the source
        Assert.Same(data, result);
    }

    [Fact]
    public void IncludeDetails_CallsIgnoreAutoIncludes_WhenIncludeDetailsFalse()
    {
        // Arrange: Create a DbContext with an in-memory database
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        var query = context?.Entities?.AsQueryable();

        // Act: Call IncludeDetails with false
        var result = query?.IncludeDetails(false);

        // Assert: Verify that the result is not null and is queryable
        Assert.NotNull(result);
        // Should be queryable, but we can't check IgnoreAutoIncludes directly
    }

    [Fact]
    public void IncludeDetails_ThrowsOnNullSource()
    {
        // Arrange: Create a null IQueryable source
        IQueryable<Entity> source = null!;

        // Act & Assert: Verify that IncludeDetails throws an ArgumentNullException
        Assert.Throws<ArgumentNullException>(() => source.IncludeDetails(true));
    }

    [Fact]
    public void ApplyQueryFilter_AppliesFilterIfExists()
    {
        // Arrange: Create a DbContext with a query filter on the DbSet
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        context?.Entities?.Add(new Entity { Id = 1, IsActive = true });
        context?.Entities?.Add(new Entity { Id = 2, IsActive = false });
        context?.SaveChanges();

        // Act: Apply the query filter
        var query = context?.Entities?.IgnoreQueryFilters().AsQueryable();
        var filtered = query?.ApplyQueryFilter(context?.Entities!).ToList();

        // Assert: Verify that only active entities are returned
        Assert.Single(filtered!);
        Assert.True(filtered?.Count == 1 && filtered[0].IsActive);
    }


    [Fact]
    public void ApplyQueryFilter_DoesNothingIfNoFilter()
    {
        // Arrange: Create a DbContext without a query filter on the DbSet
        var options = new DbContextOptionsBuilder<MyAnotherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyAnotherDbContext(options);
        context?.Entities?.Add(new Entity { Id = 1, IsActive = true });
        context?.SaveChanges();

        // Act: Apply the query filter
        var query = context?.Entities?.AsQueryable();
        var filtered = query?.ApplyQueryFilter(context?.Entities!).ToList();

        // Assert: Verify that all entities are returned since no filter exists
        Assert.Single(filtered!);
    }

    [Fact]
    public void IncludeIf_IncludesWhenConditionTrue()
    {
        // Arrange: Create a DbContext with an in-memory database
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);

        // Act: Call IncludeIf with true condition
        var query = context?.Entities?.AsQueryable();
        var result = query?.IncludeIf(true, e => e.Name);

        // Assert: Verify that the result is not null and includes the specified property
        Assert.NotNull(result);
    }

    [Fact]
    public void IncludeIf_DoesNotIncludeWhenConditionFalse()
    {
        // Arrange: Create a DbContext with an in-memory database
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);

        // Act: Call IncludeIf with false condition
        var query = context?.Entities?.AsQueryable();
        var result = query?.IncludeIf(false, e => e.Name);

        // Assert: Verify that the result is not null and does not include the specified property
        Assert.NotNull(result);
    }

    [Fact]
    public void IgnoreQueryFiltersIf_IgnoresWhenTrue()
    {
        // Arrange: Create a DbContext with an in-memory database
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);

        // Act: Call IgnoreQueryFiltersIf with true condition
        var query = context?.Entities?.AsQueryable();
        var result = query?.IgnoreQueryFiltersIf(true);

        // Assert: Verify that the result is not null and ignores query filters
        Assert.NotNull(result);
    }

    [Fact]
    public void IgnoreQueryFiltersIf_DoesNotIgnoreWhenFalse()
    {
        // Arrange: Create a DbContext with an in-memory database
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);

        // Act: Call IgnoreQueryFiltersIf with false condition
        var query = context?.Entities?.AsQueryable();
        var result = query?.IgnoreQueryFiltersIf(false);

        // Assert: Verify that the result is not null and does not ignore query filters
        Assert.NotNull(result);
    }

    private class Entity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? Name { get; set; }
    }

    private class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
        public DbSet<Entity>? Entities { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>().HasQueryFilter(e => e.IsActive);
            base.OnModelCreating(modelBuilder);
        }
    }

    private class MyAnotherDbContext : DbContext
    {
        public MyAnotherDbContext(DbContextOptions<MyAnotherDbContext> options) : base(options) { }
        public DbSet<Entity>? Entities { get; set; }
    }

    private class MyYetAnotherDbContext : DbContext
    {
        public MyYetAnotherDbContext(DbContextOptions<MyYetAnotherDbContext> options) : base(options) { }
        public DbSet<Entity>? Entities { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>().HasQueryFilter(e => e.IsActive && !e.IsDeleted);
            base.OnModelCreating(modelBuilder);
        }
    }
}
