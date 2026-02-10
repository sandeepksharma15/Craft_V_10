using Craft.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that configures ASP.NET Core Identity with custom table naming conventions.
/// Registers LoginHistory and RefreshToken DbSets and applies table name conventions.
/// </summary>
/// <typeparam name="TUser">The user entity type (must inherit from CraftUser).</typeparam>
/// <typeparam name="TRole">The role entity type (must inherit from CraftRole).</typeparam>
/// <typeparam name="TKey">The type of the primary key for identity entities.</typeparam>
public class IdentityFeature<TUser, TRole, TKey> : IDbContextFeature, IDbSetProvider
    where TUser : CraftUser<TKey>
    where TRole : CraftRole<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly string _tablePrefix;

    /// <summary>
    /// Initializes a new instance of IdentityFeature with custom table prefix.
    /// </summary>
    /// <param name="tablePrefix">Prefix for Identity tables (default: "Id_").</param>
    public IdentityFeature(string tablePrefix = "Id_") => _tablePrefix = tablePrefix;

    /// <summary>
    /// This feature doesn't require a single DbSet as it configures multiple Identity entities.
    /// </summary>
    public bool RequiresDbSet => false;

    /// <summary>
    /// Not used for this feature as it manages multiple entity types.
    /// </summary>
    public Type EntityType => typeof(TUser);

    /// <summary>
    /// Configures Identity entity table names and relationships.
    /// </summary>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Configure custom table names for Identity entities
        modelBuilder.Entity<TUser>(b => b.ToTable($"{_tablePrefix}Users"));
        modelBuilder.Entity<TRole>(b => b.ToTable($"{_tablePrefix}Roles"));
        modelBuilder.Entity<IdentityUserRole<TKey>>(b => b.ToTable($"{_tablePrefix}UserRoles"));
        modelBuilder.Entity<IdentityUserClaim<TKey>>(b => b.ToTable($"{_tablePrefix}UserClaims"));
        modelBuilder.Entity<IdentityUserLogin<TKey>>(b => b.ToTable($"{_tablePrefix}Logins"));
        modelBuilder.Entity<IdentityUserToken<TKey>>(b => b.ToTable($"{_tablePrefix}UserTokens"));
        modelBuilder.Entity<IdentityRoleClaim<TKey>>(b => b.ToTable($"{_tablePrefix}RoleClaims"));

        // Configure LoginHistory
        modelBuilder.Entity<LoginHistory<TKey>>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable($"{_tablePrefix}LoginHistory");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LastLoginOn);
        });

        // Configure RefreshToken
        modelBuilder.Entity<RefreshToken<TKey>>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable($"{_tablePrefix}RefreshTokens");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.ExpiryTime);
        });
    }
}

/// <summary>
/// Feature that configures ASP.NET Core Identity with default CraftUser, CraftRole, and KeyType.
/// </summary>
public class IdentityFeature : IdentityFeature<CraftUser, CraftRole, KeyType>
{
    /// <summary>
    /// Initializes a new instance of IdentityFeature with default types and custom table prefix.
    /// </summary>
    /// <param name="tablePrefix">Prefix for Identity tables (default: "Id_").</param>
    public IdentityFeature(string tablePrefix = "Id_") : base(tablePrefix)
    {
    }
}

