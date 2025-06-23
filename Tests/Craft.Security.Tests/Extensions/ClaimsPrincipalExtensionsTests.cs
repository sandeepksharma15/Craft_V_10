using System.Security.Claims;
using Craft.Security.Claims;

namespace Craft.Security.Tests.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetEmail_ReturnsEmail_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(ClaimTypes.Email, "user@example.com"));

        // Act & Assert
        Assert.Equal("user@example.com", principal.GetEmail());
    }

    [Fact]
    public void GetEmail_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetEmail());
    }

    [Fact]
    public void GetFirstName_ReturnsName_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "John"));

        // Act & Assert
        Assert.Equal("John", principal.GetFirstName());
    }

    [Fact]
    public void GetFirstName_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetFirstName());
    }

    [Fact]
    public void GetFullName_ReturnsFullName_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(CraftClaims.Fullname, "John Doe"));

        // Act & Assert
        Assert.Equal("John Doe", principal.GetFullName());
    }

    [Fact]
    public void GetFullName_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetFullName());
    }

    [Fact]
    public void GetImageUrl_ReturnsImageUrl_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(CraftClaims.ImageUrl, "http://img.com/u.png"));

        // Act & Assert
        Assert.Equal("http://img.com/u.png", principal.GetImageUrl());
    }

    [Fact]
    public void GetImageUrl_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetImageUrl());
    }

    [Fact]
    public void GetJwtToken_ReturnsJwtToken_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(CraftClaims.JwtToken, "jwt-token-value"));

        // Act & Assert
        Assert.Equal("jwt-token-value", principal.GetJwtToken());
    }

    [Fact]
    public void GetJwtToken_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetJwtToken());
    }

    [Fact]
    public void GetLastName_ReturnsSurname_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(ClaimTypes.Surname, "Smith"));

        // Act & Assert
        Assert.Equal("Smith", principal.GetLastName());
    }

    [Fact]
    public void GetLastName_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetLastName());
    }

    [Fact]
    public void GetMobileNumber_ReturnsMobile_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(ClaimTypes.MobilePhone, "+1234567890"));

        // Act & Assert
        Assert.Equal("+1234567890", principal.GetMobileNumber());
    }

    [Fact]
    public void GetMobileNumber_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetMobileNumber());
    }

    [Fact]
    public void GetPermissions_ReturnsPermissions_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(CraftClaims.Permissions, "read,write"));

        // Act & Assert
        Assert.Equal("read,write", principal.GetPermissions());
    }

    [Fact]
    public void GetPermissions_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetPermissions());
    }

    [Fact]
    public void GetRole_ReturnsRole_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(CraftClaims.Role, "admin"));

        // Act & Assert
        Assert.Equal("admin", principal.GetRole());
    }

    [Fact]
    public void GetRole_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetRole());
    }

    [Fact]
    public void GetTenant_ReturnsTenant_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(CraftClaims.Tenant, "tenant-1"));

        // Act & Assert
        Assert.Equal("tenant-1", principal.GetTenant());
    }

    [Fact]
    public void GetTenant_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetTenant());
    }

    [Fact]
    public void GetUserId_ReturnsUserId_WhenClaimExists()
    {
        // Arrange
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, "user-123"));

        // Act & Assert
        Assert.Equal("user-123", principal.GetUserId());
    }

    [Fact]
    public void GetUserId_ReturnsNull_WhenClaimMissing()
    {
        // Arrange
        var principal = CreatePrincipal();

        // Act & Assert
        Assert.Null(principal.GetUserId());
    }

    [Fact]
    public void ExtensionMethods_HandleNullPrincipal_Gracefully()
    {
        // Arrange
        ClaimsPrincipal? principal = null;

        // Act & Assert
        Assert.Null(principal?.GetFirstName());
        Assert.Null(principal?.GetFullName());
        Assert.Null(principal?.GetLastName());
    }

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "mock"));
}
