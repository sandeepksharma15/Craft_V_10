using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Craft.Extensions.Tests.EfCore;

public class DbSetExtensionsTests
{
    [Fact]
    public void GetQueryFilter_ReturnsNull_WhenNoQueryFilterExists()
    {
        var options = new DbContextOptionsBuilder<MyAnotherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new MyAnotherDbContext(options);
        var result = context?.Entities?.GetQueryFilter();

        Assert.Null(result);
    }

    [Fact]
    public void GetQueryFilter_ReturnsQueryFilter_WhenExists()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new MyDbContext(options);
        var result = context?.Entities?.GetQueryFilter();

        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("e", result.Parameters[0].Name);
        Assert.Equal(ExpressionType.MemberAccess, result.Body.NodeType);
    }

    [Fact]
    public void GetQueryFilter_ThrowsOnNullDbSet()
    {
        DbSet<Entity> dbSet = null!;

        Assert.Throws<ArgumentNullException>(() => dbSet.GetQueryFilter());
    }

    [Fact]
    public void RemoveFromQueryFilter_RemovesCondition()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new MyDbContext(options);
        context?.Entities?.Add(new Entity { Id = 1, IsActive = true });
        context?.Entities?.Add(new Entity { Id = 2, IsActive = false });
        context?.SaveChanges();

        var result = context?.Entities?.RemoveFromQueryFilter(e => e.IsActive);
        var list = result?.ToList();

        Assert.Equal(2, list?.Count);
    }

    [Fact]
    public void RemoveFromQueryFilter_ReturnsAllEntities_WhenConditionRemovedCompletely()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new MyDbContext(options);

        context?.Entities?.Add(new Entity { Id = 1, IsActive = true });
        context?.Entities?.Add(new Entity { Id = 2, IsActive = false });
        context?.SaveChanges();

        var result = context?.Entities?.RemoveFromQueryFilter(e => e.IsActive == true);

        var list = result?.ToList();

        Assert.Equal(2, list?.Count);
    }

    [Fact]
    public void RemoveFromQueryFilter_ReturnsEntitiesFulfillingRestFilters()
    {
        var options = new DbContextOptionsBuilder<MyYetAnotherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new MyYetAnotherDbContext(options);
        context?.Entities?.Add(new Entity { Id = 1, IsActive = true, IsDeleted = false });
        context?.Entities?.Add(new Entity { Id = 2, IsActive = false, IsDeleted = false });
        context?.Entities?.Add(new Entity { Id = 3, IsActive = true, IsDeleted = true });
        context?.SaveChanges();

        var result = context?.Entities?.RemoveFromQueryFilter(e => e.IsActive == true);
        var list = result?.ToList();

        Assert.Equal(2, list?.Count);
        Assert.Contains(list!, e => e.Id == 1);
        Assert.Contains(list!, e => e.Id == 2);
    }

    [Fact]
    public void RemoveFromQueryFilter_ThrowsOnNullDbSet()
    {
        DbSet<Entity> dbSet = null!;
        Expression<Func<Entity, bool>> cond = e => e.IsActive;
        Assert.Throws<ArgumentNullException>(() => dbSet.RemoveFromQueryFilter(cond));
    }

    [Fact]
    public void RemoveFromQueryFilter_ThrowsOnNullCondition()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        Expression<Func<Entity, bool>> cond = null!;
        Assert.Throws<ArgumentNullException>(() => context?.Entities?.RemoveFromQueryFilter(cond));
    }

    [Fact]
    public void IncludeDetails_ReturnsSource_WhenIncludeDetailsTrue()
    {
        var data = new List<Entity> { new() { Id = 1 } }.AsQueryable();
        var result = data.IncludeDetails(true);
        Assert.Same(data, result);
    }

    [Fact]
    public void IncludeDetails_CallsIgnoreAutoIncludes_WhenIncludeDetailsFalse()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        var query = context?.Entities?.AsQueryable();
        var result = query?.IncludeDetails(false);
        Assert.NotNull(result);
        // Should be queryable, but we can't check IgnoreAutoIncludes directly
    }

    [Fact]
    public void IncludeDetails_ThrowsOnNullSource()
    {
        IQueryable<Entity> source = null!;
        Assert.Throws<ArgumentNullException>(() => source.IncludeDetails(true));
    }

    [Fact]
    public void ApplyQueryFilter_AppliesFilterIfExists()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        context?.Entities?.Add(new Entity { Id = 1, IsActive = true });
        context?.Entities?.Add(new Entity { Id = 2, IsActive = false });
        context?.SaveChanges();

        var query = context?.Entities?.IgnoreQueryFilters().AsQueryable();
        var filtered = query?.ApplyQueryFilter(context?.Entities!).ToList();
        Assert.Single(filtered!);
        Assert.True(filtered?.Count == 1 && filtered[0].IsActive);
    }


    [Fact]
    public void ApplyQueryFilter_DoesNothingIfNoFilter()
    {
        var options = new DbContextOptionsBuilder<MyAnotherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyAnotherDbContext(options);
        context?.Entities?.Add(new Entity { Id = 1, IsActive = true });
        context?.SaveChanges();

        var query = context?.Entities?.AsQueryable();
        var filtered = query?.ApplyQueryFilter(context?.Entities!).ToList();
        Assert.Single(filtered!);
    }

    [Fact]
    public void IncludeIf_IncludesWhenConditionTrue()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        var query = context?.Entities?.AsQueryable();
        var result = query?.IncludeIf(true, e => e.Name);
        Assert.NotNull(result);
    }

    [Fact]
    public void IncludeIf_DoesNotIncludeWhenConditionFalse()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        var query = context?.Entities?.AsQueryable();
        var result = query?.IncludeIf(false, e => e.Name);
        Assert.NotNull(result);
    }

    [Fact]
    public void IgnoreQueryFiltersIf_IgnoresWhenTrue()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        var query = context?.Entities?.AsQueryable();
        var result = query?.IgnoreQueryFiltersIf(true);
        Assert.NotNull(result);
    }

    [Fact]
    public void IgnoreQueryFiltersIf_DoesNotIgnoreWhenFalse()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MyDbContext(options);
        var query = context?.Entities?.AsQueryable();
        var result = query?.IgnoreQueryFiltersIf(false);
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
