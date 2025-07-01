using Craft.Data.Abstractions;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Repositories;

public interface IBaseRepository<T, TKey> : IRepository where T : class, IEntity<TKey>, new ()
{
    /// <summary>
    /// Returns the <see cref="DbContext"/> instance
    /// </summary>
    /// <returns></returns>
    Task<IDbContext> GetDbContextAsync();

    /// <summary>
    /// Returns the <see cref="DbSet{T}"/> instance
    /// </summary>
    /// <returns></returns>
    Task<DbSet<T>> GetDbSetAsync();

    /// <summary>
    /// Persists changes to the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to the database.
    /// </summary>
    /// <returns></returns>
    int SaveChanges();
}

public interface IBaseRepository<T> : IBaseRepository<T, KeyType> where T : class, IEntity, new ();
