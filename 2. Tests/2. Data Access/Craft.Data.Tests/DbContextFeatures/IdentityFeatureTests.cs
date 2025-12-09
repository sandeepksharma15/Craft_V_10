using Craft.Data.DbContextFeatures;
using Craft.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.Tests.DbContextFeatures;

public class IdentityFeatureTests
{
    // Create different context types for each test to avoid model caching issues
    private class TestDbContextDefault : DbContext
    {
        public TestDbContextDefault(DbContextOptions<TestDbContextDefault> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure primary keys for Identity entities first
            modelBuilder.Entity<CraftUser>(b => b.HasKey(u => u.Id));
            modelBuilder.Entity<CraftRole>(b => b.HasKey(r => r.Id));
            modelBuilder.Entity<IdentityUserRole<KeyType>>(b => b.HasKey(ur => new { ur.UserId, ur.RoleId }));
            modelBuilder.Entity<IdentityUserClaim<KeyType>>(b => b.HasKey(uc => uc.Id));
            modelBuilder.Entity<IdentityUserLogin<KeyType>>(b => b.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey }));
            modelBuilder.Entity<IdentityUserToken<KeyType>>(b => b.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name }));
            modelBuilder.Entity<IdentityRoleClaim<KeyType>>(b => b.HasKey(rc => rc.Id));
            modelBuilder.Entity<LoginHistory<KeyType>>(b => b.HasKey(lh => lh.Id));
            modelBuilder.Entity<RefreshToken<KeyType>>(b => b.HasKey(rt => rt.Id));
            
            // Then apply the feature configuration
            var feature = new IdentityFeature("Id_");
            feature.ConfigureModel(modelBuilder);
        }
    }

    private class TestDbContextCustom : DbContext
    {
        public TestDbContextCustom(DbContextOptions<TestDbContextCustom> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure primary keys for Identity entities first
            modelBuilder.Entity<CraftUser>(b => b.HasKey(u => u.Id));
            modelBuilder.Entity<CraftRole>(b => b.HasKey(r => r.Id));
            modelBuilder.Entity<IdentityUserRole<KeyType>>(b => b.HasKey(ur => new { ur.UserId, ur.RoleId }));
            modelBuilder.Entity<IdentityUserClaim<KeyType>>(b => b.HasKey(uc => uc.Id));
            modelBuilder.Entity<IdentityUserLogin<KeyType>>(b => b.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey }));
            modelBuilder.Entity<IdentityUserToken<KeyType>>(b => b.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name }));
            modelBuilder.Entity<IdentityRoleClaim<KeyType>>(b => b.HasKey(rc => rc.Id));
            modelBuilder.Entity<LoginHistory<KeyType>>(b => b.HasKey(lh => lh.Id));
            modelBuilder.Entity<RefreshToken<KeyType>>(b => b.HasKey(rt => rt.Id));
            
            // Then apply the feature configuration
            var feature = new IdentityFeature("Custom_");
            feature.ConfigureModel(modelBuilder);
        }
    }

    private class TestDbContextTest : DbContext
    {
        public TestDbContextTest(DbContextOptions<TestDbContextTest> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure primary keys for Identity entities first
            modelBuilder.Entity<CraftUser>(b => b.HasKey(u => u.Id));
            modelBuilder.Entity<CraftRole>(b => b.HasKey(r => r.Id));
            modelBuilder.Entity<IdentityUserRole<KeyType>>(b => b.HasKey(ur => new { ur.UserId, ur.RoleId }));
            modelBuilder.Entity<IdentityUserClaim<KeyType>>(b => b.HasKey(uc => uc.Id));
            modelBuilder.Entity<IdentityUserLogin<KeyType>>(b => b.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey }));
            modelBuilder.Entity<IdentityUserToken<KeyType>>(b => b.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name }));
            modelBuilder.Entity<IdentityRoleClaim<KeyType>>(b => b.HasKey(rc => rc.Id));
            modelBuilder.Entity<LoginHistory<KeyType>>(b => b.HasKey(lh => lh.Id));
            modelBuilder.Entity<RefreshToken<KeyType>>(b => b.HasKey(rt => rt.Id));
            
            // Then apply the feature configuration
            var feature = new IdentityFeature("Test_");
            feature.ConfigureModel(modelBuilder);
        }
    }

    [Fact]
    public void RequiresDbSet_Should_Return_False()
    {
        // Arrange
        var feature = new IdentityFeature();

        // Assert
        Assert.False(feature.RequiresDbSet);
    }

    [Fact]
    public void EntityType_Should_Return_CraftUser()
    {
        // Arrange
        var feature = new IdentityFeature();

        // Assert
        Assert.Equal(typeof(CraftUser), feature.EntityType);
    }

    [Fact]
    public void ConfigureModel_Should_Use_Default_Prefix()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextDefault>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContextDefault(options);

        // Act
        var userEntity = context.Model.FindEntityType(typeof(CraftUser));
        var roleEntity = context.Model.FindEntityType(typeof(CraftRole));

        // Assert
        Assert.NotNull(userEntity);
        Assert.NotNull(roleEntity);
        Assert.Equal("Id_Users", userEntity.GetTableName());
        Assert.Equal("Id_Roles", roleEntity.GetTableName());
    }

    [Fact]
    public void ConfigureModel_Should_Use_Custom_Prefix()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextCustom>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContextCustom(options);

        // Act
        var userEntity = context.Model.FindEntityType(typeof(CraftUser));
        var roleEntity = context.Model.FindEntityType(typeof(CraftRole));

        // Assert
        Assert.NotNull(userEntity);
        Assert.NotNull(roleEntity);
        Assert.Equal("Custom_Users", userEntity.GetTableName());
        Assert.Equal("Custom_Roles", roleEntity.GetTableName());
    }

    [Fact]
    public void ConfigureModel_Should_Configure_All_Identity_Tables()
    {
        // Arrange
        var prefix = "Test_";
        var options = new DbContextOptionsBuilder<TestDbContextTest>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContextTest(options);

        // Act & Assert
        var userRoleEntity = context.Model.FindEntityType(typeof(IdentityUserRole<KeyType>));
        var userClaimEntity = context.Model.FindEntityType(typeof(IdentityUserClaim<KeyType>));
        var userLoginEntity = context.Model.FindEntityType(typeof(IdentityUserLogin<KeyType>));
        var userTokenEntity = context.Model.FindEntityType(typeof(IdentityUserToken<KeyType>));
        var roleClaimEntity = context.Model.FindEntityType(typeof(IdentityRoleClaim<KeyType>));

        Assert.Equal($"{prefix}UserRoles", userRoleEntity?.GetTableName());
        Assert.Equal($"{prefix}UserClaims", userClaimEntity?.GetTableName());
        Assert.Equal($"{prefix}Logins", userLoginEntity?.GetTableName());
        Assert.Equal($"{prefix}UserTokens", userTokenEntity?.GetTableName());
        Assert.Equal($"{prefix}RoleClaims", roleClaimEntity?.GetTableName());
    }

    [Fact]
    public void ConfigureModel_Should_Configure_LoginHistory()
    {
        // Arrange
        var prefix = "Test_";
        var options = new DbContextOptionsBuilder<TestDbContextTest>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContextTest(options);

        // Act
        var loginHistoryEntity = context.Model.FindEntityType(typeof(LoginHistory<KeyType>));

        // Assert
        Assert.NotNull(loginHistoryEntity);
        Assert.Equal($"{prefix}LoginHistory", loginHistoryEntity.GetTableName());
        
        var indexes = loginHistoryEntity.GetIndexes().ToList();
        Assert.Contains(indexes, i => i.Properties.Any(p => p.Name == nameof(LoginHistory<>.UserId)));
        Assert.Contains(indexes, i => i.Properties.Any(p => p.Name == nameof(LoginHistory<>.LastLoginOn)));
    }

    [Fact]
    public void ConfigureModel_Should_Configure_RefreshToken()
    {
        // Arrange
        var prefix = "Test_";
        var options = new DbContextOptionsBuilder<TestDbContextTest>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContextTest(options);

        // Act
        var refreshTokenEntity = context.Model.FindEntityType(typeof(RefreshToken<KeyType>));

        // Assert
        Assert.NotNull(refreshTokenEntity);
        Assert.Equal($"{prefix}RefreshTokens", refreshTokenEntity.GetTableName());
        
        var indexes = refreshTokenEntity.GetIndexes().ToList();
        Assert.Contains(indexes, i => i.Properties.Any(p => p.Name == nameof(RefreshToken<>.UserId)));
        Assert.Contains(indexes, i => i.Properties.Any(p => p.Name == nameof(RefreshToken<>.Token)));
        Assert.Contains(indexes, i => i.Properties.Any(p => p.Name == nameof(RefreshToken<>.ExpiryTime)));
    }

    [Fact]
    public void Generic_IdentityFeature_Should_Work_With_Custom_Types()
    {
        // Arrange
        var feature = new IdentityFeature<CraftUser, CraftRole, KeyType>("Custom_");

        // Assert
        Assert.Equal(typeof(CraftUser), feature.EntityType);
    }
}
