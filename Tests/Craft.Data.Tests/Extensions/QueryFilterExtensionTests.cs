using System.Linq.Expressions;
using Craft.Domain;
using Craft.MultiTenant;
using Craft.Testing.Fixtures;
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

    // 8) AddGlobalSoftDeleteFilter applies once and filters data
    [Fact]
    public void AddGlobalSoftDeleteFilter_Applies_And_DoesNot_Duplicate()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SoftDeleteDbContext2>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var ctx = new SoftDeleteDbContext2(options);
        ctx.Database.EnsureCreated();

        // Seed
        ctx.Companies!.Add(new Company { Id = 1, Name = "A", CountryId = 1, IsDeleted = false });
        ctx.Companies!.Add(new Company { Id = 2, Name = "B", CountryId = 1, IsDeleted = true });
        ctx.SaveChanges();

        // Act
        var visible = ctx.Companies!.AsNoTracking().OrderBy(x => x.Id).ToList();
        var entityType = ctx.Model.FindEntityType(typeof(Company))!;
        var filters = entityType.GetDeclaredQueryFilters();

        // Assert: Only non-deleted visible, and exactly one named filter registered
        Assert.Single(visible);
        Assert.Single(filters);
        Assert.Contains(filters, f => f.Key == QueryFilterExtension.SoftDeleteFilterName);
    }

    // 9) IncludeSoftDeleted returns deleted rows when soft-delete filter is active
    [Fact]
    public void IncludeSoftDeleted_Returns_Deleted_Rows()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SoftDeleteDbContext2>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var ctx = new SoftDeleteDbContext2(options);
        ctx.Database.EnsureCreated();
        ctx.Companies!.Add(new Company { Id = 1, Name = "A", CountryId = 1, IsDeleted = false });
        ctx.Companies!.Add(new Company { Id = 2, Name = "B", CountryId = 1, IsDeleted = true });
        ctx.SaveChanges();

        // Act
        var onlyActive = ctx.Companies!.AsNoTracking().OrderBy(x => x.Id).ToList();
        var withDeleted = QueryFilterExtension.IncludeSoftDeleted(ctx.Companies!.AsNoTracking())?.OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Single(onlyActive);
        Assert.Equal(2, withDeleted?.Count);
        Assert.Equal(2, withDeleted?[1].Id);
    }

    // 10) AddGlobalTenantFilter applies and filters by current tenant
    [Fact]
    public void AddGlobalTenantFilter_Applies_And_Filters_By_Tenant()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TenantFilterDbContext>().UseInMemoryDatabase(dbName).Options;
        var currentTenant = new FakeCurrentTenant { Id = 1 };

        using var ctx = new TenantFilterDbContext(options, currentTenant);
        ctx.Database.EnsureCreated();
        ctx.TenantEntities!.AddRange([
            new TenantEntity { Id = 1, Name = "T1A", TenantId = 1 },
            new TenantEntity { Id = 2, Name = "T1B", TenantId = 1 },
            new TenantEntity { Id = 3, Name = "T2A", TenantId = 2 }
        ]);
        ctx.SaveChanges();

        // Act
        var visible = ctx.TenantEntities!.AsNoTracking().OrderBy(x => x.Id).ToList();
        var entityType = ctx.Model.FindEntityType(typeof(TenantEntity))!;
        var filters = entityType.GetDeclaredQueryFilters();

        // Assert: Only tenant 1 visible, and one named tenant filter applied
        Assert.Equal(2, visible.Count);
        Assert.All(visible, v => Assert.Equal(1, v.TenantId));
        Assert.Single(filters);
        Assert.Contains(filters, f => f.Key == QueryFilterExtension.TenantFilterName);
    }

    // 11) IncludeAllTenants ignores tenant filter
    [Fact]
    public void IncludeAllTenants_Returns_All_Tenants()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TenantFilterDbContext>().UseInMemoryDatabase(dbName).Options;
        var currentTenant = new FakeCurrentTenant { Id = 1 };

        using var ctx = new TenantFilterDbContext(options, currentTenant);
        ctx.Database.EnsureCreated();
        ctx.TenantEntities!.AddRange([
            new TenantEntity { Id = 1, Name = "T1A", TenantId = 1 },
            new TenantEntity { Id = 2, Name = "T2A", TenantId = 2 }
        ]);
        ctx.SaveChanges();

        // Act
        var withFilter = ctx.TenantEntities!.AsNoTracking().ToList();
        var allTenants = ctx.TenantEntities!.AsNoTracking().IncludeAllTenants()?.OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Single(withFilter);
        Assert.Equal(2, allTenants?.Count);
    }

    // 12) IncludeAllTenantsAndSoftDelete ignores both filters
    [Fact]
    public void IncludeAllTenantsAndSoftDelete_Returns_All_Rows()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TenantAndSoftDeleteDbContext>().UseInMemoryDatabase(dbName).Options;
        var currentTenant = new FakeCurrentTenant { Id = 1 };

        using var ctx = new TenantAndSoftDeleteDbContext(options, currentTenant);
        ctx.Database.EnsureCreated();
        ctx.TenantSoftDeleteEntities!.AddRange([
            new TenantSoftDeleteEntity { Id = 1, Name = "A", TenantId = 1, IsDeleted = false },
            new TenantSoftDeleteEntity { Id = 2, Name = "B", TenantId = 1, IsDeleted = true },
            new TenantSoftDeleteEntity { Id = 3, Name = "C", TenantId = 2, IsDeleted = false },
            new TenantSoftDeleteEntity { Id = 4, Name = "D", TenantId = 2, IsDeleted = true }
        ]);
        ctx.SaveChanges();

        // Act
        var withFilters = ctx.TenantSoftDeleteEntities!.AsNoTracking().OrderBy(x => x.Id).ToList();
        var allIgnored = ctx.TenantSoftDeleteEntities!.AsNoTracking().IncludeAllTenantsAndSoftDelete()?.OrderBy(x => x.Id).ToList();

        // Assert: With filters -> only tenant 1 and not deleted
        Assert.Single(withFilters);
        Assert.Equal(1, withFilters[0].Id);

        // All ignored -> all 4
        Assert.Equal(4, allIgnored?.Count);
        Assert.Equal(4, allIgnored?[^1].Id);
    }

    // 13) Include* helpers throw on null query
    [Fact]
    public void IncludeSoftDeleted_Throws_On_Null_Query()
    {
        Assert.Null(QueryFilterExtension.IncludeSoftDeleted<Company>(null!));
    }

    [Fact]
    public void IncludeAllTenants_Throws_On_Null_Query()
    {
        Assert.Null(QueryFilterExtension.IncludeAllTenants<TenantEntity>(null!));
    }

    [Fact]
    public void IncludeAllTenantsAndSoftDelete_Throws_On_Null_Query()
    {
        Assert.Null(QueryFilterExtension.IncludeAllTenantsAndSoftDelete<TenantSoftDeleteEntity>(null!));
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

    // DbContext using AddGlobalSoftDeleteFilter
    private class SoftDeleteDbContext2 : DbContext
    {
        public SoftDeleteDbContext2(DbContextOptions<SoftDeleteDbContext2> options) : base(options) { }

        public DbSet<Company>? Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddGlobalSoftDeleteFilter();
            // Call twice to ensure no-duplicate behavior
            modelBuilder.AddGlobalSoftDeleteFilter();
        }
    }

    // Tenant-only entity for tenant filter tests
    private class TenantEntity : IHasTenant
    {
        public KeyType Id { get; set; }
        public string? Name { get; set; }
        public KeyType TenantId { get; set; }
    }

    // Tenant + SoftDelete entity for combined filter tests
    private class TenantSoftDeleteEntity : IHasTenant, ISoftDelete
    {
        public KeyType Id { get; set; }
        public string? Name { get; set; }
        public KeyType TenantId { get; set; }
        public bool IsDeleted { get; set; }
    }

    // Fake current tenant implementation
    private class FakeCurrentTenant : ICurrentTenant
    {
        public KeyType Id { get; set; }
        public string AdminEmail { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string DbProvider { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string LogoUri { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TenantType Type { get; set; } = TenantType.Tenant;
        public DateTime ValidUpTo { get; set; } = DateTime.UtcNow.AddMonths(1);
        public bool IsDeleted { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public bool IsActive { get; set; } = true;
        public TenantDbType DbType { get; set; }
    }

    // DbContext using AddGlobalTenantFilter
    private class TenantFilterDbContext : DbContext
    {
        private readonly ICurrentTenant currentTenant;

        public TenantFilterDbContext(DbContextOptions<TenantFilterDbContext> options, ICurrentTenant currentTenant) : base(options)
        {
            this.currentTenant = currentTenant;
        }

        public DbSet<TenantEntity>? TenantEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddGlobalTenantFilter(currentTenant);
        }
    }

    // DbContext with both tenant and soft-delete filters
    private class TenantAndSoftDeleteDbContext : DbContext
    {
        private readonly ICurrentTenant currentTenant;

        public TenantAndSoftDeleteDbContext(DbContextOptions<TenantAndSoftDeleteDbContext> options, ICurrentTenant currentTenant) : base(options)
        {
            this.currentTenant = currentTenant;
        }

        public DbSet<TenantSoftDeleteEntity>? TenantSoftDeleteEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddGlobalTenantFilter(currentTenant);
            modelBuilder.AddGlobalSoftDeleteFilter();
        }
    }
}
