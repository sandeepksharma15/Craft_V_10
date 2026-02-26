namespace Craft.Auditing;

/// <summary>
/// Represents a user who has audit trail entries.
/// </summary>
/// <param name="UserId">The user's ID.</param>
/// <param name="DisplayName">The user's display name.</param>
public record AuditUserInfo(long UserId, string? DisplayName);
