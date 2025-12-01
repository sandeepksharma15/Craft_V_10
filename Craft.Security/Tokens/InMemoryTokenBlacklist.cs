using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Craft.Security.Tokens;

/// <summary>
/// In-memory implementation of token blacklist. Suitable for single-server deployments.
/// For distributed systems, implement a Redis or database-backed version.
/// </summary>
public class InMemoryTokenBlacklist : ITokenBlacklist
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();
    private readonly ILogger<InMemoryTokenBlacklist> _logger;

    public InMemoryTokenBlacklist(ILogger<InMemoryTokenBlacklist> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task AddAsync(string token, DateTime expiration, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));

        var tokenHash = ComputeTokenHash(token);
        
        if (_blacklistedTokens.TryAdd(tokenHash, expiration))
        {
            _logger.LogInformation("Token added to blacklist. Expiration: {Expiration}", expiration);
        }
        else
        {
            _logger.LogWarning("Token already exists in blacklist");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> IsBlacklistedAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        var tokenHash = ComputeTokenHash(token);
        var isBlacklisted = _blacklistedTokens.ContainsKey(tokenHash);

        if (isBlacklisted)
            _logger.LogDebug("Token found in blacklist");

        return Task.FromResult(isBlacklisted);
    }

    /// <inheritdoc/>
    public Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredTokens = _blacklistedTokens
            .Where(kvp => kvp.Value < now)
            .Select(kvp => kvp.Key)
            .ToList();

        var removedCount = 0;
        foreach (var token in expiredTokens)
        {
            if (_blacklistedTokens.TryRemove(token, out _))
                removedCount++;
        }

        if (removedCount > 0)
            _logger.LogInformation("Removed {Count} expired tokens from blacklist", removedCount);

        return Task.FromResult(removedCount);
    }

    private static string ComputeTokenHash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}
