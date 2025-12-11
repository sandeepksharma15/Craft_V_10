using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;

namespace Craft.Security.Tests.CurrentUserService;

public class UiUserProviderTests
{
    [Fact]
    public void GetUser_Should_ReturnUser_FromAuthenticationState()
    {
        // Arrange
        var expectedPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, "testuser")], "TestAuth"));

        var authState = new AuthenticationState(expectedPrincipal);

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        var provider = new UiUserProvider(mockAuthStateProvider.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPrincipal, result);
        Assert.True(result.Identity?.IsAuthenticated);
    }

    [Fact]
    public void GetUser_Should_ReturnUnauthenticatedUser_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var unauthenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = new AuthenticationState(unauthenticatedPrincipal);

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        var provider = new UiUserProvider(mockAuthStateProvider.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Identity?.IsAuthenticated);
    }

    [Fact]
    public void GetUser_Should_ReturnUserWithClaims_WhenUserIsAuthenticated()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Name, "Jane Smith"),
            new Claim(ClaimTypes.Email, "jane@example.com"),
            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var expectedPrincipal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(expectedPrincipal);

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        var provider = new UiUserProvider(mockAuthStateProvider.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("Jane Smith", result.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("jane@example.com", result.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal("User", result.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public void GetUser_Should_HandleMultipleClaims_OfSameType()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Role, "Manager")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var expectedPrincipal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(expectedPrincipal);

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        var provider = new UiUserProvider(mockAuthStateProvider.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.NotNull(result);
        var roles = result.FindAll(ClaimTypes.Role).ToList();
        Assert.Equal(3, roles.Count);
        Assert.Contains(roles, r => r.Value == "Admin");
        Assert.Contains(roles, r => r.Value == "User");
        Assert.Contains(roles, r => r.Value == "Manager");
    }

    [Fact]
    public void GetUser_Should_ReturnSamePrincipal_OnMultipleCalls()
    {
        // Arrange
        var expectedPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, "testuser")], "TestAuth"));

        var authState = new AuthenticationState(expectedPrincipal);

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        var provider = new UiUserProvider(mockAuthStateProvider.Object);

        // Act
        var result1 = provider.GetUser();
        var result2 = provider.GetUser();

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Provider_Should_ImplementICurrentUserProvider()
    {
        // Arrange
        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(new ClaimsPrincipal()));

        var provider = new UiUserProvider(mockAuthStateProvider.Object);

        // Assert
        Assert.IsAssignableFrom<ICurrentUserProvider>(provider);
    }

    [Fact]
    public void GetUser_Should_HandleEmptyClaimsCollection()
    {
        // Arrange
        var identity = new ClaimsIdentity([], "TestAuth");
        var expectedPrincipal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(expectedPrincipal);

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        var provider = new UiUserProvider(mockAuthStateProvider.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Identity?.IsAuthenticated);
        Assert.Empty(result.Claims);
    }

    [Fact]
    public void GetUser_Should_HandleMultipleIdentities()
    {
        // Arrange
        var identity1 = new ClaimsIdentity([new Claim(ClaimTypes.Name, "User1")], "Auth1");
        var identity2 = new ClaimsIdentity([new Claim(ClaimTypes.Email, "user@example.com")], "Auth2");
        var expectedPrincipal = new ClaimsPrincipal([identity1, identity2]);
        var authState = new AuthenticationState(expectedPrincipal);

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        var provider = new UiUserProvider(mockAuthStateProvider.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Identities.Count());
        Assert.NotNull(result.FindFirst(ClaimTypes.Name));
        Assert.NotNull(result.FindFirst(ClaimTypes.Email));
    }
}
