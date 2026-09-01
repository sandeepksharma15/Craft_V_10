using Craft.Core;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Repositories.Services;

/// <summary>
/// Fake base repository for testing purposes. Returns default/empty results.
/// </summary>
public class FakeBaseRepository<T, TKey> : IBaseRepository<T, TKey> where T : class, IEntity<TKey>, new()
{
    /// <inheritdoc />
    public virtual Task<IDbContext> GetDbContextAsync()
        => Task.FromResult<IDbContext>(null!);

    /// <inheritdoc />
    public virtual Task<DbSet<T>> GetDbSetAsync()
        => Task.FromResult<DbSet<T>>(null!);

    /// <inheritdoc />
    public virtual int SaveChanges()
        => 0;

    /// <inheritdoc />
    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(0);
}

/// <summary>
/// Fake base repository for testing purposes using default key type.
/// </summary>
public class FakeBaseRepository<T> : FakeBaseRepository<T, KeyType>, IBaseRepository<T>
    where T : class, IEntity, new();
