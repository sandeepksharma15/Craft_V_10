using System.Security.Claims;
using Craft.Security.Claims;

namespace Craft.Security;

public static class ClaimsPrincipalExtensions
{
    public static string? GetEmail(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.Email);

    public static string? GetFirstName(this ClaimsPrincipal principal)
        => principal?.FindFirst(ClaimTypes.Name)?.Value;

    public static string? GetFullName(this ClaimsPrincipal principal)
        => principal?.FindFirst(CraftClaims.Fullname)?.Value;

    public static string? GetImageUrl(this ClaimsPrincipal principal)
        => principal.FindFirstValue(CraftClaims.ImageUrl);

    public static string? GetJwtToken(this ClaimsPrincipal principal)
        => principal.FindFirstValue(CraftClaims.JwtToken);

    public static string? GetLastName(this ClaimsPrincipal principal)
            => principal?.FindFirst(ClaimTypes.Surname)?.Value;

    public static string? GetMobileNumber(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.MobilePhone);

    public static string? GetPermissions(this ClaimsPrincipal principal)
        => principal.FindFirstValue(CraftClaims.Permissions);

    public static string? GetRole(this ClaimsPrincipal principal)
        => principal.FindFirstValue(CraftClaims.Role);

    public static string? GetTenant(this ClaimsPrincipal principal)
        => principal.FindFirstValue(CraftClaims.Tenant);

    public static string? GetUserId(this ClaimsPrincipal principal)
       => principal.FindFirstValue(ClaimTypes.NameIdentifier);
}
