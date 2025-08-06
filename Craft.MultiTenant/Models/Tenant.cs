using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.MultiTenant;

[Table("TN_Tenants")]
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Identifier), IsUnique = true)]
public class Tenant<TKey> : BaseEntity<TKey>, ITenant<TKey>
{
    public const int AdminEmailSize = 100;
    public const int ConnectionstringSize = 250;
    public const int DbPrviderSize = 10;
    public const int IdentifierSize = 20;
    public const int LogoUriSize = 200;
    public const int NameSize = 50;

    [MaxLength(AdminEmailSize)]
    public string AdminEmail { get; set; } = string.Empty;

    [MaxLength(ConnectionstringSize)]
    public string ConnectionString { get; set; } = string.Empty;

    [MaxLength(DbPrviderSize)]
    public string DbProvider { get; set; } = string.Empty;

    [MaxLength(IdentifierSize)]
    public string Identifier { get; set; } = null!;

    public bool IsActive { get; set; }

    [MaxLength(LogoUriSize)]
    public string LogoUri { get; set; } = string.Empty;

    [Required]
    [MaxLength(NameSize)]
    public string Name { get; set; } = string.Empty;

    public TenantType Type { get; set; }

    public DateTime ValidUpTo { get; set; } = DateTime.UtcNow.AddMonths(1);

    public Tenant() { }

    public Tenant(TKey id, string name, string connectionString, string identifier, string adminEmail)
    {
        Id = id;
        Name = name;
        ConnectionString = connectionString ?? string.Empty;
        Identifier = identifier;
        AdminEmail = adminEmail ?? string.Empty;

        IsActive = true;
        ValidUpTo = DateTime.UtcNow.AddMonths(12);
    }

    public Tenant(TKey id, string name, string connectionString, string identifier)
        : this(id, name, connectionString, identifier, string.Empty) { }

    public Tenant(string name, string connectionString, string identifier, string logoUri, string adminEmail)
        : this(default(TKey)!, name, connectionString, identifier, adminEmail)
    {
        LogoUri = logoUri;
    }

    public Tenant(TKey id, string name, string identifier, string logoUri, TenantType type)
        : this(id, name, string.Empty, identifier, string.Empty)
    {
        LogoUri = logoUri;
        Type = type;
    }

    public void Activate()
    {
        if (Type == TenantType.Host)
            throw new InvalidOperationException("Host tenant cannot be activated");

        IsActive = true;
    }

    public void AddValidity(int months) =>
            ValidUpTo = ValidUpTo.AddMonths(months);

    public void Deactivate()
    {
        if (Type == TenantType.Host)
            throw new InvalidOperationException("Host tenant cannot be deactivated");

        IsActive = false;
    }

    public void SetValidity(DateTime validUpTo) =>
            ValidUpTo = ValidUpTo < validUpTo
            ? validUpTo
            : throw new Exception("Subscription cannot be backdated");
}

public class Tenant : Tenant<KeyType>, ITenant
{
    public Tenant() { }

    public Tenant(KeyType id, string name, string connectionString, string identifier)
        : base(id, name, connectionString, identifier) { }

    public Tenant(KeyType id, string name, string connectionString, string identifier, string adminEmail)
        : base(id, name, connectionString, identifier, adminEmail) { }

    public Tenant(string name, string connectionString, string identifier, string logoUri, string adminEmail)
        : base(name, connectionString, identifier, logoUri, adminEmail) { }

    public Tenant(KeyType id, string name, string identifier, string logoUri, TenantType type)
        : base(id, name, identifier, logoUri, type) { }
}
