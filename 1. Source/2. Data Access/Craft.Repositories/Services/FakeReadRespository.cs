using Craft.Core;
using Craft.Domain;

namespace Craft.Repositories.Services;

/// <summary>
/// Fake read repository for testing purposes. Returns empty/default results.
/// </summary>
public class FakeReadRepository<T, TKey> : FakeBaseRepository<T, TKey>, IReadRepository<T, TKey>
    where T : class, IEntity<TKey>, new()
{
    /// <inheritdoc />
    public virtual Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    /// <inheritdoc />
    public virtual Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    /// <inheritdoc />
    public virtual Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<T>>([]);

    /// <inheritdoc />
    public virtual Task<T?> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default)
        => Task.FromResult<T?>(null);

    /// <inheritdoc />
    public virtual Task<long> GetCountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(0L);

    /// <inheritdoc />
    public virtual Task<PageResponse<T>> GetPagedListAsync(int currentPage, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default)
        => Task.FromResult(new PageResponse<T>([], 0, currentPage, pageSize));
}

/// <summary>
/// Fake read repository for testing purposes using default key type.
/// </summary>
public class FakeReadRepository<T> : FakeReadRepository<T, KeyType>, IReadRepository<T>
    where T : class, IEntity, new();
