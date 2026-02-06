using System.Linq.Expressions;
using Craft.Controllers.ErrorHandling;
using Craft.Core;
using Craft.Domain;
using Craft.QuerySpec.Services;
using Craft.Testing.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.QuerySpec.Tests.Services;

// TestModel implements IModel<int> for controller constraints
public class TestModel : IModel<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Concrete test query for IQuery<T>
public class TestQuery<T> : IQuery<T> where T : class
{
    public bool AsNoTracking { get; set; }
    public bool AsSplitQuery { get; set; }
    public bool IgnoreAutoIncludes { get; set; }
    public bool IgnoreQueryFilters { get; set; }
    public bool AutoIncludeNavigationProperties { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public SortOrderBuilder<T>? SortOrderBuilder { get; set; }
    public Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; set; }
    public SqlLikeSearchCriteriaBuilder<T>? SqlLikeSearchCriteriaBuilder { get; set; }
    public EntityFilterBuilder<T>? EntityFilterBuilder { get; set; }
    public List<IncludeExpression>? IncludeExpressions { get; set; }
    public void Clear() { }
    public bool IsSatisfiedBy(T entity) => true;
    public void SetPage(int page, int pageSize) { }
}

// Concrete test query for IQuery<T, TResult>
public class TestQuery<T, TResult> : TestQuery<T>, IQuery<T, TResult>
    where T : class
    where TResult : class
{
    public new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; set; }
    public QuerySelectBuilder<T, TResult>? QuerySelectBuilder { get; }
    public Expression<Func<T, IEnumerable<TResult>>>? SelectorMany { get; set; }
    public new void Clear() { }
}

public class EntityControllerTests
{
    public class TestEntityController : EntityController<TestEntity, TestModel, int>
    {
        public TestEntityController(
            IRepository<TestEntity, int> repo,
            ILogger<EntityController<TestEntity, TestModel, int>> logger,
            IDatabaseErrorHandler errorHandler)
            : base(repo, logger, errorHandler) { }
    }

    private readonly Mock<IRepository<TestEntity, int>> _repoMock;
    private readonly Mock<ILogger<EntityController<TestEntity, TestModel, int>>> _loggerMock;
    private readonly Mock<IDatabaseErrorHandler> _errorHandlerMock;
    private readonly TestEntityController _controller;
    private readonly TestQuery<TestEntity> _query;
    private readonly TestQuery<TestEntity, TestModel> _querySelect;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public EntityControllerTests()
    {
        // Arrange: Setup mocks and controller instance
        _repoMock = new Mock<IRepository<TestEntity, int>>();
        _loggerMock = new Mock<ILogger<EntityController<TestEntity, TestModel, int>>>();
        _errorHandlerMock = new Mock<IDatabaseErrorHandler>();
        _controller = new TestEntityController(_repoMock.Object, _loggerMock.Object, _errorHandlerMock.Object);
        _query = new TestQuery<TestEntity>();
        _querySelect = new TestQuery<TestEntity, TestModel>();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsOk_WhenRepositorySucceeds()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteAsync(_query, true, _cancellationToken)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(_query, _cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsBadRequest_WhenRepositoryThrows()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteAsync(_query, true, _cancellationToken)).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.DeleteAsync(_query, _cancellationToken);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, problem.StatusCode);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithEntities()
    {
        // Arrange
        var entities = new List<TestEntity> { new() { Id = 1, Name = "A" }, new() { Id = 2, Name = "B" } };
        _repoMock.Setup(r => r.GetAllAsync(_query, _cancellationToken)).ReturnsAsync(entities);

        // Act
        var result = await _controller.GetAllAsync(_query, _cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(entities, okResult.Value);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithEmptyList()
    {
        // Arrange
        var entities = new List<TestEntity>();
        _repoMock.Setup(r => r.GetAllAsync(_query, _cancellationToken)).ReturnsAsync(entities);

        // Act
        var result = await _controller.GetAllAsync(_query, _cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Empty((IEnumerable<TestEntity>)okResult.Value!);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(_query, _cancellationToken)).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetAllAsync(_query, _cancellationToken);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, problem.StatusCode);
    }

    [Fact]
    public async Task GetAllAsync_Select_ReturnsOk_WithEntities()
    {
        // Arrange
        var models = new List<TestModel> { new() { Id = 1, Name = "A" } };
        _repoMock.Setup(r => r.GetAllAsync(_querySelect, _cancellationToken)).ReturnsAsync(models);

        // Act
        var result = await _controller.GetAllAsync<TestModel>(_querySelect, _cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(models, okResult.Value);
    }

    [Fact]
    public async Task GetAllAsync_Select_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(_querySelect, _cancellationToken)).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetAllAsync<TestModel>(_querySelect, _cancellationToken);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, problem.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ReturnsOk_WhenEntityFound()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "A" };
        _repoMock.Setup(r => r.GetAsync(_query, _cancellationToken)).ReturnsAsync(entity);

        // Act
        var result = await _controller.GetAsync(_query, _cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(entity, okResult.Value);
    }

    [Fact]
    public async Task GetAsync_ReturnsNotFound_WhenEntityNull()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAsync(_query, _cancellationToken)).ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _controller.GetAsync(_query, _cancellationToken);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAsync_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAsync(_query, _cancellationToken)).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetAsync(_query, _cancellationToken);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, problem.StatusCode);
    }

    [Fact]
    public async Task GetAsync_Select_ReturnsOk_WhenEntityFound()
    {
        // Arrange
        var model = new TestModel { Id = 1, Name = "A" };
        _repoMock.Setup(r => r.GetAsync<TestModel>(_querySelect, _cancellationToken)).ReturnsAsync(model);

        // Act
        var result = await _controller.GetAsync<TestModel>(_querySelect, _cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(model, okResult.Value);
    }

    [Fact]
    public async Task GetAsync_Select_ReturnsNotFound_WhenEntityNull()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAsync<TestModel>(_querySelect, _cancellationToken)).ReturnsAsync((TestModel?)null);

        // Act
        var result = await _controller.GetAsync<TestModel>(_querySelect, _cancellationToken);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAsync_Select_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAsync<TestModel>(_querySelect, _cancellationToken)).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetAsync<TestModel>(_querySelect, _cancellationToken);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, problem.StatusCode);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsOk_WithCount()
    {
        // Arrange
        _repoMock.Setup(r => r.GetCountAsync(_query, _cancellationToken)).ReturnsAsync(42);

        // Act
        var result = await _controller.GetCountAsync(_query, _cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(42L, okResult.Value);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetCountAsync(_query, _cancellationToken)).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetCountAsync(_query, _cancellationToken);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, problem.StatusCode);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsOk_WithPageResponse()
    {
        // Arrange
        var page = new PageResponse<TestEntity>([new() { Id = 1, Name = "A" }], 1, 1, 10);
        _repoMock.Setup(r => r.GetPagedListAsync(_query, _cancellationToken)).ReturnsAsync(page);

        // Act
        var result = await _controller.GetPagedListAsync(_query, _cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(page, okResult.Value);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetPagedListAsync(_query, _cancellationToken)).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetPagedListAsync(_query, _cancellationToken);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, problem.StatusCode);
    }

    [Fact]
    public async Task GetPagedListAsync_Select_ReturnsOk_WithPageResponse()
    {
        // Arrange
        var page = new PageResponse<TestModel>([new() { Id = 1, Name = "A" }], 1, 1, 10);
        _repoMock.Setup(r => r.GetPagedListAsync(_querySelect, _cancellationToken)).ReturnsAsync(page);

        // Act
        var result = await _controller.GetPagedListAsync<TestModel>(_querySelect, _cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(page, okResult.Value);
    }

    [Fact]
    public async Task GetPagedListAsync_Select_ReturnsBadRequest_OnException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetPagedListAsync(_querySelect, _cancellationToken)).ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.GetPagedListAsync<TestModel>(_querySelect, _cancellationToken);

        // Assert
        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, problem.StatusCode);
    }
}
