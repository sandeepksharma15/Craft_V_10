using Craft.Data.DbContextFeatures;
using Craft.MultiTenant;
using Craft.Security;

namespace Craft.Data;

/// <summary>
/// Extension methods for fluent registration of DbContext features.
/// </summary>
public static class DbContextFeatureExtensions
{
    /// <summary>
    /// Adds audit trail functionality to the DbContext.
    /// Automatically tracks entity changes for entities marked with [Audit] attribute.
    /// </summary>
    public static DbContextFeatureCollection AddAuditTrail(this DbContextFeatureCollection features)
    {
        return features.AddFeature<AuditTrailFeature>();
    }

    /// <summary>
    /// Adds soft delete functionality to the DbContext.
    /// Automatically converts hard deletes to soft deletes for entities implementing ISoftDelete.
    /// </summary>
    public static DbContextFeatureCollection AddSoftDelete(this DbContextFeatureCollection features)
    {
        return features.AddFeature<SoftDeleteFeature>();
    }

    /// <summary>
    /// Adds multi-tenancy functionality to the DbContext.
    /// Automatically filters queries and sets tenant ID for entities implementing IHasTenant.
    /// </summary>
    public static DbContextFeatureCollection AddMultiTenancy(this DbContextFeatureCollection features, ITenant currentTenant)
    {
        return features.AddFeature(new MultiTenancyFeature(currentTenant));
    }

    /// <summary>
    /// Adds concurrency management functionality to the DbContext.
    /// Automatically updates concurrency stamps for entities implementing IHasConcurrency.
    /// </summary>
    public static DbContextFeatureCollection AddConcurrency(this DbContextFeatureCollection features)
    {
        return features.AddFeature<ConcurrencyFeature>();
    }

    /// <summary>
    /// Adds version tracking functionality to the DbContext.
    /// Automatically sets initial version and increments version on modifications for entities implementing IHasVersion.
    /// </summary>
    public static DbContextFeatureCollection AddVersionTracking(this DbContextFeatureCollection features)
    {
        return features.AddFeature<VersionTrackingFeature>();
    }

    /// <summary>
    /// Adds ASP.NET Core Identity configuration with custom table naming.
    /// Configures Identity tables with the specified prefix (default: "Id_").
    /// </summary>
    /// <typeparam name="TUser">The user entity type.</typeparam>
    /// <typeparam name="TRole">The role entity type.</typeparam>
    /// <typeparam name="TKey">The primary key type.</typeparam>
    public static DbContextFeatureCollection AddIdentity<TUser, TRole, TKey>(this DbContextFeatureCollection features, string tablePrefix = "ID_")
        where TUser : CraftUser<TKey>
        where TRole : CraftRole<TKey>
        where TKey : IEquatable<TKey>
    {
        return features.AddFeature(new IdentityFeature<TUser, TRole, TKey>(tablePrefix));
    }

    /// <summary>
    /// Adds ASP.NET Core Identity configuration with default CraftUser, CraftRole, and KeyType.
    /// Configures Identity tables with the specified prefix (default: "Id_").
    /// </summary>
    public static DbContextFeatureCollection AddIdentity(this DbContextFeatureCollection features, string tablePrefix = "ID_")
    {
        return features.AddFeature(new IdentityFeature(tablePrefix));
    }

    /// <summary>
    /// Adds refresh token support to the DbContext.
    /// Configures the ID_RefreshTokens table, a unique index on Token, an index on UserId,
    /// and a global query filter that excludes soft-deleted records.
    /// </summary>
    /// <typeparam name="TKey">The primary key type shared with the user entity.</typeparam>
    public static DbContextFeatureCollection AddRefreshTokens<TKey>(this DbContextFeatureCollection features)
        where TKey : IEquatable<TKey>
    {
        return features.AddFeature(new RefreshTokensFeature<TKey>());
    }

    /// <summary>
    /// Adds refresh token support to the DbContext using the default <see cref="KeyType"/>.
    /// Configures the ID_RefreshTokens table, a unique index on Token, an index on UserId,
    /// and a global query filter that excludes soft-deleted records.
    /// </summary>
    public static DbContextFeatureCollection AddRefreshTokens(this DbContextFeatureCollection features)
    {
        return features.AddFeature<RefreshTokensFeature>();
    }

    /// <summary>
    /// Adds login history support to the DbContext.
    /// Configures the ID_LoginHistory table, an index on UserId, and an index on LastLoginOn.
    /// </summary>
    /// <typeparam name="TKey">The primary key type shared with the user entity.</typeparam>
    public static DbContextFeatureCollection AddLoginHistory<TKey>(this DbContextFeatureCollection features)
        where TKey : IEquatable<TKey>
    {
        return features.AddFeature(new LoginHistoryFeature<TKey>());
    }

    /// <summary>
    /// Adds login history support to the DbContext using the default <see cref="KeyType"/>.
    /// Configures the ID_LoginHistory table, an index on UserId, and an index on LastLoginOn.
    /// </summary>
    public static DbContextFeatureCollection AddLoginHistory(this DbContextFeatureCollection features)
    {
        return features.AddFeature<LoginHistoryFeature>();
    }

    /// <summary>
    /// Adds all common features: AuditTrail, SoftDelete, Concurrency, and VersionTracking.
    /// </summary>
    public static DbContextFeatureCollection AddCommonFeatures(this DbContextFeatureCollection features)
    {
        return features
            .AddAuditTrail()
            .AddSoftDelete()
            .AddConcurrency()
            .AddVersionTracking();
    }

    /// <summary>
    /// Adds all features including multi-tenancy: AuditTrail, SoftDelete, Concurrency, VersionTracking, and MultiTenancy.
    /// </summary>
    public static DbContextFeatureCollection AddAllFeatures(this DbContextFeatureCollection features, ITenant currentTenant)
    {
        return features
            .AddAuditTrail()
            .AddSoftDelete()
            .AddConcurrency()
            .AddVersionTracking()
            .AddMultiTenancy(currentTenant);
    }
}

