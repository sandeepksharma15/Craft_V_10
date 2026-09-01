using Craft.Domain;

namespace Craft.Repositories.Services;

/// <summary>
/// Fake change repository for testing purposes. All mutations are no-ops.
/// </summary>
public class FakeChangeRepository<T, TKey> : FakeReadRepository<T, TKey>, IChangeRepository<T, TKey>
    where T : class, IEntity<TKey>, new()
{
    /// <inheritdoc />
    public virtual Task<T> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public virtual Task<List<T>> AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return Task.FromResult(entities.ToList());
    }

    /// <inheritdoc />
    public virtual Task<T> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public virtual Task<List<T>> DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return Task.FromResult(entities.ToList());
    }

    /// <inheritdoc />
    public virtual Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public virtual Task<List<T>> UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return Task.FromResult(entities.ToList());
    }

    /// <inheritdoc />
    public virtual Task<T> RestoreAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public virtual Task<List<T>> RestoreRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return Task.FromResult(entities.ToList());
    }
}

/// <summary>
/// Fake change repository for testing purposes using default key type.
/// </summary>
public class FakeChangeRepository<T> : FakeChangeRepository<T, KeyType>, IChangeRepository<T>
    where T : class, IEntity, new();
