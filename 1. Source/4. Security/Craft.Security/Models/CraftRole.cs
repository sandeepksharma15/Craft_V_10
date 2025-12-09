using Microsoft.AspNetCore.Identity;

namespace Craft.Security;

public class CraftRole<TKey> : IdentityRole<TKey>, ICraftRole<TKey>
    where TKey : IEquatable<TKey>
{
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    public CraftRole() { }

    public CraftRole(string? name, string? description = null)
    {
        Name = name;
        Description = description;
        NormalizedName = name?.ToUpperInvariant();
        IsActive = true;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}

public class CraftRole : CraftRole<KeyType>, ICraftRole
{
    public CraftRole() { }

    public CraftRole(string? name, string? description = null) : base(name, description) { }
}
