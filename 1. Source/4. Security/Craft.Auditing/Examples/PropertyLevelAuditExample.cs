namespace Craft.Auditing.Examples;

/// <summary>
/// Example demonstrating property-level audit control.
/// This class shows how to exclude sensitive properties from audit trails.
/// </summary>
[Audit]
public class UserExample
{
    public int Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Password hash is excluded from audit trail for security.
    /// </summary>
    [DoNotAudit]
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh tokens are excluded from audit trail for security.
    /// </summary>
    [DoNotAudit]
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Security stamps are excluded from audit trail.
    /// </summary>
    [DoNotAudit]
    public string? SecurityStamp { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Example showing a class with mostly sensitive data where only specific properties are audited.
/// </summary>
public class ApiKeyExample
{
    public int Id { get; set; }
    
    /// <summary>
    /// Name is explicitly included in audit trail.
    /// </summary>
    [Audit]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The actual API key is excluded from audit trail.
    /// </summary>
    [DoNotAudit]
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Secret is excluded from audit trail.
    /// </summary>
    [DoNotAudit]
    public string Secret { get; set; } = string.Empty;
    
    /// <summary>
    /// Expiration date is explicitly included in audit trail.
    /// </summary>
    [Audit]
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// IsActive status is explicitly included in audit trail.
    /// </summary>
    [Audit]
    public bool IsActive { get; set; }
}
