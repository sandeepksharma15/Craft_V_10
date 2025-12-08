using System.Security.Claims;
using Craft.Core;

namespace Craft.Security;

/// <summary>
/// Represents the current user information retrieved from the specified provider.
/// </summary>
/// <typeparam name="TKey">The type used to represent the user's ID.</typeparam>
public class CurrentUser<TKey> : ICurrentUser<TKey>
{
    public CurrentUser(ICurrentUserProvider currentUserProvider)
    {
        _user = currentUserProvider?.GetUser();

        Id = GetId();
    }

    protected ClaimsPrincipal? _user;

    public TKey Id { get; set; }

    public ClaimsPrincipal? GetUser() => _user;

    public string? Name => IsAuthenticated() ? _user?.GetFirstName() : string.Empty;

    public string? GetEmail() => IsAuthenticated() ? _user?.GetEmail() : string.Empty;

    public string? GetFirstName() => IsAuthenticated() ? _user?.GetFirstName() : string.Empty;

    public string? GetFullName() => IsAuthenticated() ? _user?.GetFullName() : string.Empty;

    public TKey GetId()
        => _user?.GetUserId() != null
                ? (TKey)Convert.ChangeType(_user.GetUserId(), typeof(TKey))!
                : default!;

    public TKey GetUserId() => GetId();

    public string? GetImageUrl() => IsAuthenticated() ? _user?.GetImageUrl() : string.Empty;

    public string? GetJwtToken() => IsAuthenticated() ? _user?.GetJwtToken() : string.Empty;

    public string? GetLastName() => IsAuthenticated() ? _user?.GetLastName() : string.Empty;

    public string? GetPermissions() => IsAuthenticated() ? _user?.GetPermissions() : string.Empty;

    public string? GetPhoneNumber() => IsAuthenticated() ? _user?.GetMobileNumber() : string.Empty;

    public string? GetRole() => IsAuthenticated() ? _user?.GetRole() : string.Empty;

    public string? GetTenant() => IsAuthenticated() ? _user?.GetTenant() : string.Empty;

    public IEnumerable<Claim> GetUserClaims()
        => IsAuthenticated()
            ? _user?.Claims ?? []
            : [];

    public bool IsAuthenticated() => _user?.Identity?.IsAuthenticated is true;

    public bool IsInRole(string role) => _user?.IsInRole(role) is true;

    public void SetCurrentUserId(TKey id) => Id = id;
}

public class CurrentUser(ICurrentUserProvider currentUserProvider)
    : CurrentUser<KeyType>(currentUserProvider), ICurrentUser
{ }
