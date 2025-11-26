using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Craft.Security.Tokens;

public class TokenManager(IOptions<JwtSettings> options) : ITokenManager
{
    private readonly JwtSettings jwtSettings = options.Value;

    public static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public JwtAuthResponse GenerateJwtTokens(IEnumerable<Claim> claims)
    {
        return new JwtAuthResponse(GenerateTokens(claims),
            GenerateRefreshToken(),
            DateTime.Now.AddDays(jwtSettings.RefreshTokenExpirationInDays));
    }

    public string GenerateTokens(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
                    issuer: jwtSettings.ValidIssuer,
                    audience: jwtSettings.ValidAudiences?.FirstOrDefault() ?? string.Empty,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(jwtSettings.TokenExpirationInMinutes),
                    signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        SecurityToken securityToken;
        ClaimsPrincipal principal;

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = jwtSettings.ValidateAudience,
            ValidateIssuer = jwtSettings.ValidateIssuer,
            ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey)),
            ValidateLifetime = jwtSettings.ValidateLifetime,
            ValidAudiences = jwtSettings.ValidAudiences,
            ValidIssuer = jwtSettings.ValidIssuer
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            principal = tokenHandler.ValidateToken(token,
                tokenValidationParameters,
                out securityToken);
        }
        catch (Exception)
        {
            throw;
        }

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    public RefreshToken GetRefreshToken(CraftUser user)
    {
        return new RefreshToken
        {
            UserId = user.Id,
            Token = GenerateRefreshToken(),
            ExpiryTime = DateTime.Now.AddDays(jwtSettings.RefreshTokenExpirationInDays)
        };
    }

    public bool ValidateToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = jwtSettings.ValidateAudience,
            ValidateIssuer = jwtSettings.ValidateIssuer,
            ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey)),
            ValidateLifetime = jwtSettings.ValidateLifetime
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
        }
        catch
        {
            return false;
        }

        return true;
    }
}
