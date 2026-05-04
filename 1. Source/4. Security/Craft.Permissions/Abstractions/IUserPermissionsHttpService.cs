using Craft.Core;

namespace Craft.Permissions;

/// <summary>
/// Client-side HTTP service for fetching the current user's permission codes from the API.
/// </summary>
public interface IUserPermissionsHttpService
{
    /// <summary>
    /// Fetches the union of all permission codes for the currently authenticated user.
    /// Returns an empty array when the user has no permissions or is not authenticated.
    /// </summary>
    Task<ServiceResult<int[]>> GetCurrentUserPermissionsAsync(CancellationToken cancellationToken = default);
}
