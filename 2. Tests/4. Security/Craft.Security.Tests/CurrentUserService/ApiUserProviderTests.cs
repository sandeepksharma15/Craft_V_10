using System.Security.Claims;
using Craft.Core;
using Craft.Security.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        Assert.IsType<ICurrentUserProvider>(provider, exactMatch: false);
    }

    // -----------------------------------------------------------------------
    // AddCraftJwtAuthApi registration tests
    // -----------------------------------------------------------------------

    private static IConfiguration BuildJwtConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection([
                KeyValuePair.Create<string, string?>("SecuritySettings:JwtSettings:IssuerSigningKey", "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456"),
                KeyValuePair.Create<string, string?>("SecuritySettings:JwtSettings:ValidIssuer", "TestIssuer"),
                KeyValuePair.Create<string, string?>("SecuritySettings:JwtSettings:ValidAudiences:0", "TestAudience"),
                KeyValuePair.Create<string, string?>("SecuritySettings:JwtSettings:TokenExpirationInMinutes", "60"),
                KeyValuePair.Create<string, string?>("SecuritySettings:JwtSettings:RefreshTokenExpirationInDays", "7"),
                KeyValuePair.Create<string, string?>("SecuritySettings:JwtSettings:RequireHttpsMetaData", "false"),
            ])
            .Build();

    [Fact]
    public void AddCraftJwtAuthApi_ReturnsSameServiceCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = BuildJwtConfig();

        // Act
        var result = services.AddCraftJwtAuthApi<object>(config);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCraftJwtAuthApi_RegistersApiUserProvider_AsICurrentUserProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddCraftJwtAuthApi<object>(BuildJwtConfig());
        var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var userProvider = scope.ServiceProvider.GetService<ICurrentUserProvider>();

        // Assert
        Assert.NotNull(userProvider);
        Assert.IsType<ApiUserProvider>(userProvider);
    }

    [Fact]
    public void AddCraftJwtAuthApi_RegistersCurrentUser_AsICurrentUser()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddCraftJwtAuthApi<object>(BuildJwtConfig());
        var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var currentUser = scope.ServiceProvider.GetService<ICurrentUser>();

        // Assert
        Assert.NotNull(currentUser);
        Assert.IsType<CurrentUser>(currentUser);
    }

    [Fact]
    public void AddCraftJwtAuthApi_RegistersCurrentUserOfTKey_AsICurrentUserOfTKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddCraftJwtAuthApi<object>(BuildJwtConfig());
        var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var currentUser = scope.ServiceProvider.GetService<ICurrentUser<long>>();

        // Assert
        Assert.NotNull(currentUser);
        Assert.IsType<CurrentUser<long>>(currentUser);
    }

    [Fact]
    public void AddCraftJwtAuthApi_RegistersInMemoryTokenBlacklist_AsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCraftJwtAuthApi<object>(BuildJwtConfig());
        var provider = services.BuildServiceProvider();

        // Act
        var blacklist = provider.GetService<ITokenBlacklist>();

        // Assert
        Assert.NotNull(blacklist);
        Assert.IsType<InMemoryTokenBlacklist>(blacklist);
    }

    [Fact]
    public void AddCraftJwtAuthApi_RegistersTimeProvider_AsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCraftJwtAuthApi<object>(BuildJwtConfig());
        var provider = services.BuildServiceProvider();

        // Act
        var timeProvider = provider.GetService<TimeProvider>();

        // Assert
        Assert.NotNull(timeProvider);
        Assert.Same(TimeProvider.System, timeProvider);
    }

    [Fact]
    public void AddCraftJwtAuthApi_RegistersTokenBlacklistCleanupService_AsHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCraftJwtAuthApi<object>(BuildJwtConfig());

        // Act
        var descriptor = services.FirstOrDefault(d => d.ImplementationType == typeof(TokenBlacklistCleanupService));

        // Assert
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddCraftJwtAuthApi_CurrentUserServices_AreScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddCraftJwtAuthApi<object>(BuildJwtConfig());
        var provider = services.BuildServiceProvider();

        // Act
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();
        var user1 = scope1.ServiceProvider.GetService<ICurrentUser>();
        var user2 = scope2.ServiceProvider.GetService<ICurrentUser>();

        // Assert
        Assert.NotNull(user1);
        Assert.NotNull(user2);
        Assert.NotSame(user1, user2);
    }
}
