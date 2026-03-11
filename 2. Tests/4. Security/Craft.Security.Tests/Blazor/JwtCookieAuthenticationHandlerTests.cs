using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Craft.Security.Tests.Blazor;

public class JwtCookieAuthenticationHandlerTests
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private const string SigningKey = "test-signing-key-at-least-32-bytes!";

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static string CreateToken(IEnumerable<Claim>? claims = null, DateTime? expires = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims ?? [],
            expires: expires ?? DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return TokenHandler.WriteToken(token);
    }

    private static string CreateTokenWithoutExpiry(IEnumerable<Claim>? claims = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // No 'expires' — jwt.ValidTo will be DateTime.MinValue
        var token = new JwtSecurityToken(
            claims: claims ?? [],
            signingCredentials: creds);

        return TokenHandler.WriteToken(token);
    }

    private static async Task<(JwtCookieAuthenticationHandler Handler, DefaultHttpContext Context)> BuildHandlerAsync(
        string? cookieValue,
        JwtCookieAuthenticationOptions? optionsOverride = null)
    {
        var opts = optionsOverride ?? new JwtCookieAuthenticationOptions();

        var optionsMonitor = new Mock<IOptionsMonitor<JwtCookieAuthenticationOptions>>();
        optionsMonitor.Setup(o => o.Get(JwtCookieAuthenticationOptions.SchemeName)).Returns(opts);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

        var handler = new JwtCookieAuthenticationHandler(
            optionsMonitor.Object,
            loggerFactory.Object,
            UrlEncoder.Default);

        var context = new DefaultHttpContext();

        if (cookieValue is not null)
            context.Request.Headers.Cookie = $"{opts.CookieName}={cookieValue}";

        var scheme = new AuthenticationScheme(
            JwtCookieAuthenticationOptions.SchemeName,
            null,
            typeof(JwtCookieAuthenticationHandler));

        await handler.InitializeAsync(scheme, context);

        return (handler, context);
    }

    // -----------------------------------------------------------------------
    // HandleAuthenticateAsync — cookie / token absent
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AuthenticateAsync_Should_ReturnNoResult_WhenNoCookiePresent()
    {
        // Arrange
        var (handler, _) = await BuildHandlerAsync(cookieValue: null);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.None);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_ReturnNoResult_WhenCookieIsEmpty()
    {
        // Arrange
        var (handler, _) = await BuildHandlerAsync(cookieValue: string.Empty);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.None);
    }

    // -----------------------------------------------------------------------
    // HandleAuthenticateAsync — valid token
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AuthenticateAsync_Should_ReturnSuccess_WhenValidTokenPresent()
    {
        // Arrange
        var claims = new[] { new Claim("sub", "user-42") };
        var (handler, _) = await BuildHandlerAsync(CreateToken(claims));

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("user-42", result.Principal?.FindFirst("sub")?.Value);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_IncludeAllClaimsFromToken()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "99"),
            new Claim("email", "user@example.com"),
            new Claim("role", "Admin")
        };
        var (handler, _) = await BuildHandlerAsync(CreateToken(claims));

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.Equal("99", result.Principal?.FindFirst("sub")?.Value);
        Assert.Equal("user@example.com", result.Principal?.FindFirst("email")?.Value);
        Assert.Equal("Admin", result.Principal?.FindFirst("role")?.Value);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_ReturnSuccess_WhenTokenHasNoExpiry()
    {
        // Arrange — ValidTo == DateTime.MinValue; handler must not reject it
        var (handler, _) = await BuildHandlerAsync(CreateTokenWithoutExpiry());

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    // -----------------------------------------------------------------------
    // HandleAuthenticateAsync — invalid / expired token
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AuthenticateAsync_Should_ReturnFail_WhenTokenIsExpired()
    {
        // Arrange
        var (handler, _) = await BuildHandlerAsync(CreateToken(expires: DateTime.UtcNow.AddHours(-1)));

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.NotNull(result.Failure);
        Assert.Contains("expired", result.Failure.Message);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_ReturnFail_WhenTokenIsMalformed()
    {
        // Arrange
        var (handler, _) = await BuildHandlerAsync("not.a.valid.jwt");

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.NotNull(result.Failure);
        Assert.Contains("Invalid token", result.Failure.Message);
    }

    // -----------------------------------------------------------------------
    // HandleChallengeAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ChallengeAsync_Should_RedirectToLoginPath()
    {
        // Arrange
        var (handler, context) = await BuildHandlerAsync(cookieValue: null);

        // Act
        await handler.ChallengeAsync(null);

        // Assert
        var location = context.Response.Headers.Location.ToString();
        Assert.StartsWith("/login", location);
    }

    [Fact]
    public async Task ChallengeAsync_Should_IncludeUrlEncodedReturnUrl()
    {
        // Arrange
        var (handler, context) = await BuildHandlerAsync(cookieValue: null);
        context.Request.Path = "/protected/page";
        context.Request.QueryString = new QueryString("?foo=bar");

        // Act
        await handler.ChallengeAsync(null);

        // Assert
        var location = context.Response.Headers.Location.ToString();
        Assert.Equal("/login?returnUrl=%2Fprotected%2Fpage%3Ffoo%3Dbar", location);
    }

    // -----------------------------------------------------------------------
    // HandleForbiddenAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ForbidAsync_Should_RedirectToAccessDeniedPath()
    {
        // Arrange
        var (handler, context) = await BuildHandlerAsync(cookieValue: null);

        // Act
        await handler.ForbidAsync(null);

        // Assert
        var location = context.Response.Headers.Location.ToString();
        Assert.Equal("/access-denied", location);
    }

    [Fact]
    public async Task ForbidAsync_Should_UseConfiguredAccessDeniedPath()
    {
        // Arrange
        var opts = new JwtCookieAuthenticationOptions { AccessDeniedPath = "/forbidden" };
        var (handler, context) = await BuildHandlerAsync(cookieValue: null, optionsOverride: opts);

        // Act
        await handler.ForbidAsync(null);

        // Assert
        var location = context.Response.Headers.Location.ToString();
        Assert.Equal("/forbidden", location);
    }
}
