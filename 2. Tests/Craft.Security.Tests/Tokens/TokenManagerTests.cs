using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Craft.Security.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Craft.Security.Tests.Tokens;

public class TokenManagerTests
{
    private readonly Mock<ITokenBlacklist> _mockBlacklist;
    private readonly Mock<ILogger<TokenManager>> _mockLogger;
    private readonly JwtSettings _jwtSettings;
    private readonly FakeTimeProvider _timeProvider;
    private readonly TokenManager _tokenManager;

    public TokenManagerTests()
    {
        _mockBlacklist = new Mock<ITokenBlacklist>();
        _mockLogger = new Mock<ILogger<TokenManager>>();
        
        _jwtSettings = new JwtSettings
        {
            IssuerSigningKey = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456",
            ValidIssuer = "TestIssuer",
            ValidAudiences = ["TestAudience"],
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = 5
        };

        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        
        var options = Microsoft.Extensions.Options.Options.Create(_jwtSettings);
        _tokenManager = new TokenManager(options, _timeProvider, _mockBlacklist.Object, _mockLogger.Object);
    }

    [Fact]
    public void GenerateRefreshToken_Should_Return_Base64String()
    {
        // Act
        var token = _tokenManager.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(IsBase64String(token));
    }

    [Fact]
    public void GenerateRefreshToken_Should_Generate_UniqueTokens()
    {
        // Act
        var token1 = _tokenManager.GenerateRefreshToken();
        var token2 = _tokenManager.GenerateRefreshToken();

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateAccessToken_Should_Return_ValidJwtToken()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };

        // Act
        var token = _tokenManager.GenerateAccessToken(claims);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.Equal(_jwtSettings.ValidIssuer, jwtToken.Issuer);
        Assert.Contains(_jwtSettings.ValidAudiences![0], jwtToken.Audiences);
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == "testuser");
    }

    [Fact]
    public void GenerateAccessToken_Should_ThrowException_When_ClaimsAreNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenManager.GenerateAccessToken(null!));
    }

    [Fact]
    public void GenerateAccessToken_Should_ThrowException_When_ClaimsAreEmpty()
    {
        // Arrange
        var claims = Array.Empty<Claim>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenManager.GenerateAccessToken(claims));
    }

    [Fact]
    public void GenerateJwtTokens_Should_Return_CompleteAuthResponse()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };

        // Act
        var response = _tokenManager.GenerateJwtTokens(claims);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.JwtToken);
        Assert.NotNull(response.RefreshToken);
        Assert.True(response.RefreshTokenExpiryTime > _timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void ValidateToken_Should_Return_True_ForValidToken()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var token = _tokenManager.GenerateAccessToken(claims);

        // Act
        var isValid = _tokenManager.ValidateToken(token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateToken_Should_Return_False_ForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = _tokenManager.ValidateToken(invalidToken);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_Should_Return_False_ForNullToken()
    {
        // Act
        var isValid = _tokenManager.ValidateToken(null!);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_Should_Return_False_ForExpiredToken()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var token = _tokenManager.GenerateAccessToken(claims);

        // Advance time beyond token expiration
        _timeProvider.Advance(TimeSpan.FromMinutes(_jwtSettings.TokenExpirationInMinutes + 10));

        // Act
        var isValid = _tokenManager.ValidateToken(token);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_Should_Return_Principal_ForExpiredToken()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var token = _tokenManager.GenerateAccessToken(claims);

        // Advance time to expire token
        _timeProvider.Advance(TimeSpan.FromMinutes(_jwtSettings.TokenExpirationInMinutes + 10));

        // Act
        var principal = _tokenManager.GetPrincipalFromExpiredToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal("testuser", principal.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_Should_ThrowException_ForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act & Assert
        Assert.Throws<SecurityTokenException>(() => _tokenManager.GetPrincipalFromExpiredToken(invalidToken));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_Should_ThrowException_ForNullToken()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenManager.GetPrincipalFromExpiredToken(null!));
    }

    [Fact]
    public void GetRefreshToken_Should_Return_RefreshTokenEntity()
    {
        // Arrange
        var user = new CraftUser { Id = 123, UserName = "testuser" };

        // Act
        var refreshToken = _tokenManager.GetRefreshToken(user);

        // Assert
        Assert.NotNull(refreshToken);
        Assert.Equal(user.Id, refreshToken.UserId);
        Assert.NotNull(refreshToken.Token);
        Assert.True(refreshToken.ExpiryTime > _timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void GetRefreshToken_Should_ThrowException_ForNullUser()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _tokenManager.GetRefreshToken(null!));
    }

    [Fact]
    public void GetTokenClaims_Should_Return_Claims_FromValidToken()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var token = _tokenManager.GenerateAccessToken(claims);

        // Act
        var extractedClaims = _tokenManager.GetTokenClaims(token).ToList();

        // Assert
        Assert.NotEmpty(extractedClaims);
        Assert.Contains(extractedClaims, c => c.Type == ClaimTypes.Name && c.Value == "testuser");
        Assert.Contains(extractedClaims, c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
    }

    [Fact]
    public void GetTokenClaims_Should_Return_EmptyCollection_ForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var claims = _tokenManager.GetTokenClaims(invalidToken);

        // Assert
        Assert.Empty(claims);
    }

    [Fact]
    public void GetTokenExpiration_Should_Return_ExpirationTime()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var token = _tokenManager.GenerateAccessToken(claims);

        // Act
        var expiration = _tokenManager.GetTokenExpiration(token);

        // Assert
        Assert.NotNull(expiration);
        var expectedExpiration = _timeProvider.GetUtcNow().AddMinutes(_jwtSettings.TokenExpirationInMinutes).UtcDateTime;
        Assert.Equal(expectedExpiration, expiration.Value, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetTokenExpiration_Should_Return_Null_ForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var expiration = _tokenManager.GetTokenExpiration(invalidToken);

        // Assert
        Assert.Null(expiration);
    }

    [Fact]
    public void IsTokenExpired_Should_Return_False_ForValidToken()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var token = _tokenManager.GenerateAccessToken(claims);

        // Act
        var isExpired = _tokenManager.IsTokenExpired(token);

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void IsTokenExpired_Should_Return_True_ForExpiredToken()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var token = _tokenManager.GenerateAccessToken(claims);

        // Advance time beyond expiration
        _timeProvider.Advance(TimeSpan.FromMinutes(_jwtSettings.TokenExpirationInMinutes + 10));

        // Act
        var isExpired = _tokenManager.IsTokenExpired(token);

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public async Task RevokeTokenAsync_Should_AddTokenToBlacklist()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var token = _tokenManager.GenerateAccessToken(claims);

        // Act
        await _tokenManager.RevokeTokenAsync(token);

        // Assert
        _mockBlacklist.Verify(b => b.AddAsync(token, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeTokenAsync_Should_ThrowException_ForNullToken()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _tokenManager.RevokeTokenAsync(null!));
    }

    [Fact]
    public async Task IsTokenRevokedAsync_Should_Return_True_ForRevokedToken()
    {
        // Arrange
        var token = "some.token.value";
        _mockBlacklist.Setup(b => b.IsBlacklistedAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var isRevoked = await _tokenManager.IsTokenRevokedAsync(token);

        // Assert
        Assert.True(isRevoked);
    }

    [Fact]
    public async Task IsTokenRevokedAsync_Should_Return_False_ForNonRevokedToken()
    {
        // Arrange
        var token = "some.token.value";
        _mockBlacklist.Setup(b => b.IsBlacklistedAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var isRevoked = await _tokenManager.IsTokenRevokedAsync(token);

        // Assert
        Assert.False(isRevoked);
    }

    [Fact]
    public async Task ValidateTokenAsync_Should_Return_False_ForRevokedToken()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var token = _tokenManager.GenerateAccessToken(claims);
        
        _mockBlacklist.Setup(b => b.IsBlacklistedAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var isValid = await _tokenManager.ValidateTokenAsync(token);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task ValidateTokenAsync_Should_Return_True_ForValidNonRevokedToken()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var token = _tokenManager.GenerateAccessToken(claims);
        
        _mockBlacklist.Setup(b => b.IsBlacklistedAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var isValid = await _tokenManager.ValidateTokenAsync(token);

        // Assert
        Assert.True(isValid);
    }

    private static bool IsBase64String(string value)
    {
        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Fake TimeProvider for testing time-dependent code.
/// </summary>
public class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _currentTime;

    public FakeTimeProvider(DateTimeOffset startTime)
    {
        _currentTime = startTime;
    }

    public override DateTimeOffset GetUtcNow() => _currentTime;

    public void Advance(TimeSpan timeSpan)
    {
        _currentTime = _currentTime.Add(timeSpan);
    }

    public void SetTime(DateTimeOffset time)
    {
        _currentTime = time;
    }
}
