using System.Security.Claims;

namespace Craft.Security;

public interface ICurrentUser<TKey>
{
    public TKey Id { get; }

    public ClaimsPrincipal? GetUser();

    public string? Name { get; }

    public string? GetEmail();

    public string? GetFirstName();

    public string? GetFullName();

    public TKey GetId();

    public string? GetImageUrl();

    public string? GetJwtToken();

    public string? GetLastName();

    public string? GetPermissions();

    public string? GetPhoneNumber();

    public string? GetRole();

    public string? GetTenant();

    IEnumerable<Claim> GetUserClaims();

    public string? GetUserName() => GetEmail();

    bool IsAuthenticated();

    bool IsInRole(string role);

    void SetCurrentUserId(TKey id);
}

public interface ICurrentUser : ICurrentUser<KeyType>;
