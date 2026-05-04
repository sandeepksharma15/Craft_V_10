using Craft.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Permissions;

/// <summary>
/// EF Core implementation of <see cref="IRolePermissionRepository"/>.
/// Requires the consuming app to call <c>Features.AddPermissions()</c> in its DbContext
/// so that <see cref="RolePermission"/> is included in the model.
/// </summary>
public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly DbContext _dbContext;
    private readonly ILogger<RolePermissionRepository> _logger;

    public RolePermissionRepository(DbContext dbContext, ILogger<RolePermissionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<int>> GetPermissionCodesForRoleAsync(KeyType roleId, CancellationToken cancellationToken = default)
    {
        var codes = await _dbContext.Set<RolePermission>()
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

        var codes = await _dbContext.Set<RolePermission>()
            .Where(rp => idList.Contains(rp.RoleId))
            .Select(rp => rp.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        return codes;
    }

    /// <inheritdoc />
    public async Task AssignPermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Set<RolePermission>()
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionCode == permissionCode, cancellationToken);

        if (exists)
        {
            _logger.LogDebug("Permission {Code} already assigned to role {RoleId}; skipping.", permissionCode, roleId);
            return;
        }

        _dbContext.Set<RolePermission>().Add(new RolePermission { RoleId = roleId, PermissionCode = permissionCode });
    }

    /// <inheritdoc />
    public async Task RevokePermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionCode == permissionCode, cancellationToken);

        if (entry is null)
        {
            _logger.LogDebug("Permission {Code} not found on role {RoleId}; nothing to revoke.", permissionCode, roleId);
            return;
        }

        _dbContext.Set<RolePermission>().Remove(entry);
    }

    /// <inheritdoc />
    public async Task SetPermissionsForRoleAsync(KeyType roleId, IEnumerable<int> permissionCodes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permissionCodes);

        var existing = await _dbContext.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _dbContext.Set<RolePermission>().RemoveRange(existing);

        var newEntries = permissionCodes
            .Distinct()
            .Select(code => new RolePermission { RoleId = roleId, PermissionCode = code });

        _dbContext.Set<RolePermission>().AddRange(newEntries);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
