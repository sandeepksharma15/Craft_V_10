using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Craft.AppComponents.Security;
using Craft.Security;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Tests.Security;

/// <summary>
/// Unit tests for <see cref="AuthHttpService{TUserVM}"/> covering all four HTTP endpoints.
/// Uses a stub <see cref="HttpMessageHandler"/> to avoid real network calls.
/// </summary>
public class AuthHttpServiceTests
{
    // Simple view-model for registration tests
    public class TestUserVM
    {
        public string? Email { get; set; }
    }

    private static AuthHttpService<TestUserVM> CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var apiUrl = new Uri("https://api/api/auth");
        var logger = Mock.Of<ILogger<AuthHttpService<TestUserVM>>>();
        return new AuthHttpService<TestUserVM>(apiUrl, httpClient, logger);
    }

    private static HttpMessageHandler OkJsonHandler<T>(T body)
        => new StubHttpHandler(HttpStatusCode.OK, JsonSerializer.Serialize(body));

    private static HttpMessageHandler NoContentHandler()
        => new StubHttpHandler(HttpStatusCode.NoContent, string.Empty);

    private static HttpMessageHandler ErrorHandler(HttpStatusCode code, string body = "error")
        => new StubHttpHandler(code, body);

    // ── LoginAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ReturnsSuccess_WhenApiRespondsWithTokens()
    {
        // Arrange
        var expected = new JwtAuthResponse("jwt", "refresh", DateTime.UtcNow.AddHours(1));
        var service = CreateService(OkJsonHandler(expected));
        var request = new UserLoginRequest { Email = "user@example.com", Password = "pass" };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expected.JwtToken, result.Value.JwtToken);
        Assert.Equal(expected.RefreshToken, result.Value.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WhenApiRespondsWithUnauthorized()
    {
        // Arrange
        var service = CreateService(ErrorHandler(HttpStatusCode.Unauthorized));
        var request = new UserLoginRequest { Email = "user@example.com", Password = "wrong" };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_ThrowsArgumentNullException_WhenModelIsNull()
    {
        // Arrange
        var service = CreateService(OkJsonHandler(JwtAuthResponse.Empty));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.LoginAsync(null!));
    }

    // ── RefreshAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshAsync_ReturnsSuccess_WhenApiIssuesNewTokens()
    {
        // Arrange
        var expected = new JwtAuthResponse("new-jwt", "new-refresh", DateTime.UtcNow.AddHours(1));
        var service = CreateService(OkJsonHandler(expected));
        var request = new RefreshTokenRequest("old-jwt", "old-refresh", "127.0.0.1");

        // Act
        var result = await service.RefreshAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected.JwtToken, result.Value!.JwtToken);
    }

    [Fact]
    public async Task RefreshAsync_ReturnsFailure_WhenApiRespondsWithUnauthorized()
    {
        // Arrange
        var service = CreateService(ErrorHandler(HttpStatusCode.Unauthorized));
        var request = new RefreshTokenRequest("expired", "invalid", "127.0.0.1");

        // Act
        var result = await service.RefreshAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task RefreshAsync_ThrowsArgumentNullException_WhenModelIsNull()
    {
        // Arrange
        var service = CreateService(OkJsonHandler(JwtAuthResponse.Empty));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.RefreshAsync(null!));
    }

    // ── LogoutAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_ReturnsSuccess_WhenApiRespondsWithNoContent()
    {
        // Arrange
        var service = CreateService(NoContentHandler());

        // Act
        var result = await service.LogoutAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LogoutAsync_ReturnsFailure_WhenApiRespondsWithError()
    {
        // Arrange
        var service = CreateService(ErrorHandler(HttpStatusCode.InternalServerError));

        // Act
        var result = await service.LogoutAsync();

        // Assert
        Assert.False(result.IsSuccess);
    }

    // ── RegisterAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ReturnsSuccess_WhenApiRespondsWithNoContent()
    {
        // Arrange
        var service = CreateService(NoContentHandler());
        var model = new TestUserVM { Email = "new@example.com" };

        // Act
        var result = await service.RegisterAsync(model);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsFailure_WhenApiRespondsWithBadRequest()
    {
        // Arrange
        var service = CreateService(ErrorHandler(HttpStatusCode.BadRequest));
        var model = new TestUserVM { Email = "bad@example.com" };

        // Act
        var result = await service.RegisterAsync(model);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsArgumentNullException_WhenModelIsNull()
    {
        // Arrange
        var service = CreateService(NoContentHandler());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.RegisterAsync(null!));
    }

    // ── Helper: stub message handler ─────────────────────────────────────────────

    private sealed class StubHttpHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
