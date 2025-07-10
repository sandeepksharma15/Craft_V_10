using Craft.QuerySpec;

namespace Craft.QuerySpec.Tests.Evaluators;

public class QueryEvaluatorTests
{
    private readonly IQueryable<TestEntity> _testEntities;

    public QueryEvaluatorTests()
    {
        _testEntities = new List<TestEntity>
        {
            new() { Id = 1, Name = "Test1", IsActive = true },
            new() { Id = 2, Name = "Test2", IsActive = false },
            new() { Id = 3, Name = "Test3", IsActive = true },
            new() { Id = 4, Name = "Test4", IsActive = false },
            new() { Id = 5, Name = "Test5", IsActive = true },
        }.AsQueryable();
    }

    [Fact]
    public void GetQuery_WithNullQuery_ReturnsQueryable()
    {
        // Arrange
        var evaluator = QueryEvaluator.Instance;

        // Act & Assert
        var result = evaluator.GetQuery(_testEntities, null);
        Assert.Equal([.. _testEntities], [.. result]);
    }

    [Fact]
    public void GetQuery_WithEmptyQuery_ReturnsQueryable()
    {
        // Arrange
        var evaluator = QueryEvaluator.Instance;
        var query = new Query<TestEntity>();

        // Act & Assert
        var result = evaluator.GetQuery(_testEntities, query);
        Assert.Equal([.. _testEntities], [.. result]);
    }

    [Fact]
    public void GetQuery_WithValidQuery_ReturnsQueryable()
    {
        // Arrange
        var evaluator = QueryEvaluator.Instance;
        var query = new Query<TestEntity>()
            .Where(e => e.IsActive)!
            .OrderBy(e => e.Id)!
            .Skip(1)!
            .Take(2);

        // Act
        var expected = _testEntities
            .Where(e => e.IsActive)
            .OrderBy(e => e.Id)
            .Skip(1)
            .Take(2)
            .ToList();

        var result = evaluator.GetQuery(_testEntities, query).ToList();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetQuery_WithValidQueryAndSelect_ReturnsQueryable()
    {
        // Arrange
        var evaluator = QueryEvaluator.Instance;
        var query = new Query<TestEntity, TestDto>();

        query.Where(e => e.IsActive)!
            .OrderBy(e => e.Id)!
            .Skip(1)!
            .Take(2);

        query.Select(e => e.Id);
        query.Select(e => e.Name);

        var expected = _testEntities
            .Where(e => e.IsActive)
            .OrderBy(e => e.Id)
            .Skip(1)
            .Take(2)
            .Select(e => new TestDto { Id = e.Id, Name = e.Name })
            .ToList();

        // Act
        var result = evaluator.GetQuery(_testEntities, query).ToList();

        // Assert
        Assert.Equal(expected.Count, result.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, result[i].Id);
            Assert.Equal(expected[i].Name, result[i].Name);
        }
    }

    [Fact]
    public void GetQuery_WithNullQueryable_ThrowsArgumentNullException()
    {
        // Arrange
        var evaluator = QueryEvaluator.Instance;
        var query = new Query<TestEntity>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.GetQuery<TestEntity>(null!, query));
    }

    [Fact]
    public void GetQuery_WithNullQueryForSelect_ThrowsArgumentNullException()
    {
        // Arrange
        var evaluator = QueryEvaluator.Instance;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.GetQuery<TestEntity, TestDto>(_testEntities, null));
    }

    [Fact]
    public void GetQuery_WithNoSelectOrSelectMany_ThrowsInvalidOperationException()
    {
        // Arrange
        var evaluator = QueryEvaluator.Instance;
        var query = new Query<TestEntity, TestDto>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => evaluator.GetQuery<TestEntity, TestDto>(_testEntities, query));
    }

    [Fact]
    public void GetQuery_WithBothSelectAndSelectMany_ThrowsInvalidOperationException()
    {
        // Arrange
        var evaluator = QueryEvaluator.Instance;
        var query = new Query<TestEntity, TestDto>();
        query.Select(e => e.Id);
        query.SelectorMany = e => new List<TestDto>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => evaluator.GetQuery<TestEntity, TestDto>(_testEntities, query));
    }

    [Fact]
    public void GetQuery_WithSelectMany_ReturnsFlattenedResults()
    {
        // Arrange
        var evaluator = QueryEvaluator.Instance;
        var query = new Query<TestEntity, TestSubEntity>
        {
            SelectorMany = e => e.TestSubEntities
        };
        var testEntities = new List<TestEntity>
        {
            new() { Id = 1, Name = "Test1", TestSubEntities = [new() { Id = 10, Name = "Sub1" }, new() { Id = 11, Name = "Sub2" }] },
            new() { Id = 2, Name = "Test2", TestSubEntities = [new() { Id = 20, Name = "Sub3" }] },
        }.AsQueryable();

        // Act
        var result = evaluator.GetQuery(testEntities, query).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.Id == 10 && x.Name == "Sub1");
        Assert.Contains(result, x => x.Id == 11 && x.Name == "Sub2");
        Assert.Contains(result, x => x.Id == 20 && x.Name == "Sub3");
    }

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<TestSubEntity> TestSubEntities { get; set; } = [];
    }

    private class TestSubEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
