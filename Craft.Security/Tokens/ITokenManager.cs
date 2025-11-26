using System.Security.Claims;

namespace Craft.Security.Tokens;

public interface ITokenManager
{
    JwtAuthResponse GenerateJwtTokens(IEnumerable<Claim> claims);

    string GenerateTokens(IEnumerable<Claim> claims);

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    RefreshToken GetRefreshToken(CraftUser user);

    bool ValidateToken(string token);
}
