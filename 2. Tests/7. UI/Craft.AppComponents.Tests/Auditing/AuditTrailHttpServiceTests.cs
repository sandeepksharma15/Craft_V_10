using System.Net;
using System.Text.Json;
using Craft.AppComponents.Auditing;
using Craft.Auditing;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Tests.Auditing;

/// <summary>
/// Unit tests for <see cref="AuditTrailHttpService"/> covering the two audit-specific HTTP endpoints.
/// Uses a stub <see cref="HttpMessageHandler"/> to avoid real network calls.
/// </summary>
public class AuditTrailHttpServiceTests
{
    private static AuditTrailHttpService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var apiUrl = new Uri("https://api/api/audittrail");
        var logger = Mock.Of<ILogger<AuditTrailHttpService>>();
        return new AuditTrailHttpService(apiUrl, httpClient, logger);
    }

    private static HttpMessageHandler OkJsonHandler<T>(T body)
        => new StubHttpHandler(HttpStatusCode.OK, JsonSerializer.Serialize(body));

    private static HttpMessageHandler ErrorHandler(HttpStatusCode code, string body = "error")
        => new StubHttpHandler(code, body);

    // ── GetTableNamesAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetTableNamesAsync_ReturnsSuccess_WithTableNames()
    {
        // Arrange
        var tableNames = new List<string> { "Orders", "Products" };
        var service = CreateService(OkJsonHandler(tableNames));

        // Act
        var result = await service.GetTableNamesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(tableNames, result.Value);
    }

    [Fact]
    public async Task GetTableNamesAsync_ReturnsSuccess_WithEmptyList()
    {
        // Arrange
        var service = CreateService(OkJsonHandler(new List<string>()));

        // Act
        var result = await service.GetTableNamesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetTableNamesAsync_ReturnsFailure_WhenApiRespondsWithError()
    {
        // Arrange
        var service = CreateService(ErrorHandler(HttpStatusCode.InternalServerError));

        // Act
        var result = await service.GetTableNamesAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task GetTableNamesAsync_ReturnsFailure_WhenApiRespondsWithNotFound()
    {
        // Arrange
        var service = CreateService(ErrorHandler(HttpStatusCode.NotFound));

        // Act
        var result = await service.GetTableNamesAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }

    // ── GetAuditUsersAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAuditUsersAsync_ReturnsSuccess_WithUsers()
    {
        // Arrange
        var users = new List<AuditUserDTO>
        {
            new(1, "Alice"),
            new(2, "Bob")
        };
        var service = CreateService(OkJsonHandler(users));

        // Act
        var result = await service.GetAuditUsersAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Alice", result.Value[0].UserName);
    }

    [Fact]
    public async Task GetAuditUsersAsync_ReturnsSuccess_WithEmptyList()
    {
        // Arrange
        var service = CreateService(OkJsonHandler(new List<AuditUserDTO>()));

        // Act
        var result = await service.GetAuditUsersAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAuditUsersAsync_ReturnsFailure_WhenApiRespondsWithError()
    {
        // Arrange
        var service = CreateService(ErrorHandler(HttpStatusCode.InternalServerError));

        // Act
        var result = await service.GetAuditUsersAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task GetAuditUsersAsync_ReturnsFailure_WhenApiRespondsWithUnauthorized()
    {
        // Arrange
        var service = CreateService(ErrorHandler(HttpStatusCode.Unauthorized));

        // Act
        var result = await service.GetAuditUsersAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
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
