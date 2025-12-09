using System.Security.Claims;

namespace Craft.Security.Tokens;

/// <summary>
/// Provides JWT token management capabilities including generation, validation, and revocation.
/// </summary>
public interface ITokenManager
{
    /// <summary>
    /// Generates a complete JWT authentication response including access token, refresh token, and expiry time.
    /// </summary>
    /// <param name="claims">The claims to include in the JWT token.</param>
    /// <returns>A <see cref="JwtAuthResponse"/> containing the tokens and expiry information.</returns>
    JwtAuthResponse GenerateJwtTokens(IEnumerable<Claim> claims);

    /// <summary>
    /// Generates a JWT access token from the provided claims.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <returns>The generated JWT token string.</returns>
    string GenerateAccessToken(IEnumerable<Claim> claims);

    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    /// <returns>A base64-encoded refresh token string.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Retrieves the claims principal from an expired JWT token without validating its lifetime.
    /// </summary>
    /// <param name="token">The expired JWT token.</param>
    /// <returns>The <see cref="ClaimsPrincipal"/> extracted from the token.</returns>
    /// <exception cref="SecurityTokenException">Thrown when the token is invalid or malformed.</exception>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Creates a refresh token entity for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to create the refresh token.</param>
    /// <returns>A <see cref="RefreshToken"/> entity.</returns>
    RefreshToken GetRefreshToken(CraftUser user);

    /// <summary>
    /// Validates a JWT token and returns whether it is valid.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns><c>true</c> if the token is valid; otherwise, <c>false</c>.</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Extracts claims from a JWT token without full validation (useful for introspection).
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>A collection of claims from the token, or empty if invalid.</returns>
    IEnumerable<Claim> GetTokenClaims(string token);

    /// <summary>
    /// Gets the expiration time of a JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>The expiration <see cref="DateTime"/> in UTC, or null if invalid.</returns>
    DateTime? GetTokenExpiration(string token);

    /// <summary>
    /// Checks if a JWT token is expired.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns><c>true</c> if the token is expired; otherwise, <c>false</c>.</returns>
    bool IsTokenExpired(string token);

    /// <summary>
    /// Revokes a JWT token by adding it to the blacklist.
    /// </summary>
    /// <param name="token">The JWT token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a JWT token has been revoked.
    /// </summary>
    /// <param name="token">The JWT token to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the token is revoked; otherwise, <c>false</c>.</returns>
    Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a token and checks if it has been revoked.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the token is valid and not revoked; otherwise, <c>false</c>.</returns>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}
