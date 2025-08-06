using Craft.Core;
using Craft.Data.Abstractions;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.MultiTenant.Stores;

public class DbStore<TStoreDbContext, T>(TStoreDbContext dbContext) : ITenantStore<T>
        where TStoreDbContext : DbContext, ITenantStoreDbContext<T>
        where T : class, ITenant, IEntity, new()
{
    internal readonly TStoreDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public virtual async Task<T?> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext
            .Tenants
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        if (autoSave)
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        result.State = EntityState.Detached;

        return result.Entity;
    }

    public virtual async Task<List<T>> AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await _dbContext
                   .Tenants
                   .AddRangeAsync(entities, cancellationToken)
                   .ConfigureAwait(false);

        if (autoSave)
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return [.. entities];
    }

    public virtual async Task<T?> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext
            .Tenants
            .Where(ti => ti.Identifier == entity.Identifier)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)
                ?? throw new MultiTenantException($"There is no tenant with the identifier '{entity.Identifier}'");

        try
        {
            if (entity is ISoftDelete softDeleteEntity)
            {
                softDeleteEntity.IsDeleted = true;
                _dbContext.Tenants.Update(entity);
            }
            else
                _dbContext.Tenants.Remove(entity);

            if (autoSave)
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return existing;
        }
        catch (Exception ex)
        {
            throw new MultiTenantException($"Could not delete tenant with identifier '{entity.Identifier}'", ex);
        }
    }

    public virtual async Task<List<T>> DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        _dbContext
            .Tenants.RemoveRange(entities);

        if (autoSave)
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return [.. entities];
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .Tenants
            .IncludeDetails(includeDetails)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> GetAsync(long id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .Tenants
            .IncludeDetails(includeDetails)
            .AsNoTracking()
            .Where(ti => ti.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<T?> GetByIdentifierAsync(string identifier, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .Tenants
            .AsNoTracking()
            .Where(ti => ti.Identifier == identifier)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext
            .Tenants
            .LongCountAsync(cancellationToken);
    }

    public virtual Task<IDbContext> GetDbContextAsync() 
        => Task.FromResult((IDbContext)_dbContext);

    public virtual Task<DbSet<T>> GetDbSetAsync()
        => Task.FromResult(_dbContext.Tenants);

    public virtual Task<T?> GetHostAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return _dbContext
            .Tenants
            .AsNoTracking()
            .Where(ti => ti.Type == TenantType.Host)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<PageResponse<T>> GetPagedListAsync(int currentPage, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual int SaveChanges()
        => _dbContext.SaveChanges();

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) 
        => await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    public virtual async Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        var result = _dbContext.Tenants.Update(entity);

        if (autoSave)
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        result.State = EntityState.Detached;

        return result.Entity;
    }

    public virtual async Task<List<T>> UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        _dbContext.Tenants.UpdateRange(entities);

        if (autoSave)
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return [.. entities];
    }
}
