using System.Security.Claims;

namespace Craft.Security.Tokens;

/// <summary>
/// Provides an abstraction for managing revoked JWT tokens.
/// </summary>
public interface ITokenBlacklist
{
    /// <summary>
    /// Adds a token to the blacklist.
    /// </summary>
    /// <param name="token">The JWT token to blacklist.</param>
    /// <param name="expiration">The expiration time of the token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(string token, DateTime expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a token is in the blacklist.
    /// </summary>
    /// <param name="token">The JWT token to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the token is blacklisted; otherwise, <c>false</c>.</returns>
    Task<bool> IsBlacklistedAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired tokens from the blacklist.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of tokens removed.</returns>
    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
