using Craft.Core;
using Craft.Domain;
using Craft.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.Controllers.Tests;

/// <summary>
/// Unit tests for EntityReadController covering all controller actions, edge cases, and exception handling.
/// </summary>
public class EntityReadControllerTests
{
    // Test entity implementing IEntity<int> and IModel<int> for controller constraints
    public class TestEntity : IEntity<int>, IModel<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Test model implementing IModel<int> for DataTransferT
    public class TestEntityModel : IModel<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private readonly Mock<IReadRepository<TestEntity, int>> _repositoryMock;
    private readonly Mock<ILogger<EntityReadController<TestEntity, TestEntityModel, int>>> _loggerMock;
    private readonly EntityReadController<TestEntity, TestEntityModel, int> _controller;

    public EntityReadControllerTests()
    {
        // Arrange: Setup mocks and controller instance
        _repositoryMock = new Mock<IReadRepository<TestEntity, int>>();
        _loggerMock = new Mock<ILogger<EntityReadController<TestEntity, TestEntityModel, int>>>();
        _controller = new EntityReadController<TestEntity, TestEntityModel, int>(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithEntities()
    {
        // Arrange
        var entities = new List<TestEntity> { new() { Id = 1, Name = "A" }, new() { Id = 2, Name = "B" } };
        _repositoryMock.Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        // Act
        var result = await _controller.GetAllAsync(false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(entities, okResult.Value);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithEmptyList()
    {
        // Arrange
        var entities = new List<TestEntity>();
        _repositoryMock.Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        // Act
        var result = await _controller.GetAllAsync(false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Empty((IEnumerable<TestEntity>)okResult.Value!);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetAllAsync(false);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ReturnsOk_WhenEntityFound()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "A" };
        _repositoryMock.Setup(r => r.GetAsync(1, false, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Act
        var result = await _controller.GetAsync(1, false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(entity, okResult.Value);
    }

    [Fact]
    public async Task GetAsync_ReturnsNotFound_WhenEntityNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAsync(1, false, It.IsAny<CancellationToken>())).ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _controller.GetAsync(1, false);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAsync_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAsync(1, false, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetAsync(1, false);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsOk_WithCount()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(42);

        // Act
        var result = await _controller.GetCountAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(42L, okResult.Value);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetCountAsync();

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsOk_WithPageResponse()
    {
        // Arrange
        var pageResponse = new PageResponse<TestEntity>([new() { Id = 1, Name = "A" }], 1, 1, 10);
        _repositoryMock.Setup(r => r.GetPagedListAsync(1, 10, false, It.IsAny<CancellationToken>())).ReturnsAsync(pageResponse);

        // Act
        var result = await _controller.GetPagedListAsync(1, 10, false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(pageResponse, okResult.Value);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetPagedListAsync(1, 10, false, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetPagedListAsync(1, 10, false);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }
}
