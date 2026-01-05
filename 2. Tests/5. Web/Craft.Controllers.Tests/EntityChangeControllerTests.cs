using Craft.Domain;
using Craft.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Controllers.Tests;

/// <summary>
/// Unit tests for EntityChangeController covering all controller actions, edge cases, and exception handling.
/// </summary>
public class EntityChangeControllerTests
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

    private readonly Mock<IChangeRepository<TestEntity, int>> _repoMock;
    private readonly Mock<ILogger<EntityChangeController<TestEntity, TestEntityModel, int>>> _loggerMock;
    private readonly TestController _controller;

    // Concrete controller for testing (since EntityChangeController is abstract)
    private class TestController : EntityChangeController<TestEntity, TestEntityModel, int>
    {
        public TestController(IChangeRepository<TestEntity, int> repo, ILogger<EntityChangeController<TestEntity, TestEntityModel, int>> logger)
            : base(repo, logger) { }
    }

    public EntityChangeControllerTests()
    {
        // Arrange: Setup mocks and controller instance
        _repoMock = new Mock<IChangeRepository<TestEntity, int>>();
        _loggerMock = new Mock<ILogger<EntityChangeController<TestEntity, TestEntityModel, int>>>();
        _controller = new TestController(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddAsync_ReturnsCreated_WhenSuccess()
    {
        // Arrange
        var model = new TestEntityModel { Id = 1, Name = "A" };
        var entity = new TestEntity { Id = 1, Name = "A" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<TestEntity>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Act
        var result = await _controller.AddAsync(model);

        // Assert
        var created = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal(entity, created.Value);
    }

    [Fact]
    public async Task AddAsync_ReturnsProblem_OnException()
    {
        // Arrange
        var model = new TestEntityModel { Id = 1, Name = "A" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<TestEntity>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.AddAsync(model);

        // Assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task AddRangeAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var models = new List<TestEntityModel> { new() { Id = 1, Name = "A" } };
        _repoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        // Act
        var result = await _controller.AddRangeAsync(models);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task AddRangeAsync_ReturnsProblem_OnException()
    {
        // Arrange
        var models = new List<TestEntityModel> { new() { Id = 1, Name = "A" } };
        _repoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.AddRangeAsync(models);

        // Assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsOk_WhenEntityFound()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "A" };
        _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.DeleteAsync(entity, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFound_WhenEntityNotFound()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsProblem_OnException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task DeleteRangeAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var models = new List<TestEntityModel> { new() { Id = 1, Name = "A" } };
        _repoMock.Setup(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        // Act
        var result = await _controller.DeleteRangeAsync(models);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteRangeAsync_ReturnsProblem_OnException()
    {
        // Arrange
        var models = new List<TestEntityModel> { new() { Id = 1, Name = "A" } };
        _repoMock.Setup(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.DeleteRangeAsync(models);

        // Assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var model = new TestEntityModel { Id = 1, Name = "A" };
        var entity = new TestEntity { Id = 1, Name = "A" };
        _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Act
        var result = await _controller.UpdateAsync(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(entity, okResult.Value);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsProblem_OnDbUpdateConcurrencyException()
    {
        // Arrange
        var model = new TestEntityModel { Id = 1, Name = "A" };
        var entity = new TestEntity { Id = 1, Name = "A" };
        _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException("concurrency"));

        // Act
        var result = await _controller.UpdateAsync(model);

        // Assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problem.StatusCode);
        Assert.Contains("ProblemDetails", problem.Value?.ToString());
    }

    [Fact]
    public async Task UpdateAsync_ReturnsProblem_OnException()
    {
        // Arrange
        var model = new TestEntityModel { Id = 1, Name = "A" };
        var entity = new TestEntity { Id = 1, Name = "A" };
        _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.UpdateAsync(model);

        // Assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task UpdateRangeAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var models = new List<TestEntityModel> { new() { Id = 1, Name = "A" } };
        var entity = new TestEntity { Id = 1, Name = "A" };
        _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        // Act
        var result = await _controller.UpdateRangeAsync(models);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }

        [Fact]
        public async Task UpdateRangeAsync_ReturnsProblem_OnException()
        {
            // Arrange
            var models = new List<TestEntityModel> { new() { Id = 1, Name = "A" } };
            var entity = new TestEntity { Id = 1, Name = "A" };
            _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _repoMock.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _controller.UpdateRangeAsync(models);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, problem.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNotFound_WhenEntityNotFound()
        {
            // Arrange
            var model = new TestEntityModel { Id = 1, Name = "A" };
            _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync((TestEntity?)null);

            // Act
            var result = await _controller.UpdateAsync(model);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateRangeAsync_ReturnsNotFound_WhenEntityNotFound()
        {
            // Arrange
            var models = new List<TestEntityModel> { new() { Id = 1, Name = "A" } };
            _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync((TestEntity?)null);

            // Act
            var result = await _controller.UpdateRangeAsync(models);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("Entity with ID 1 not found", notFound.Value?.ToString());
        }

            [Fact]
            public async Task UpdateRangeAsync_ReturnsProblem_OnDbUpdateConcurrencyException()
            {
                // Arrange
                var models = new List<TestEntityModel> { new() { Id = 1, Name = "A" } };
                var entity = new TestEntity { Id = 1, Name = "A" };
                _repoMock.Setup(r => r.GetAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
                _repoMock.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException("concurrency"));

                // Act
                var result = await _controller.UpdateRangeAsync(models);

                // Assert
                var problem = Assert.IsType<ObjectResult>(result.Result);
                Assert.Equal(409, problem.StatusCode);
            }
        }
