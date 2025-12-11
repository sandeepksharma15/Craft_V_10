using Craft.Core;
using Craft.Security.Tokens;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MicrosoftOptions = Microsoft.Extensions.Options;

namespace Craft.Security.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCurrentApiUser_RegistersApiUserProviderAndCurrentUser()
    {
        // Arrange
        var services = new ServiceCollection();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        services.AddSingleton(httpContextAccessorMock.Object);
        services.AddCurrentApiUser();
        var provider = services.BuildServiceProvider();

        var userProvider = provider.GetService<ICurrentUserProvider>();
        var currentUser = provider.GetService<ICurrentUser>();

        Assert.NotNull(userProvider);
        Assert.Equal("Craft.Security.ApiUserProvider", userProvider!.GetType().FullName);
        Assert.NotNull(currentUser);
        Assert.Equal("Craft.Security.CurrentUser", currentUser!.GetType().FullName);
    }

    [Fact]
    public void AddCurrentUiUser_RegistersUiUserProviderAndCurrentUser()
    {
        var services = new ServiceCollection();
        var authStateProviderMock = new Mock<AuthenticationStateProvider>();
        authStateProviderMock
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal()));
        services.AddScoped(_ => authStateProviderMock.Object);
        services.AddCurrentUiUser();
        var provider = services.BuildServiceProvider();

        var userProvider = provider.GetService<ICurrentUserProvider>();
        var currentUser = provider.GetService<ICurrentUser>();

        Assert.NotNull(userProvider);
        Assert.Equal("Craft.Security.UiUserProvider", userProvider!.GetType().FullName);
        Assert.NotNull(currentUser);
        Assert.Equal("Craft.Security.CurrentUser", currentUser!.GetType().FullName);
    }

    [Fact]
    public void AddCurrentApiUser_ReturnsSameServiceCollectionInstance()
    {
        var services = new ServiceCollection();
        var result = services.AddCurrentApiUser();
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCurrentUiUser_ReturnsSameServiceCollectionInstance()
    {
        var services = new ServiceCollection();
        var result = services.AddCurrentUiUser();
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCraftSecurity_RegistersTokenManager()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register dependencies for TokenManager
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITokenBlacklist, InMemoryTokenBlacklist>();
        services.AddLogging();
        
        var jwtSettings = new JwtSettings
        {
            IssuerSigningKey = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456",
            ValidIssuer = "TestIssuer",
            ValidAudiences = ["TestAudience"],
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };
        services.AddSingleton(MicrosoftOptions.Options.Create(jwtSettings));
        
        services.AddCraftSecurity();
        var provider = services.BuildServiceProvider();

        // Act
        var tokenManager = provider.GetService<ITokenManager>();

        // Assert
        Assert.NotNull(tokenManager);
        Assert.IsType<TokenManager>(tokenManager);
    }

    [Fact]
    public void AddCraftSecurity_ReturnsSameServiceCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCraftSecurity();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCurrentApiUser_RegistersServicesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        services.AddSingleton(httpContextAccessorMock.Object);
        services.AddCurrentApiUser();
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

    [Fact]
    public void AddCurrentUiUser_RegistersServicesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var authStateProviderMock = new Mock<AuthenticationStateProvider>();
        authStateProviderMock
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal()));
        services.AddScoped(_ => authStateProviderMock.Object);
        services.AddCurrentUiUser();
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

    [Fact]
    public void AddCraftSecurity_RegistersTokenManagerAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register dependencies for TokenManager
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITokenBlacklist, InMemoryTokenBlacklist>();
        services.AddLogging();
        
        var jwtSettings = new JwtSettings
        {
            IssuerSigningKey = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456",
            ValidIssuer = "TestIssuer",
            ValidAudiences = ["TestAudience"],
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };
        services.AddSingleton(MicrosoftOptions.Options.Create(jwtSettings));
        
        services.AddCraftSecurity();
        var provider = services.BuildServiceProvider();

        // Act
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();
        var manager1 = scope1.ServiceProvider.GetService<ITokenManager>();
        var manager2 = scope2.ServiceProvider.GetService<ITokenManager>();

        // Assert
        Assert.NotNull(manager1);
        Assert.NotNull(manager2);
        Assert.NotSame(manager1, manager2);
    }
}
