using System.ComponentModel.DataAnnotations.Schema;
using Craft.Auditing;
using Craft.Domain;

namespace Craft.Security;

public class LoginHistory<TKey> : ILoginHistory<TKey> where TKey : IEquatable<TKey>
{
    public LoginHistory() { }

    public LoginHistory(string lastIpAddress, DateTime? lastLoginOn, TKey userId)
    {
        LastIpAddress = lastIpAddress;
        LastLoginOn = lastLoginOn;
        UserId = userId;
    }

    public TKey Id { get; set; } = default!;

    // IsDeleted is not used in this class, but is required as there is a GlobalQueryFilter
    // On CraftUser that requires it.
    public bool IsDeleted { get; set; }

    public string? LastIpAddress { get; set; }

    public DateTime? LastLoginOn { get; set; }

    public string? Provider { get; set; }

    [ForeignKey("UserId")]
    public TKey UserId { get; set; } = default!;

}

[DoNotAudit]
[Table("ID_LoginHistory")]
public class LoginHistory : LoginHistory<KeyType>, IEntity, IModel;
