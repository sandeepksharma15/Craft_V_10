using Craft.AppComponents.Auditing;
using Craft.Auditing;
using Craft.Controllers.ErrorHandling;
using Craft.QuerySpec;
using Craft.QuerySpec.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Tests.Auditing;

/// <summary>
/// Unit tests for <see cref="AuditTrailController"/> covering the two custom endpoints
/// and verifying exception-path behaviour.
/// </summary>
public class AuditTrailControllerTests
{
    private readonly Mock<IRepository<AuditTrail, KeyType>> _repoMock;
    private readonly Mock<ILogger<EntityController<AuditTrail, AuditTrail, KeyType>>> _entityLoggerMock;
    private readonly Mock<IDatabaseErrorHandler> _errorHandlerMock;
    private readonly Mock<IAuditTrailRepository> _auditRepoMock;
    private readonly Mock<ILogger<AuditTrailController>> _auditLoggerMock;
    private readonly AuditTrailController _controller;

    public AuditTrailControllerTests()
    {
        // Arrange: shared setup
        _repoMock = new Mock<IRepository<AuditTrail, KeyType>>();
        _entityLoggerMock = new Mock<ILogger<EntityController<AuditTrail, AuditTrail, KeyType>>>();
        _errorHandlerMock = new Mock<IDatabaseErrorHandler>();
        _auditRepoMock = new Mock<IAuditTrailRepository>();
        _auditLoggerMock = new Mock<ILogger<AuditTrailController>>();

        _controller = new AuditTrailController(
            _repoMock.Object,
            _entityLoggerMock.Object,
            _errorHandlerMock.Object,
            _auditRepoMock.Object,
            _auditLoggerMock.Object);
    }

    // ── GetTableNamesAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetTableNamesAsync_ReturnsOk_WithTableNames()
    {
        // Arrange
        var tableNames = new List<string> { "Orders", "Products", "Users" };
        _auditRepoMock
            .Setup(r => r.GetTableNamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tableNames);

        // Act
        var result = await _controller.GetTableNamesAsync();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(tableNames, ok.Value);
    }

    [Fact]
    public async Task GetTableNamesAsync_ReturnsOk_WithEmptyList()
    {
        // Arrange
        _auditRepoMock
            .Setup(r => r.GetTableNamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _controller.GetTableNamesAsync();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<string>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetTableNamesAsync_ReturnsBadRequest_WhenRepositoryThrows()
    {
        // Arrange
        _auditRepoMock
            .Setup(r => r.GetTableNamesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        var result = await _controller.GetTableNamesAsync();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ── GetAuditUsersAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAuditUsersAsync_ReturnsOk_WithUsers()
    {
        // Arrange
        var users = new List<AuditUserDTO>
        {
            new(1, "Alice"),
            new(2, "Bob")
        };
        _auditRepoMock
            .Setup(r => r.GetAuditUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAuditUsersAsync();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(users, ok.Value);
    }

    [Fact]
    public async Task GetAuditUsersAsync_ReturnsOk_WithEmptyList()
    {
        // Arrange
        _auditRepoMock
            .Setup(r => r.GetAuditUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _controller.GetAuditUsersAsync();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<AuditUserDTO>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetAuditUsersAsync_ReturnsBadRequest_WhenRepositoryThrows()
    {
        // Arrange
        _auditRepoMock
            .Setup(r => r.GetAuditUsersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        var result = await _controller.GetAuditUsersAsync();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
