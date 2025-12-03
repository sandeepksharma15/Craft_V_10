using Craft.Domain;
using Craft.Cache;

namespace Craft.MultiTenant;

public class CacheStore(ICacheService cacheService, ITenantStore<Tenant> tenantRepository)
    : CacheStore<Tenant>(cacheService, tenantRepository), ITenantStore
{
}

public class CacheStore<T>(ICacheService cacheService, ITenantStore<T> tenantRepository)
    : ITenantStore<T> where T : class, ITenant, IEntity, new()
{
    private const string _cacheKey = "_TENANT_STORE";

    private readonly ICacheService _cacheService = cacheService;
    private readonly ITenantStore<T> _tenantRepository = tenantRepository;

    private async Task<IReadOnlyList<T>?> GetTenantList()
    {
        // Get From The Cache
        (bool hasKey, IReadOnlyList<T>? tenants) = _cacheService.TryGet<List<T>>(_cacheKey);

        // If Key Is Missing, Get Again From Repository
        if (!hasKey || tenants == null)
            tenants = await _tenantRepository.GetAllAsync();

        // Set In Cache
        _cacheService.Set(_cacheKey, tenants);

        return tenants;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
        => await GetTenantList() ?? [];

    public async Task<T?> GetAsync(KeyType id, bool includeDetails = false, CancellationToken cancellationToken = default)
        => (await GetTenantList())?.ToList()?.Find(t => t.Id == id);

    public async Task<T?> GetByIdentifierAsync(string identifier, bool includeDetails = false, CancellationToken cancellationToken = default)
        => (await GetTenantList())?.ToList()?.Find(t => t.Identifier == identifier);

    public async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
        => (await GetTenantList())?.Count ?? 0;

    public async Task<T?> GetHostAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
        => (await GetTenantList())?.ToList()?.Find(t => t.Type == TenantType.Host);

    #region CRUD Operations Not Implemented

    public Task<T?> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T?> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    #endregion
}
