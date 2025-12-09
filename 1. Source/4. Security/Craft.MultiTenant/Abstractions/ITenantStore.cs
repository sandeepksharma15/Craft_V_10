using Craft.Domain;

namespace Craft.MultiTenant;

public interface ITenantStore<T> where T : class, ITenant, IEntity, new()
{
    Task<T?> GetAsync(long id, bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);

    Task<T?> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);
    Task<T?> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);

    Task<T?> GetByIdentifierAsync(string identifier, bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<T?> GetHostAsync(bool includeDetails = false, CancellationToken cancellationToken = default);
}

public interface ITenantStore : ITenantStore<Tenant>;
