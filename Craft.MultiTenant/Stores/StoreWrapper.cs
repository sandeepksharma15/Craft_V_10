using Craft.Core;
using Craft.Data.Abstractions;
using Craft.Domain;
using Craft.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.MultiTenant.Stores;

public class StoreWrapper<T> : ITenantStore<T> where T : class, ITenant, IEntity, new()
{
    private readonly ILogger _logger;
    private readonly ITenantStore<T> _store;

    public StoreWrapper(ITenantStore<T> store, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(store, nameof(store));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _logger = logger;
        _store = store;
    }

    public async Task<T> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(entity.Identifier, nameof(entity.Identifier));

        var existing = await GetAsync(entity.Id, false, cancellationToken);
        var existingByIdentifier = await GetByIdentifierAsync(entity.Identifier, false, cancellationToken);

        if (existing != null || existingByIdentifier != null)
            throw new MultiTenantException($"Tenant with Id '{entity.Id}' or Identifier '{entity.Identifier}' already exists.");

        // Check that there is only one host tenant
        var host = await GetHostAsync(false, cancellationToken);

        if (host is not null && entity.Type == TenantType.Host)
            throw new MultiTenantException("There is already a host tenant.");

        return await _store.AddAsync(entity, autoSave, cancellationToken);
    }

    public async Task<T> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(entity.Identifier, nameof(entity.Identifier));

        try
        {
            await _store.DeleteAsync(entity, autoSave, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in DeleteAsync");

            throw;
        }

        return entity;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        var result = new List<T>();

        try
        {
            result = (List<T>)await _store.GetAllAsync(includeDetails, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in GetAllAsync");
        }

        return result;
    }

    public async Task<T?> GetAsync(long id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        T? result = null;

        try
        {
            result = await _store.GetAsync(id, includeDetails, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
                if (result == null)
                    _logger.LogDebug("GetAsync: Unable to find Tenant Id \"{TenantId}\".", id);
                else
                    _logger.LogDebug("GetAsync: Tenant Id \"{TenantId}\" found.", id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in GetAsync");
        }

        return result;
    }

    public async Task<T?> GetByIdentifierAsync(string identifier, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        T? result = null;

        ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));

        try
        {
            result = await _store.GetByIdentifierAsync(identifier, includeDetails, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
                if (result != null)
                    _logger.LogDebug("GetByIdentifierAsync: Tenant found with identifier \"{TenantIdentifier}\"", identifier);
                else
                    _logger.LogDebug("GetByIdentifierAsync: Unable to find Tenant with identifier \"{TenantIdentifier}\"", identifier);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in TryGetByIdentifierAsync");
        }

        return result;
    }

    public Task<long> GetCountAsync(CancellationToken cancellationToken = default) 
        => _store.GetCountAsync(cancellationToken);

    public async Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(entity.Identifier, nameof(entity.Identifier));

        try
        {
            // Check that there is no duplicate identifier
            ITenant? tenant = await GetByIdentifierAsync(entity.Identifier, false, cancellationToken);

            if (tenant != null && tenant.Id != entity.Id)
                throw new MultiTenantException($"Tenant with identifier \"{entity.Identifier}\" already exists.");

            // Check that there is only one host tenant
            if (entity.Type == TenantType.Host)
            {
                ITenant? host = await GetHostAsync(false, cancellationToken);

                if (host != null && host.Id != entity.Id)
                    throw new MultiTenantException("There is already a host tenant.");
            }

            var existing = await GetAsync(entity.Id, false, cancellationToken);

            if (existing is not null)
                entity = await _store.UpdateAsync(entity, autoSave, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in UpdateAsync");
        }

        return entity;
    }

    #region CRUD Members Not Implemented

    public Task<List<T>> AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IDbContext> GetDbContextAsync()
    {
        throw new NotImplementedException();
    }

    public Task<DbSet<T>> GetDbSetAsync()
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetHostAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PageResponse<T>> GetPagedListAsync(int currentPage, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int SaveChanges()
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    #endregion
}
