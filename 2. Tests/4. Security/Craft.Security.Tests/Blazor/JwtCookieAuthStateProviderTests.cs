using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Craft.Security.Tests.Blazor;

public class JwtCookieAuthStateProviderTests
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static string CreateToken(IEnumerable<Claim>? claims = null, DateTime? expires = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-signing-key-at-least-32-bytes!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims ?? [],
            expires: expires ?? DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return TokenHandler.WriteToken(token);
    }

    private static JwtCookieAuthStateProvider BuildProvider(string? cookieValue)
    {
        var cookiesMock = new Mock<IRequestCookieCollection>();
        cookiesMock.Setup(c => c["BearerToken"]).Returns(cookieValue);

        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(r => r.Cookies).Returns(cookiesMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        return new JwtCookieAuthStateProvider(accessorMock.Object);
    }

    private static JwtCookieAuthStateProvider BuildProvider(ClaimsPrincipal user)
    {
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.User).Returns(user);

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        return new JwtCookieAuthStateProvider(accessorMock.Object);
    }

    // -----------------------------------------------------------------------
    // GetAuthenticationStateAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAuthenticationStateAsync_Should_ReturnAnonymous_WhenNoCookiePresent()
    {
        // Arrange
        var provider = BuildProvider(cookieValue: null);

        // Act
        var result = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(result.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_Should_ReturnAnonymous_WhenCookieIsEmpty()
    {
        // Arrange
        var provider = BuildProvider(cookieValue: string.Empty);

        // Act
        var result = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(result.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_Should_ReturnAuthenticated_WhenValidTokenPresent()
    {
        // Arrange
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim("sub", "user-42"), new Claim("name", "Test User")],
                JwtBearerDefaults.AuthenticationScheme));
        var provider = BuildProvider(user);

        // Act
        var result = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.True(result.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_Should_SetJwtBearerAuthenticationType_WhenValidTokenPresent()
    {
        // Arrange
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim("sub", "user-42")],
                JwtBearerDefaults.AuthenticationScheme));
        var provider = BuildProvider(user);

        // Act
        var result = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, result.User.Identity?.AuthenticationType);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_Should_ReturnAnonymous_WhenTokenIsExpired()
    {
        // Arrange
        var provider = BuildProvider(CreateToken(expires: DateTime.UtcNow.AddHours(-1)));

        // Act
        var result = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(result.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_Should_ReturnAnonymous_WhenTokenIsMalformed()
    {
        // Arrange
        var provider = BuildProvider(cookieValue: "not.a.valid.jwt");

        // Act
        var result = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(result.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_Should_ReturnAnonymous_WhenHttpContextIsNull()
    {
        // Arrange
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var provider = new JwtCookieAuthStateProvider(accessorMock.Object);

        // Act
        var result = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(result.User.Identity?.IsAuthenticated);
    }

    [Theory]
    [InlineData("sub", "99")]
    [InlineData("email", "user@example.com")]
    [InlineData("role", "Admin")]
    public async Task GetAuthenticationStateAsync_Should_IncludeClaimFromToken(string claimType, string expectedValue)
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "99"),
            new Claim("email", "user@example.com"),
            new Claim("role", "Admin")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme));
        var provider = BuildProvider(user);

        // Act
        var result = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.Equal(expectedValue, result.User.FindFirst(claimType)?.Value);
    }

    // -----------------------------------------------------------------------
    // NotifyAuthChanged
    // -----------------------------------------------------------------------

    [Fact]
    public void NotifyAuthChanged_Should_FireAuthenticationStateChangedEvent()
    {
        // Arrange
        var provider = BuildProvider(cookieValue: null);

        AuthenticationState? notifiedState = null;
        provider.AuthenticationStateChanged += state => notifiedState = state.GetAwaiter().GetResult();

        var user = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, "updated-user")], "Bearer"));

        // Act
        provider.NotifyAuthChanged(user);

        // Assert
        Assert.NotNull(notifiedState);
        Assert.True(notifiedState.User.Identity?.IsAuthenticated);
        Assert.Equal("updated-user", notifiedState.User.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public void NotifyAuthChanged_Should_FireWithAnonymousPrincipal_WhenSigningOut()
    {
        // Arrange
        var provider = BuildProvider(cookieValue: null);

        AuthenticationState? notifiedState = null;
        provider.AuthenticationStateChanged += state => notifiedState = state.GetAwaiter().GetResult();

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        provider.NotifyAuthChanged(anonymous);

        // Assert
        Assert.NotNull(notifiedState);
        Assert.False(notifiedState.User.Identity?.IsAuthenticated);
    }

    // -----------------------------------------------------------------------
    // Type checks
    // -----------------------------------------------------------------------

    [Fact]
    public void Provider_Should_ImplementAuthenticationStateProvider()
    {
        // Arrange
        var accessorMock = new Mock<IHttpContextAccessor>();
        var provider = new JwtCookieAuthStateProvider(accessorMock.Object);

        // Assert
        Assert.IsType<AuthenticationStateProvider>(provider, exactMatch: false);
    }
}
