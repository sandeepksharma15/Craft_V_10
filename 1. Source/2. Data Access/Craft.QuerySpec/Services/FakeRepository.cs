using Craft.Core;
using Craft.Domain;
using Craft.Repositories.Services;

namespace Craft.QuerySpec.Services;

/// <summary>
/// Fake repository for testing purposes. Returns empty/default results for all query operations.
/// </summary>
public class FakeRepository<T, TKey> : FakeChangeRepository<T, TKey>, IRepository<T, TKey>
    where T : class, IEntity<TKey>, new()
{
    /// <inheritdoc />
    public virtual Task DeleteAsync(IQuery<T> query, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<List<T>> GetAllAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.FromResult(new List<T>());
    }

    /// <inheritdoc />
    public virtual Task<List<TResult>> GetAllAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.FromResult(new List<TResult>());
    }

    /// <inheritdoc />
    public virtual Task<T?> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.FromResult<T?>(null);
    }

    /// <inheritdoc />
    public virtual Task<TResult?> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.FromResult<TResult?>(null);
    }

    /// <inheritdoc />
    public virtual Task<long> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.FromResult(0L);
    }

    /// <inheritdoc />
    public virtual Task<PageResponse<T>> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.FromResult(new PageResponse<T>([], 0, 1, 10));
    }

    /// <inheritdoc />
    public virtual Task<PageResponse<TResult>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.FromResult(new PageResponse<TResult>([], 0, 1, 10));
    }
}

/// <summary>
/// Fake repository for testing purposes using default key type.
/// </summary>
public class FakeRepository<T> : FakeRepository<T, KeyType>, IRepository<T>
    where T : class, IEntity, new();
