namespace Craft.AppComponents.Auditing;

/// <summary>
/// Resolves user display information from a list of user IDs for audit trail display.
/// Implement this interface in the host application to provide app-specific user resolution.
/// </summary>
public interface IAuditUserResolver
{
    /// <summary>
    /// Returns display information for the given user IDs.
    /// </summary>
    /// <param name="userIds">The list of user IDs to resolve.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A list of <see cref="AuditUserDTO"/> containing the resolved user display names.</returns>
    Task<List<AuditUserDTO>> GetAuditUsersAsync(List<KeyType> userIds, CancellationToken cancellationToken = default);
}
