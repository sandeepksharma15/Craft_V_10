using System.Linq.Expressions;
using Craft.Domain;
using Craft.TestDataStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Craft.Data.Tests.Extensions;

public class QueryFilterExtensionTests
{
    private const string SoftDeleteFilterName = "SoftDelete";

    // 1) Throws if T is not an interface
    [Fact]
    public void AddGlobalQueryFilter_Throws_When_TypeArgument_IsNotInterface()
    {
        // Arrange
        // modelBuilder can be null since the type check happens first

        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueryFilterExtension.AddGlobalQueryFilter<Company>(null!, SoftDeleteFilterName, _ => true));
    }

    // 2) Throws if modelBuilder is null
    [Fact]
    public void AddGlobalQueryFilter_Throws_When_ModelBuilder_IsNull()
    {
        // Arrange
        Expression<Func<ISoftDelete, bool>> filter = e => !e.IsDeleted;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => QueryFilterExtension.AddGlobalQueryFilter<ISoftDelete>(null!, SoftDeleteFilterName, filter));
    }

    // 3) Throws if filterName is null or empty
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddGlobalQueryFilter_Throws_When_FilterName_IsNullOrEmpty(string? name)
    {
        // Arrange
        var builder = new ModelBuilder(new ConventionSet());
        Expression<Func<ISoftDelete, bool>> filter = e => !e.IsDeleted;

        // Act & Assert
        if (name is null)
            Assert.Throws<ArgumentNullException>(() => builder.AddGlobalQueryFilter<ISoftDelete>(name!, filter));
        else
            Assert.Throws<ArgumentException>(() => builder.AddGlobalQueryFilter<ISoftDelete>(name!, filter));
    }

    // 4) Throws if filter is null
    [Fact]
    public void AddGlobalQueryFilter_Throws_When_Filter_IsNull()
    {
        // Arrange
        var builder = new ModelBuilder(new ConventionSet());
        Expression<Func<ISoftDelete, bool>> filter = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddGlobalQueryFilter<ISoftDelete>(SoftDeleteFilterName, filter));
    }

    // 5) Applies filter to all entities implementing ISoftDelete and demonstrates changed behavior
    [Fact]
    public void AddGlobalQueryFilter_Applies_To_All_Entities_Implementing_ISoftDelete_And_Filters_Data()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var optionsWithFilter = new DbContextOptionsBuilder<SoftDeleteDbContext>().UseInMemoryDatabase(dbName).Options;
        var optionsNoFilter = new DbContextOptionsBuilder<NoFilterDbContext>().UseInMemoryDatabase(dbName).Options;

        using (var setup = new NoFilterDbContext(optionsNoFilter))
        {
            setup.Database.EnsureCreated();

            setup.Companies!.Add(new Company { Id = 1, Name = "A", CountryId = 1, IsDeleted = false });
            setup.Companies!.Add(new Company { Id = 2, Name = "B", CountryId = 1, IsDeleted = true });
            setup.Countries!.Add(new Country { Id = 10, Name = "X", IsDeleted = false });
            setup.Countries!.Add(new Country { Id = 11, Name = "Y", IsDeleted = true });
            setup.NoSoftDeleteEntities!.Add(new NoSoftDeleteEntity { Id = 100, Desc = "N1" });
            setup.NoSoftDeleteEntities!.Add(new NoSoftDeleteEntity { Id = 101, Desc = "N2" });

            setup.SaveChanges();
        }

        // Act
        using var noFilter = new NoFilterDbContext(optionsNoFilter);
        using var withFilter = new SoftDeleteDbContext(optionsWithFilter);

        var companiesNoFilter = noFilter.Companies!.AsNoTracking().OrderBy(x => x.Id).ToList();
        var countriesNoFilter = noFilter.Countries!.AsNoTracking().OrderBy(x => x.Id).ToList();
        var nosoftNoFilter = noFilter.NoSoftDeleteEntities!.AsNoTracking().OrderBy(x => x.Id).ToList();

        var companiesWithFilter = withFilter.Companies!.AsNoTracking().OrderBy(x => x.Id).ToList();
        var countriesWithFilter = withFilter.Countries!.AsNoTracking().OrderBy(x => x.Id).ToList();
        var nosoftWithFilter = withFilter.NoSoftDeleteEntities!.AsNoTracking().OrderBy(x => x.Id).ToList();

        // Assert
        // Without filter: all rows
        Assert.Equal(2, companiesNoFilter.Count);
        Assert.Equal(2, countriesNoFilter.Count);
        Assert.Equal(2, nosoftNoFilter.Count);

        // With filter: only non-deleted for ISoftDelete types, unchanged for NoSoftDeleteEntity
        Assert.Single(companiesWithFilter);
        Assert.Equal(1, companiesWithFilter[0].Id);

        Assert.Single(countriesWithFilter);
        Assert.Equal(10, countriesWithFilter[0].Id);

        Assert.Equal(2, nosoftWithFilter.Count);
    }

    // 6) Does not apply duplicate filter when same name used repeatedly
    [Fact]
    public void AddGlobalQueryFilter_DoesNot_Duplicate_When_Same_Name_Used()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SoftDeleteDbContextDuplicateRegistration>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        // Act
        using var ctx = new SoftDeleteDbContextDuplicateRegistration(options);
        ctx.Database.EnsureCreated();

        // Seed one deleted and one active
        ctx.Companies!.Add(new Company { Id = 1, Name = "A", CountryId = 1, IsDeleted = true });
        ctx.Companies!.Add(new Company { Id = 2, Name = "B", CountryId = 1, IsDeleted = false });
        ctx.SaveChanges();

        // Fetch to ensure filter is applied only once (should still return one active)
        var companies = ctx.Companies!.AsNoTracking().OrderBy(x => x.Id).ToList();

        // Also, verify metadata has only one declared filter for the entity
        var entityType = ctx.Model.FindEntityType(typeof(Company))!;
        var filters = entityType.GetDeclaredQueryFilters();

        // Assert
        Assert.Single(companies);
        Assert.Single(filters);
        Assert.Contains(filters, f => f.Key == SoftDeleteFilterName);
    }

    // 7) No-op when no entities implement the interface
    private interface INotImplemented { }

    [Fact]
    public void AddGlobalQueryFilter_NoOp_When_No_Entities_Implement_Interface()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<NoFilterDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        // Act
        using var ctx = new NoFilterDbContext(options);
        ctx.Database.EnsureCreated();

        // Apply a filter for an interface that no entity implements
        ctx.ModelBuilderHook?.Invoke();

        // Seed and query to demonstrate no effect
        ctx.Companies!.Add(new Company { Id = 1, Name = "A", CountryId = 1, IsDeleted = true });
        ctx.Companies!.Add(new Company { Id = 2, Name = "B", CountryId = 1, IsDeleted = false });
        ctx.NoSoftDeleteEntities!.Add(new NoSoftDeleteEntity { Id = 10, Desc = "X" });
        ctx.SaveChanges();

        var companies = ctx.Companies!.AsNoTracking().OrderBy(x => x.Id).ToList();
        var nosoft = ctx.NoSoftDeleteEntities!.AsNoTracking().ToList();

        // Assert
        // No soft delete filter applied, all companies should be visible
        Assert.Equal(2, companies.Count);
        Assert.Single(nosoft);
    }

    // Test DbContexts
    private class NoFilterDbContext : DbContext
    {
        public NoFilterDbContext(DbContextOptions<NoFilterDbContext> options) : base(options) { }

        public DbSet<Company>? Companies { get; set; }
        public DbSet<Country>? Countries { get; set; }
        public DbSet<NoSoftDeleteEntity>? NoSoftDeleteEntities { get; set; }

        // Expose a hook to demonstrate no-op filter registration for a non-implemented interface
        public Action? ModelBuilderHook { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // For the no-op test, register a filter for an interface not implemented by any entity
            ModelBuilderHook = () => modelBuilder.AddGlobalQueryFilter<INotImplemented>("NoOp", _ => true);
        }
    }

    private class SoftDeleteDbContext : DbContext
    {
        public SoftDeleteDbContext(DbContextOptions<SoftDeleteDbContext> options) : base(options) { }

        public DbSet<Company>? Companies { get; set; }
        public DbSet<Country>? Countries { get; set; }
        public DbSet<NoSoftDeleteEntity>? NoSoftDeleteEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply the global query filter to all ISoftDelete entities
            modelBuilder.AddGlobalQueryFilter<ISoftDelete>(SoftDeleteFilterName, e => !e.IsDeleted);
        }
    }

    private class SoftDeleteDbContextDuplicateRegistration : DbContext
    {
        public SoftDeleteDbContextDuplicateRegistration(DbContextOptions<SoftDeleteDbContextDuplicateRegistration> options) : base(options) { }

        public DbSet<Company>? Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Register the same named filter twice; second call should be ignored for each entity
            modelBuilder.AddGlobalQueryFilter<ISoftDelete>(SoftDeleteFilterName, e => !e.IsDeleted);
            modelBuilder.AddGlobalQueryFilter<ISoftDelete>(SoftDeleteFilterName, e => !e.IsDeleted);
        }
    }
}
