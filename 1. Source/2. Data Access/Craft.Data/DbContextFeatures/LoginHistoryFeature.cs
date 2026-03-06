using Craft.Security;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that registers and configures the LoginHistory entity for EF Core.
/// Applies the ID_LoginHistory table, an index on UserId, and an index on LastLoginOn.
/// </summary>
/// <typeparam name="TKey">The primary key type shared with the user entity.</typeparam>
public class LoginHistoryFeature<TKey> : IDbContextFeature
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc/>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoginHistory<TKey>>(entity =>
        {
            entity.ToTable("ID_LoginHistory");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LastLoginOn);
        });
    }
}

/// <summary>
/// Feature that registers and configures the LoginHistory entity using the default <see cref="KeyType"/>.
/// </summary>
public class LoginHistoryFeature : LoginHistoryFeature<KeyType>;
