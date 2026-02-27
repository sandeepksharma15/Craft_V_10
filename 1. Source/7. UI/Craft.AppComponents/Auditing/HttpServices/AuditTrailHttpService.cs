using System.Net.Http.Json;
using Craft.Auditing;
using Craft.Core;
using Craft.QuerySpec;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Auditing;

/// <summary>
/// HTTP service for querying audit trail data from the API.
/// Inherits standard CRUD operations and adds audit-specific endpoints.
/// </summary>
public class AuditTrailHttpService(Uri apiURL, HttpClient httpClient, ILogger<AuditTrailHttpService> logger)
    : HttpService<AuditTrail, AuditTrail, AuditTrail, KeyType>(apiURL, httpClient, logger), IAuditTrailHttpService
{
    /// <inheritdoc />
    public async Task<ServiceResult<List<string>?>> GetTableNamesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAndParseAsync(
            ct => _httpClient.GetAsync(new Uri($"{_apiURL}/tablenames"), ct),
            (content, ct) => content.ReadFromJsonAsync<List<string>>(cancellationToken: ct),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<List<AuditUserDTO>?>> GetAuditUsersAsync(CancellationToken cancellationToken = default)
    {
        return await GetAndParseAsync(
            ct => _httpClient.GetAsync(new Uri($"{_apiURL}/auditusers"), ct),
            (content, ct) => content.ReadFromJsonAsync<List<AuditUserDTO>>(cancellationToken: ct),
            cancellationToken);
    }
}
