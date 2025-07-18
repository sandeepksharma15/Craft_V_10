﻿using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Evaluators;

public class IgnoreAutoIncludeEvaluatorTests
{
    private readonly IQueryable<Company> _companies;

    public IgnoreAutoIncludeEvaluatorTests()
    {
        _companies = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 2, Name = "Company 2" }
        }.AsQueryable();
    }

    [Fact]
    public void GetQuery_IgnoreAutoIncludesTrue_ReturnsIgnoreAutoIncludesQuery()
    {
        // Arrange
        var evaluator = IgnoreAutoIncludeEvaluator.Instance;
        var query = new Query<Company>();
        query.IgnoreAutoIncludes();

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result.ToList());
        Assert.True(query.IgnoreAutoIncludes);
    }

    [Fact]
    public void GetQuery_IgnoreAutoIncludesFalse_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = IgnoreAutoIncludeEvaluator.Instance;
        var query = new Query<Company> { IgnoreAutoIncludes = false };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result.ToList());
        Assert.False(query.IgnoreAutoIncludes);
    }

    [Fact]
    public void GetQuery_NullQuery_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = IgnoreAutoIncludeEvaluator.Instance;

        // Act
        var result = evaluator.GetQuery(_companies, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result.ToList());
    }

    [Fact]
    public void GetQuery_EmptyQueryable_ReturnsEmpty()
    {
        // Arrange
        var evaluator = IgnoreAutoIncludeEvaluator.Instance;
        var query = new Query<Company>();
        var emptyQueryable = new List<Company>().AsQueryable();
        query.IgnoreAutoIncludes();

        // Act
        var result = evaluator.GetQuery(emptyQueryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetQuery_SingleElementQueryable_ReturnsSingleElement()
    {
        // Arrange
        var evaluator = IgnoreAutoIncludeEvaluator.Instance;
        var singleQueryable = new List<Company> { new() { Id = 99, Name = "Single" } }.AsQueryable();
        var query = new Query<Company>();
        query.IgnoreAutoIncludes();

        // Act
        var result = evaluator.GetQuery(singleQueryable, query);

        // Assert
        Assert.Single(result);
        Assert.Equal(99, result.First().Id);
    }

    [Fact]
    public void GetQuery_DuplicateElementsQueryable_ReturnsAllElements()
    {
        // Arrange
        var evaluator = IgnoreAutoIncludeEvaluator.Instance;
        var duplicateQueryable = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 1, Name = "Company 1" }
        }.AsQueryable();
        var query = new Query<Company>();
        query.IgnoreAutoIncludes();

        // Act
        var result = evaluator.GetQuery(duplicateQueryable, query);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal(1, c.Id));
    }

    [Fact]
    public void GetQuery_DifferentEntityType_DoesNotThrow()
    {
        // Arrange
        var evaluator = IgnoreAutoIncludeEvaluator.Instance;
        var countryQueryable = new List<Country> { new() { Id = 5, Name = "Country" } }.AsQueryable();
        var query = new Query<Country>();
        query.IgnoreAutoIncludes();

        // Act & Assert
        var result = evaluator.GetQuery(countryQueryable, query);
        Assert.Single(result);
        Assert.Equal(5, result.First().Id);
    }

    [Fact]
    public void GetQuery_NullQueryable_ThrowsArgumentNullException()
    {
        // Arrange
        var evaluator = IgnoreAutoIncludeEvaluator.Instance;
        var query = new Query<Company>();
        query.IgnoreAutoIncludes();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.GetQuery<Company>(null!, query));
    }
}
