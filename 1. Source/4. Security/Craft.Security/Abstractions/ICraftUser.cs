using Craft.Domain;

namespace Craft.Security;

public interface ICraftUser<TKey> : IEntity<TKey>, ISoftDelete, IHasConcurrency, IHasActive, IModel<TKey>
{
    public string? DialCode { get; set; }

    public string? Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? FirstName { get; set; }

    public GenderType Gender { get; set; }

    public string? ImageUrl { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public HonorificType Title { get; set; }

    public string? UserName { get; set; }
}

public interface ICraftUser : ICraftUser<KeyType>, IEntity, IModel;
