using System.Net.Http.Json;
using Craft.Core;
using Craft.HttpServices;
using Microsoft.Extensions.Logging;

namespace Craft.Permissions;

/// <summary>
/// Client-side HTTP service for managing role-permission assignments.
/// Base URL should be <c>/api/permissions</c>.
/// </summary>
public class RolePermissionHttpService : HttpServiceBase, IRolePermissionHttpService
{
    private readonly ILogger<RolePermissionHttpService> _logger;

    public RolePermissionHttpService(Uri apiURL, HttpClient httpClient, ILogger<RolePermissionHttpService> logger)
        : base(apiURL, httpClient)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int[]>> GetPermissionsForRoleAsync(KeyType roleId, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[RolePermissionHttpService] Method: [\"GetPermissionsForRoleAsync\"] RoleId: {RoleId}", roleId);

        var result = await GetAndParseAsync<int[]>(
            ct => _httpClient.GetAsync(new Uri($"{_apiURL}/role/{roleId}"), ct),
            (content, ct) => content.ReadFromJsonAsync<int[]>(cancellationToken: ct)!,
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? ServiceResult<int[]>.Success(result.Value)
            : ServiceResult<int[]>.Success([]);
    }

    /// <inheritdoc />
    public Task<ServiceResult<bool>> AssignPermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[RolePermissionHttpService] Method: [\"AssignPermissionAsync\"] RoleId: {RoleId}, Code: {Code}", roleId, permissionCode);

        return SendAndParseNoContentAsync(
            ct => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/role/{roleId}/{permissionCode}"), (object?)null, cancellationToken: ct),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ServiceResult<bool>> RevokePermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[RolePermissionHttpService] Method: [\"RevokePermissionAsync\"] RoleId: {RoleId}, Code: {Code}", roleId, permissionCode);

        return SendAndParseNoContentAsync(
            ct => _httpClient.DeleteAsync(new Uri($"{_apiURL}/role/{roleId}/{permissionCode}"), ct),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ServiceResult<bool>> SetPermissionsForRoleAsync(KeyType roleId, IEnumerable<int> permissionCodes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permissionCodes);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[RolePermissionHttpService] Method: [\"SetPermissionsForRoleAsync\"] RoleId: {RoleId}", roleId);

        return SendAndParseNoContentAsync(
            ct => _httpClient.PutAsJsonAsync(new Uri($"{_apiURL}/role/{roleId}"), permissionCodes, cancellationToken: ct),
            cancellationToken);
    }
}
