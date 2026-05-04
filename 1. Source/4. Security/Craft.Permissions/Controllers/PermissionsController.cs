using Craft.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.Permissions;

/// <summary>
/// API controller that exposes permission data for the current user and allows
/// role-permission assignment management. Base route: <c>api/permissions</c>.
/// </summary>
/// <typeparam name="TUser">The application user type (must be <see cref="IdentityUser{TKey}"/>).</typeparam>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PermissionsController<TUser>(
    UserManager<TUser> userManager,
    IRolePermissionRepository repository,
    ILogger<PermissionsController<TUser>> logger)
    : ControllerBase
    where TUser : IdentityUser<KeyType>
{
    /// <summary>
    /// Returns the union of all permission codes for the currently authenticated user
    /// across all their roles.
    /// </summary>
    [HttpGet("user")]
    public async Task<ActionResult<int[]>> GetCurrentUserPermissionsAsync(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            logger.LogWarning("GetCurrentUserPermissions: authenticated user not found in store.");
            return Ok(Array.Empty<int>());
        }

        var roleNames = await userManager.GetRolesAsync(user);

        if (roleNames.Count == 0)
            return Ok(Array.Empty<int>());

        // Resolve role names → role IDs via the RoleManager claims
        // We use the user's claims to find role IDs; fall back to name-based lookup
        var roleIds = User.Claims
            .Where(c => c.Type == "roleId")
            .Select(c => { _ = long.TryParse(c.Value, out var id); return id; })
            .Where(id => id != 0)
            .Distinct()
            .ToList();

        if (roleIds.Count == 0)
        {
            logger.LogDebug("No roleId claims found; permissions will be empty for user {UserId}.", user.Id);
            return Ok(Array.Empty<int>());
        }

        var codes = await repository.GetPermissionCodesForRolesAsync(roleIds, cancellationToken);

        return Ok(codes);
    }

    /// <summary>
    /// Returns the permission codes assigned to a specific role.
    /// </summary>
    [HttpGet("role/{roleId:long}")]
    public async Task<ActionResult<int[]>> GetPermissionsForRoleAsync(KeyType roleId, CancellationToken cancellationToken)
    {
        var codes = await repository.GetPermissionCodesForRoleAsync(roleId, cancellationToken);
        return Ok(codes);
    }

    /// <summary>
    /// Assigns a permission code to a role.
    /// </summary>
    [HttpPost("role/{roleId:long}/{permissionCode:int}")]
    public async Task<IActionResult> AssignPermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken)
    {
        await repository.AssignPermissionAsync(roleId, permissionCode, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Revokes a permission code from a role.
    /// </summary>
    [HttpDelete("role/{roleId:long}/{permissionCode:int}")]
    public async Task<IActionResult> RevokePermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken)
    {
        await repository.RevokePermissionAsync(roleId, permissionCode, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Replaces the full permission set for a role with the supplied codes.
    /// </summary>
    [HttpPut("role/{roleId:long}")]
    public async Task<IActionResult> SetPermissionsForRoleAsync(KeyType roleId, [FromBody] IEnumerable<int> permissionCodes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(permissionCodes);
        await repository.SetPermissionsForRoleAsync(roleId, permissionCodes, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
