using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Craft.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Craft.Security.Tokens;

/// <summary>
/// Manages JWT token generation, validation, and revocation.
/// </summary>
public class TokenManager : ITokenManager, IScopedDependency
{
    private readonly JwtSettings _jwtSettings;
    private readonly TimeProvider _timeProvider;
    private readonly ITokenBlacklist _tokenBlacklist;
    private readonly ILogger<TokenManager> _logger;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly SigningCredentials _signingCredentials;

    public TokenManager(
        IOptions<JwtSettings> options,
        TimeProvider timeProvider,
        ITokenBlacklist tokenBlacklist,
        ILogger<TokenManager> logger)
    {
        _jwtSettings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _tokenBlacklist = tokenBlacklist ?? throw new ArgumentNullException(nameof(tokenBlacklist));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.IssuerSigningKey));
        _signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
    }

    /// <inheritdoc/>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        
        var token = Convert.ToBase64String(randomNumber);
        _logger.LogDebug("Generated new refresh token");
        
        return token;
    }

    /// <inheritdoc/>
    public JwtAuthResponse GenerateJwtTokens(IEnumerable<Claim> claims)
    {
        if (claims == null || !claims.Any())
            throw new ArgumentException("Claims cannot be null or empty.", nameof(claims));

        var now = _timeProvider.GetUtcNow();
        var accessToken = GenerateAccessToken(claims);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = now.AddDays(_jwtSettings.RefreshTokenExpirationInDays).UtcDateTime;

        _logger.LogInformation("Generated JWT tokens for user");

        return new JwtAuthResponse(accessToken, refreshToken, refreshTokenExpiry);
    }

    /// <inheritdoc/>
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        if (claims == null || !claims.Any())
            throw new ArgumentException("Claims cannot be null or empty.", nameof(claims));

        var now = _timeProvider.GetUtcNow();
        var expires = now.AddMinutes(_jwtSettings.TokenExpirationInMinutes);

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.ValidIssuer,
            audience: _jwtSettings.ValidAudiences?.FirstOrDefault() ?? string.Empty,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: _signingCredentials);

        var tokenString = tokenHandler.WriteToken(token);
        
        _logger.LogDebug("Generated access token. Expires: {Expiration}", expires);
        
        return tokenString;
    }

    /// <inheritdoc/>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));

        var tokenValidationParameters = CreateTokenValidationParameters(validateLifetime: false);
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid token algorithm");
                throw new SecurityTokenException("Invalid token");
            }

            _logger.LogDebug("Successfully extracted principal from expired token");
            return principal;
        }
        catch (Exception ex) when (ex is not SecurityTokenException)
        {
            _logger.LogError(ex, "Failed to extract principal from expired token");
            throw new SecurityTokenException("Failed to extract principal from token", ex);
        }
    }

    /// <inheritdoc/>
    public RefreshToken GetRefreshToken(CraftUser user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var now = _timeProvider.GetUtcNow();

        return new RefreshToken
        {
            UserId = user.Id,
            Token = GenerateRefreshToken(),
            ExpiryTime = now.AddDays(_jwtSettings.RefreshTokenExpirationInDays).UtcDateTime
        };
    }

    /// <inheritdoc/>
    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogDebug("Token validation failed: token is null or empty");
            return false;
        }

        var tokenValidationParameters = CreateTokenValidationParameters(validateLifetime: true);
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            _logger.LogDebug("Token validation successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token validation failed");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!ValidateToken(token))
            return false;

        var isRevoked = await IsTokenRevokedAsync(token, cancellationToken);
        
        if (isRevoked)
            _logger.LogWarning("Token validation failed: token has been revoked");

        return !isRevoked;
    }

    /// <inheritdoc/>
    public IEnumerable<Claim> GetTokenClaims(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return [];

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract claims from token");
            return [];
        }
    }

    /// <inheritdoc/>
    public DateTime? GetTokenExpiration(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract expiration from token");
            return null;
        }
    }

    /// <inheritdoc/>
    public bool IsTokenExpired(string token)
    {
        var expiration = GetTokenExpiration(token);
        
        if (!expiration.HasValue)
            return true;

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        return expiration.Value < now;
    }

    /// <inheritdoc/>
    public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));

        var expiration = GetTokenExpiration(token);
        
        if (!expiration.HasValue)
        {
            _logger.LogWarning("Cannot revoke token: unable to determine expiration");
            throw new SecurityTokenException("Invalid token: cannot determine expiration");
        }

        await _tokenBlacklist.AddAsync(token, expiration.Value, cancellationToken);
        _logger.LogInformation("Token revoked successfully");
    }

    /// <inheritdoc/>
    public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        return await _tokenBlacklist.IsBlacklistedAsync(token, cancellationToken);
    }

    private TokenValidationParameters CreateTokenValidationParameters(bool validateLifetime)
    {
        return new TokenValidationParameters
        {
            ValidateAudience = _jwtSettings.ValidateAudience,
            ValidateIssuer = _jwtSettings.ValidateIssuer,
            ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = _signingKey,
            ValidateLifetime = validateLifetime,
            ValidAudiences = _jwtSettings.ValidAudiences,
            ValidIssuer = _jwtSettings.ValidIssuer,
            ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkew),
            LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
            {
                if (!validateLifetime)
                    return true;

                var now = _timeProvider.GetUtcNow().UtcDateTime;
                
                if (notBefore.HasValue && now < notBefore.Value)
                    return false;

                if (expires.HasValue && now > expires.Value.Add(validationParameters.ClockSkew))
                    return false;

                return true;
            }
        };
    }
}
