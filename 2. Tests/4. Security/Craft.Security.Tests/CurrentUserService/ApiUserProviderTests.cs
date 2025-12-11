using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Craft.Security.Tests.CurrentUserService;

public class ApiUserProviderTests
{
    [Fact]
    public void GetUser_Should_ReturnUser_FromHttpContext()
    {
        // Arrange
        var expectedPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, "testuser")], "TestAuth"));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(expectedPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var provider = new ApiUserProvider(mockHttpContextAccessor.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPrincipal, result);
        Assert.True(result.Identity?.IsAuthenticated);
    }

    [Fact]
    public void GetUser_Should_ReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var provider = new ApiUserProvider(mockHttpContextAccessor.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUser_Should_ReturnUnauthenticatedUser_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var unauthenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(unauthenticatedPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var provider = new ApiUserProvider(mockHttpContextAccessor.Object);

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
            new Claim(ClaimTypes.Name, "John Doe"),
            new Claim(ClaimTypes.Email, "john@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var expectedPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(expectedPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var provider = new ApiUserProvider(mockHttpContextAccessor.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("John Doe", result.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("john@example.com", result.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal("Admin", result.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public void GetUser_Should_HandleNullHttpContextAccessor_Gracefully()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var provider = new ApiUserProvider(mockHttpContextAccessor.Object);

        // Act
        var result = provider.GetUser();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUser_Should_ReturnSamePrincipal_OnMultipleCalls()
    {
        // Arrange
        var expectedPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, "testuser")], "TestAuth"));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(expectedPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var provider = new ApiUserProvider(mockHttpContextAccessor.Object);

        // Act
        var result1 = provider.GetUser();
        var result2 = provider.GetUser();

        // Assert
        Assert.Equal(result1, result2);
        Assert.Same(result1, result2);
    }

    [Fact]
    public void Provider_Should_ImplementICurrentUserProvider()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var provider = new ApiUserProvider(mockHttpContextAccessor.Object);

        // Assert
        Assert.IsAssignableFrom<ICurrentUserProvider>(provider);
    }
}
