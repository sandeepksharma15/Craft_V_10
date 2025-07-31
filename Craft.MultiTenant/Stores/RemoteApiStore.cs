using Craft.Core;
using Craft.Data.Abstractions;
using Craft.Domain;
using Craft.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Craft.MultiTenant.Stores;

public class RemoteApiStore<T> : ITenantStore<T> where T : class, ITenant, IEntity, new()
{
    private readonly RemoteApiStoreClient<T> _client;
    private readonly string _endpointTemplate;

    internal static readonly string EndpointIdentifierToken = $"{{{TenantConstants.TenantToken}}}";

    public RemoteApiStore(RemoteApiStoreClient<T> client, string endpointTemplate)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(endpointTemplate, nameof(endpointTemplate));

        _client = client;
        _endpointTemplate = endpointTemplate;

        if (!endpointTemplate.Contains(EndpointIdentifierToken))
            if (endpointTemplate.EndsWith('/'))
                endpointTemplate += EndpointIdentifierToken;
            else
                endpointTemplate += $"/{EndpointIdentifierToken}";

        if (Uri.IsWellFormedUriString(endpointTemplate, UriKind.Absolute))
            throw new ArgumentException("Parameter 'endpointTemplate' is not a well formed uri.", nameof(endpointTemplate));

        if (!endpointTemplate.StartsWith("https", StringComparison.OrdinalIgnoreCase)
            && !endpointTemplate.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Parameter 'endpointTemplate' is not an http or https uri.", nameof(endpointTemplate));

        _endpointTemplate = endpointTemplate;
    }


    public async Task<T?> GetByIdentifierAsync(string identifier, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return await _client.GetByIdentifierAsync(_endpointTemplate, identifier);
    }

    #region CRUD Members Not Implemented

    public Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetHostAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DbContext> GetDbContextAsync()
    {
        throw new NotImplementedException();
    }

    public Task<DbSet<T>> GetDbSetAsync()
    {
        throw new NotImplementedException();
    }

    public int SaveChanges()
    {
        throw new NotImplementedException();
    }

    Task<IReadOnlyList<T>> IReadRepository<T, long>.GetAllAsync(bool includeDetails, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<T> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetAsync(KeyType id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    Task<List<T>> IChangeRepository<T, long>.AddRangeAsync(IEnumerable<T> entities, bool autoSave, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task<T> IChangeRepository<T, long>.DeleteAsync(T entity, bool autoSave, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task<List<T>> IChangeRepository<T, long>.DeleteRangeAsync(IEnumerable<T> entities, bool autoSave, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task<List<T>> IChangeRepository<T, long>.UpdateRangeAsync(IEnumerable<T> entities, bool autoSave, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<PageResponse<T>> GetPagedListAsync(int currentPage, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    Task<IDbContext> IBaseRepository<T, long>.GetDbContextAsync()
    {
        throw new NotImplementedException();
    }


    #endregion
}

public class RemoteApiStore(RemoteApiStoreClient<Tenant> client, string endpointTemplate)
    : RemoteApiStore<Tenant>(client, endpointTemplate) { }
