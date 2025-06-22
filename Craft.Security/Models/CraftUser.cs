using System.ComponentModel.DataAnnotations.Schema;
using Craft.Domain;
using Microsoft.AspNetCore.Identity;

namespace Craft.Security;

public class CraftUser<TKey> : IdentityUser<TKey>, ICraftUser<TKey>
    where TKey : IEquatable<TKey>
{
    public static readonly string UserId = "UserId";

    public CraftUser() => SecurityStamp = Guid.NewGuid().ToString();

    public CraftUser(string? userName, string? email, string? firstName, string? lastName, string? dialCode,
        string? phoneNumber, string? password)
    {
        UserName = userName;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        DialCode = dialCode;
        PhoneNumber = phoneNumber;
        PasswordHash = password;
    }

    public string? DialCode { get; set; }

    public string? FirstName { get; set; }

    [NotMapped]
    public string FullName => (FirstName ?? string.Empty) + " " + (LastName ?? string.Empty);

    public GenderType Gender { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public string? LastName { get; set; }

    public HonorificType Title { get; set; }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}

public class CraftUser : CraftUser<KeyType>, ICraftUser;
