using System.Security.Claims;
using Craft.Security.Claims;
using Moq;

namespace Craft.Security.Tests.CurrentUserService;

public class CurrentUserTests
{
    [Fact]
    public void Constructor_SetsUserAndId_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim(ClaimTypes.NameIdentifier, "42"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal(principal, user.GetUser());
        Assert.Equal(42, user.Id);
    }

    [Fact]
    public void Constructor_SetsDefaultId_WhenNoUser()
    {
        var provider = MockProvider(null);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Null(user.GetUser());
        Assert.Equal(0, user.Id);
    }

    [Fact]
    public void Name_ReturnsFirstName_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim(ClaimTypes.Name, "John"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("John", user.Name);
    }

    [Fact]
    public void Name_ReturnsEmpty_WhenNotAuthenticated()
    {
        var principal = CreatePrincipal(false);
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal(string.Empty, user.Name);
    }

    [Fact]
    public void GetEmail_ReturnsEmail_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim(ClaimTypes.Email, "a@b.com"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("a@b.com", user.GetEmail());
    }

    [Fact]
    public void GetEmail_ReturnsEmpty_WhenNotAuthenticated()
    {
        var principal = CreatePrincipal(false);
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal(string.Empty, user.GetEmail());
    }

    [Fact]
    public void GetFirstName_ReturnsFirstName_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim(ClaimTypes.Name, "Jane"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("Jane", user.GetFirstName());
    }

    [Fact]
    public void GetFullName_ReturnsFullName_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim("fullname", "Jane Doe"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("Jane Doe", user.GetFullName());
    }

    [Fact]
    public void GetId_ReturnsId_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim(ClaimTypes.NameIdentifier, "123"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal(123, user.GetId());
    }

    [Fact]
    public void GetId_ReturnsDefault_WhenNoIdClaim()
    {
        var principal = CreatePrincipal(true);
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal(0, user.GetId());
    }

    [Fact]
    public void GetImageUrl_ReturnsImageUrl_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim(CraftClaims.ImageUrl, "http://img.com/u.png"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("http://img.com/u.png", user.GetImageUrl());
    }

    [Fact]
    public void GetJwtToken_ReturnsJwtToken_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim(CraftClaims.JwtToken, "token"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("token", user.GetJwtToken());
    }

    [Fact]
    public void GetLastName_ReturnsLastName_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim(ClaimTypes.Surname, "Smith"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("Smith", user.GetLastName());
    }

    [Fact]
    public void GetPermissions_ReturnsPermissions_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim("permissions", "read,write"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("read,write", user.GetPermissions());
    }

    [Fact]
    public void GetPhoneNumber_ReturnsPhoneNumber_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim(ClaimTypes.MobilePhone, "+1234567890"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("+1234567890", user.GetPhoneNumber());
    }

    [Fact]
    public void GetRole_ReturnsRole_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim("role", "admin"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("admin", user.GetRole());
    }

    [Fact]
    public void GetTenant_ReturnsTenant_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true, new Claim("tenant", "tenant-1"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Equal("tenant-1", user.GetTenant());
    }

    [Fact]
    public void GetUserClaims_ReturnsClaims_WhenAuthenticated()
    {
        // Arrange
        var claim = new Claim("custom", "value");
        var principal = CreatePrincipal(true, claim);
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);

        // Act & Assert
        Assert.Contains(user.GetUserClaims(), c => c.Type == claim.Type && c.Value == claim.Value);
    }

    [Fact]
    public void GetUserClaims_ReturnsEmpty_WhenNotAuthenticated()
    {
        var principal = CreatePrincipal(false, new Claim("custom", "value"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.Empty(user.GetUserClaims());
    }

    [Fact]
    public void IsAuthenticated_ReturnsTrue_WhenAuthenticated()
    {
        var principal = CreatePrincipal(true);
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.True(user.IsAuthenticated());
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_WhenNotAuthenticated()
    {
        var principal = CreatePrincipal(false);
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.False(user.IsAuthenticated());
    }

    [Fact]
    public void IsInRole_ReturnsTrue_WhenUserInRole()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "admin")], "mock");
        var principal = new ClaimsPrincipal(identity);
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.True(user.IsInRole("admin"));
    }

    [Fact]
    public void IsInRole_ReturnsFalse_WhenUserNotInRole()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "user")], "mock");
        var principal = new ClaimsPrincipal(identity);
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        Assert.False(user.IsInRole("admin"));
    }

    [Fact]
    public void SetCurrentUserId_ChangesId()
    {
        var principal = CreatePrincipal(true, new Claim(ClaimTypes.NameIdentifier, "1"));
        var provider = MockProvider(principal);
        var user = new CurrentUser<int>(provider.Object);
        user.SetCurrentUserId(99);
        Assert.Equal(99, user.Id);
    }

    [Fact]
    public void NonGenericCurrentUser_WorksWithKeyType()
    {
        var principal = CreatePrincipal(true, new Claim(ClaimTypes.NameIdentifier, "123"));
        var provider = MockProvider(principal);
        var user = new CurrentUser(provider.Object);
        Assert.Equal(principal, user.GetUser());
        Assert.Equal(user.GetId(), user.Id);
    }

    private static ClaimsPrincipal CreatePrincipal(bool isAuthenticated = true, params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, isAuthenticated ? "mock" : null);
        if (!isAuthenticated) identity = new ClaimsIdentity();
        return new ClaimsPrincipal(identity);
    }

    private static Mock<ICurrentUserProvider> MockProvider(ClaimsPrincipal? principal)
    {
        var mock = new Mock<ICurrentUserProvider>();
        mock.Setup(p => p.GetUser()).Returns(principal);
        return mock;
    }
}
