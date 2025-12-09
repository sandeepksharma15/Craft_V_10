using Craft.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Craft.Security.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCurrentApiUser_RegistersApiUserProviderAndCurrentUser()
    {
        // Arrange
        var services = new ServiceCollection();
        // Register a mock IHttpContextAccessor
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
        // Register a mock AuthenticationStateProvider as scoped
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
}
