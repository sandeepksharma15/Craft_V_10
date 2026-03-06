using Craft.Security;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that registers and configures the RefreshToken entity for EF Core.
/// Applies the ID_RefreshTokens table, a unique index on Token, an index on UserId,
/// and a global query filter that excludes soft-deleted records.
/// </summary>
/// <typeparam name="TKey">The primary key type shared with the user entity.</typeparam>
public class RefreshTokensFeature<TKey> : IDbContextFeature
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc/>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken<TKey>>(entity =>
        {
            entity.ToTable("ID_RefreshTokens");
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}

/// <summary>
/// Feature that registers and configures the RefreshToken entity using the default <see cref="KeyType"/>.
/// </summary>
public class RefreshTokensFeature : RefreshTokensFeature<KeyType>;
