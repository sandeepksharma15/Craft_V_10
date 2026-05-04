using Craft.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Permissions;

/// <summary>
/// EF Core implementation of <see cref="IRolePermissionRepository"/>.
/// Requires the consuming app's <see cref="DbContext"/> to implement <see cref="IPermissionDbContext"/>.
/// </summary>
public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly IPermissionDbContext _dbContext;
    private readonly ILogger<RolePermissionRepository> _logger;

    public RolePermissionRepository(IPermissionDbContext dbContext, ILogger<RolePermissionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<int>> GetPermissionCodesForRoleAsync(KeyType roleId, CancellationToken cancellationToken = default)
    {
        var codes = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionCode)
            .ToListAsync(cancellationToken);

        return codes;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<int>> GetPermissionCodesForRolesAsync(IEnumerable<KeyType> roleIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roleIds);

        var idList = roleIds.ToList();

        var codes = await _dbContext.RolePermissions
            .Where(rp => idList.Contains(rp.RoleId))
            .Select(rp => rp.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        return codes;
    }

    /// <inheritdoc />
    public async Task AssignPermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionCode == permissionCode, cancellationToken);

        if (exists)
        {
            _logger.LogDebug("Permission {Code} already assigned to role {RoleId}; skipping.", permissionCode, roleId);
            return;
        }

        _dbContext.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionCode = permissionCode });
    }

    /// <inheritdoc />
    public async Task RevokePermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionCode == permissionCode, cancellationToken);

        if (entry is null)
        {
            _logger.LogDebug("Permission {Code} not found on role {RoleId}; nothing to revoke.", permissionCode, roleId);
            return;
        }

        _dbContext.RolePermissions.Remove(entry);
    }

    /// <inheritdoc />
    public async Task SetPermissionsForRoleAsync(KeyType roleId, IEnumerable<int> permissionCodes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permissionCodes);

        var existing = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _dbContext.RolePermissions.RemoveRange(existing);

        var newEntries = permissionCodes
            .Distinct()
            .Select(code => new RolePermission { RoleId = roleId, PermissionCode = code });

        _dbContext.RolePermissions.AddRange(newEntries);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContext is DbContext efContext)
            return await efContext.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("IPermissionDbContext does not inherit DbContext; SaveChangesAsync was not called.");
        return 0;
    }
}
