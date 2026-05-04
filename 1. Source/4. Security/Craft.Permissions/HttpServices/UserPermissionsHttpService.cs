using System.Net.Http.Json;
using Craft.Core;
using Craft.HttpServices;
using Microsoft.Extensions.Logging;

namespace Craft.Permissions;

/// <summary>
/// Client-side HTTP service that fetches the current user's permission codes from the API.
/// Calls <c>GET /api/permissions/user</c> which returns an <c>int[]</c>.
/// </summary>
public class UserPermissionsHttpService : HttpServiceBase, IUserPermissionsHttpService
{
    private readonly ILogger<UserPermissionsHttpService> _logger;

    public UserPermissionsHttpService(Uri apiURL, HttpClient httpClient, ILogger<UserPermissionsHttpService> logger)
        : base(apiURL, httpClient)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int[]>> GetCurrentUserPermissionsAsync(CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[UserPermissionsHttpService] Method: [\"GetCurrentUserPermissionsAsync\"]");

        var result = await GetAndParseAsync<int[]>(
            ct => _httpClient.GetAsync(new Uri($"{_apiURL}/user"), ct),
            (content, ct) => content.ReadFromJsonAsync<int[]>(cancellationToken: ct)!,
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? ServiceResult<int[]>.Success(result.Value)
            : ServiceResult<int[]>.Success([]);
    }
}
