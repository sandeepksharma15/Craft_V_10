using System.Security.Claims;

namespace Craft.Security.Claims;

public static class CraftClaims
{
    public const string Fullname = "fullName";
    public const string Id = ClaimTypes.NameIdentifier;
    public const string ImageUrl = "imageurl";
    public const string JwtToken = "JwtToken";
    public const string Permissions = "permissions";
    public const string Role = "role";
    public const string Tenant = "tenant";
    public const string UserId = ClaimTypes.NameIdentifier;
}
