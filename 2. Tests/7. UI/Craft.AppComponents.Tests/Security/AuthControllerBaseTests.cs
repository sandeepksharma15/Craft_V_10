using System.Security.Claims;
using Craft.AppComponents.Security;
using Craft.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Tests.Security;

/// <summary>
/// Unit tests for AuthControllerBase covering all seven authentication endpoints.
/// </summary>
public class AuthControllerBaseTests
{
    // Concrete subclass needed because AuthControllerBase is abstract
    private sealed class TestAuthController(IAuthRepository authRepository, ILogger<AuthControllerBase> logger)
        : AuthControllerBase(authRepository, logger);

    private readonly Mock<IAuthRepository> _repoMock;
    private readonly Mock<ILogger<AuthControllerBase>> _loggerMock;
    private readonly TestAuthController _controller;

    public AuthControllerBaseTests()
    {
        // Arrange: shared setup
        _repoMock = new Mock<IAuthRepository>();
        _loggerMock = new Mock<ILogger<AuthControllerBase>>();
        _controller = new TestAuthController(_repoMock.Object, _loggerMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ── LoginAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ReturnsOk_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new UserLoginRequest { Email = "user@example.com", Password = "pass" };
        var authResponse = new JwtAuthResponse("jwt", "refresh", DateTime.UtcNow.AddHours(1));
        _repoMock.Setup(r => r.LoginAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(authResponse, ok.Value);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_WhenRepositoryReturnsNull()
    {
        // Arrange
        var request = new UserLoginRequest { Email = "user@example.com", Password = "wrong" };
        _repoMock.Setup(r => r.LoginAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync((JwtAuthResponse?)null);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        var errors = Assert.IsType<List<string>>(unauthorized.Value);
        Assert.Contains("Invalid email or password.", errors);
    }

    [Fact]
    public async Task LoginAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.LoginAsync(null!));
    }

    // ── RefreshTokenAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshTokenAsync_ReturnsOk_WhenTokensAreValid()
    {
        // Arrange
        var request = new RefreshTokenRequest("old-jwt", "old-refresh", "127.0.0.1");
        var authResponse = new JwtAuthResponse("new-jwt", "new-refresh", DateTime.UtcNow.AddHours(1));
        _repoMock.Setup(r => r.RefreshTokenAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.RefreshTokenAsync(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(authResponse, ok.Value);
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsUnauthorized_WhenRepositoryReturnsNull()
    {
        // Arrange
        var request = new RefreshTokenRequest("expired-jwt", "invalid-refresh", "127.0.0.1");
        _repoMock.Setup(r => r.RefreshTokenAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync((JwtAuthResponse?)null);

        // Act
        var result = await _controller.RefreshTokenAsync(request);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        var errors = Assert.IsType<List<string>>(unauthorized.Value);
        Assert.Contains("Session expired. Please sign in again.", errors);
    }

    [Fact]
    public async Task RefreshTokenAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.RefreshTokenAsync(null!));
    }

    // ── LogoutAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_ReturnsNoContent_WhenCalled()
    {
        // Arrange
        const string userId = "42";
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer test-token";

        var claims = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("uid", userId)
        ]));
        _controller.ControllerContext.HttpContext.User = claims;

        _repoMock.Setup(r => r.LogoutAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LogoutAsync();

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    // ── RegisterUserAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterUserAsync_ReturnsCreated_WhenRegistrationSucceeds()
    {
        // Arrange
        var request = new CreateUserRequest { Email = "new@example.com", Password = "P@ssw0rd", FirstName = "New", LastName = "User" };
        _repoMock.Setup(r => r.RegisterUserAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync("100");

        // Act
        var result = await _controller.RegisterUserAsync(request);

        // Assert
        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status201Created, statusResult.StatusCode);
    }

    [Fact]
    public async Task RegisterUserAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.RegisterUserAsync(null!));
    }

    // ── ChangePasswordAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_ReturnsNoContent_WhenPasswordChangedSuccessfully()
    {
        // Arrange
        var request = new PasswordChangeRequest { Id = 1, Password = "old", NewPassword = "new" };
        _repoMock.Setup(r => r.ChangePasswordAsync(request, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ChangePasswordAsync(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.ChangePasswordAsync(null!));
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsBadRequest_WhenRepositoryThrows()
    {
        // Arrange
        var request = new PasswordChangeRequest { Id = 1, Password = "wrong", NewPassword = "new" };
        _repoMock.Setup(r => r.ChangePasswordAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Password change failed: incorrect current password."));

        // Act
        var result = await _controller.ChangePasswordAsync(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsType<List<string>>(badRequest.Value);
        Assert.Contains("Password change failed. Please check your input and try again.", errors);
    }

    // ── ForgotPasswordAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPasswordAsync_ReturnsNoContent_WhenEmailDispatched()
    {
        // Arrange
        var request = new PasswordForgotRequest { Email = "user@example.com", ClientURI = "https://app/reset" };
        _repoMock.Setup(r => r.ForgotPasswordAsync(request, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ForgotPasswordAsync(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.ForgotPasswordAsync(null!));
    }

    [Fact]
    public async Task ForgotPasswordAsync_ReturnsNoContent_WhenSenderThrows()
    {
        // Arrange — simulates SMTP failure; response must still be 204 to avoid e-mail enumeration.
        var request = new PasswordForgotRequest { Email = "user@example.com", ClientURI = "https://app/reset" };
        _repoMock.Setup(r => r.ForgotPasswordAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable."));

        // Act
        var result = await _controller.ForgotPasswordAsync(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    // ── ResetPasswordAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPasswordAsync_ReturnsNoContent_WhenPasswordResetSuccessfully()
    {
        // Arrange
        var request = new ResetPasswordRequest { Email = "user@example.com", Token = "reset-token", Password = "newP@ss" };
        _repoMock.Setup(r => r.ResetPasswordAsync(request, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ResetPasswordAsync(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ResetPasswordAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.ResetPasswordAsync(null!));
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsBadRequest_WhenRepositoryThrows()
    {
        // Arrange
        var request = new ResetPasswordRequest { Email = "user@example.com", Token = "expired-token", Password = "newP@ss" };
        _repoMock.Setup(r => r.ResetPasswordAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Password reset failed: token has expired."));

        // Act
        var result = await _controller.ResetPasswordAsync(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsType<List<string>>(badRequest.Value);
        Assert.Contains("Password reset failed. Please check your input and try again.", errors);
    }
}
