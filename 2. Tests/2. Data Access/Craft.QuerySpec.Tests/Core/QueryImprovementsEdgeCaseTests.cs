using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Core;

/// <summary>
/// Comprehensive edge case tests for all improvements made to Query and QuerySpec.
/// </summary>
public class QueryImprovementsEdgeCaseTests
{
    [Fact]
    public void Skip_WithMaxIntValue_DoesNotThrow()
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        query.Skip = int.MaxValue;

        // Assert
        Assert.Equal(int.MaxValue, query.Skip);
    }

    [Fact]
    public void Take_WithMaxIntValue_DoesNotThrow()
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        query.Take = int.MaxValue;

        // Assert
        Assert.Equal(int.MaxValue, query.Take);
    }

    [Fact]
    public void SetPage_WithMaxIntValues_CausesOverflow()
    {
        // Arrange
        var query = new Query<Company>();

        // Act & Assert - This will overflow and cause negative Skip value
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => query.SetPage(int.MaxValue, int.MaxValue));
        Assert.Contains("Skip cannot be negative", exception.Message);
    }

    [Fact]
    public void Clear_CalledMultipleTimes_RemainsConsistent()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(c => c.Name == "Test");
        query.OrderBy(c => c.Id);
        query.Skip = 100;
        query.Take = 50;

        // Act
        query.Clear();
        query.Clear();
        query.Clear();

        // Assert
        Assert.Null(query.Skip);
        Assert.Null(query.Take);
        Assert.Empty(query.EntityFilterBuilder!.EntityFilterList);
        Assert.Empty(query.SortOrderBuilder!.OrderDescriptorList);
    }

    [Fact]
    public void ToString_WithComplexQuery_ReturnsDetailedString()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(c => c.Name!.StartsWith("A"));
        query.Where(c => c.Id > 100);
        query.OrderBy(c => c.Name!);
        query.OrderBy(c => c.Id); // This becomes ThenOrderBy automatically via SortOrderBuilder
        query.Search(c => c.Name!, "test%");
        query.Skip = 20;
        query.Take = 10;
        query.AsSplitQuery = true;
        query.IgnoreQueryFilters = true;

        // Act
        var result = query.ToString();

        // Assert
        Assert.Contains("Query<Company>", result);
        Assert.Contains("AsNoTracking: True", result);
        Assert.Contains("AsSplitQuery: True", result);
        Assert.Contains("IgnoreQueryFilters: True", result);
        Assert.Contains("Skip: 20, Take: 10", result);
        Assert.Contains("Filters: 2", result);
        Assert.Contains("Orders:", result);
        Assert.Contains("Search Criteria: 1", result);
    }

    [Fact]
    public void ToString_WithMinimalQuery_ReturnsBasicString()
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        var result = query.ToString();

        // Assert
        Assert.Contains("Query<Company>", result);
        Assert.Contains("Skip: null, Take: null", result);
        // Note: ToString() may include "Filters: 0" or omit it - both are acceptable
        Assert.DoesNotContain("Orders:", result);
        Assert.DoesNotContain("Search Criteria:", result);
    }

    [Fact]
    public void IsSatisfiedBy_WithMultipleFilters_EvaluatesAllFilters()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(c => c.Id > 0);
        query.Where(c => c.Name!.Length > 2);
        query.Where(c => c.Name!.StartsWith("C"));
        var company = new Company { Id = 1, Name = "Company" };

        // Act
        var result = query.IsSatisfiedBy(company);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithFailingFilter_ReturnsFalse()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(c => c.Id > 0);
        query.Where(c => c.Name == "NotMatching");
        var company = new Company { Id = 1, Name = "Company" };

        // Act
        var result = query.IsSatisfiedBy(company);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_IgnoresOrdering_EvaluatesOnlyFilters()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(c => c.Id == 1);
        query.OrderByDescending(c => c.Name!); // Should be ignored
        var company = new Company { Id = 1, Name = "Company" };

        // Act
        var result = query.IsSatisfiedBy(company);

        // Assert - Should pass even though ordering is present
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_IgnoresPagination_EvaluatesOnlyFilters()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(c => c.Id == 1);
        query.Skip = 1000; // Should be ignored
        query.Take = 1; // Should be ignored
        var company = new Company { Id = 1, Name = "Company" };

        // Act
        var result = query.IsSatisfiedBy(company);

        // Assert - Should pass even though pagination would exclude it
        Assert.True(result);
    }

    [Fact]
    public void Skip_SetToNull_ResetsValue()
    {
        // Arrange
        var query = new Query<Company> { Skip = 100 };

        // Act
        query.Skip = null;

        // Assert
        Assert.Null(query.Skip);
    }

    [Fact]
    public void Take_SetToNull_AcceptsNull()
    {
        // Arrange
        var query = new Query<Company> { Take = 100 };

        // Act
        query.Take = null;

        // Assert
        Assert.Null(query.Take);
    }

    [Fact]
    public void SetPage_WithPage1AndPageSize1_SetsSkipToZero()
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        query.SetPage(1, 1);

        // Assert
        Assert.Equal(0, query.Skip);
        Assert.Equal(1, query.Take);
    }

    [Fact]
    public void SetPage_WithPage2AndPageSize1_SetsSkipToOne()
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        query.SetPage(2, 1);

        // Assert
        Assert.Equal(1, query.Skip);
        Assert.Equal(1, query.Take);
    }

    [Fact]
    public void QueryWithProjection_Clear_ResetsProjectionAndBase()
    {
        // Arrange
        var query = new Query<Company, CompanyDto>();
        query.Where(c => c.Name == "Test");
        query.Select(c => c.Name!, d => d.Name!);
        query.SelectorMany = c => new List<CompanyDto>();
        query.Skip = 10;
        query.Take = 20;

        // Act
        query.Clear();

        // Assert
        Assert.Null(query.Skip);
        Assert.Null(query.Take);
        Assert.Equal(0, query.QuerySelectBuilder!.Count);
        Assert.Null(query.SelectorMany);
        Assert.Empty(query.EntityFilterBuilder!.EntityFilterList);
    }

    [Fact]
    public void PostProcessingAction_WithNullValue_DoesNotThrow()
    {
        // Arrange
        var query = new Query<Company> { PostProcessingAction = null };

        // Act & Assert
        var exception = Record.Exception(() => query.PostProcessingAction?.Invoke(new List<Company>()));
        Assert.Null(exception);
    }

    [Fact]
    public void PostProcessingAction_WithEmptyList_Works()
    {
        // Arrange
        var query = new Query<Company>
        {
            PostProcessingAction = items => items.Take(10)
        };

        // Act
        var result = query.PostProcessingAction(new List<Company>());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void AllBuilders_Count_AfterMultipleAddsAndRemoves_RemainsAccurate()
    {
        // Arrange & Act
        var sortBuilder = new SortOrderBuilder<Company>();
        sortBuilder.Add(c => c.Name!);
        sortBuilder.Add(c => c.Id);
        Assert.Equal(2, sortBuilder.Count);
        sortBuilder.Remove(c => c.Name!);
        Assert.Equal(1, sortBuilder.Count);

        var filterBuilder = new EntityFilterBuilder<Company>();
        filterBuilder.Add(c => c.Name == "A");
        filterBuilder.Add(c => c.Id == 1);
        filterBuilder.Add(c => c.Name!.Length > 0);
        Assert.Equal(3, filterBuilder.Count);
        filterBuilder.Remove(c => c.Name == "A");
        Assert.Equal(2, filterBuilder.Count);

        var selectBuilder = new QuerySelectBuilder<Company, CompanyDto>();
        selectBuilder.Add(c => c.Name!, d => d.Name!);
        selectBuilder.Add(c => c.Id, d => d.Id);
        Assert.Equal(2, selectBuilder.Count);
        selectBuilder.Clear();
        Assert.Equal(0, selectBuilder.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void SetPage_WithInvalidPage_ThrowsWithCorrectMessage(int invalidPage)
    {
        // Arrange
        var query = new Query<Company>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => query.SetPage(invalidPage, 10));
        Assert.Equal("page", exception.ParamName);
        Assert.Contains("Page number must be 1 or greater", exception.Message);
        Assert.Contains(invalidPage.ToString(), exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void SetPage_WithInvalidPageSize_ThrowsWithCorrectMessage(int invalidPageSize)
    {
        // Arrange
        var query = new Query<Company>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => query.SetPage(1, invalidPageSize));
        Assert.Equal("pageSize", exception.ParamName);
        Assert.Contains("Page size must be 1 or greater", exception.Message);
        Assert.Contains(invalidPageSize.ToString(), exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Skip_WithNegativeValue_ThrowsWithCorrectMessage(int invalidSkip)
    {
        // Arrange
        var query = new Query<Company>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => query.Skip = invalidSkip);
        Assert.Equal("Skip", exception.ParamName);
        Assert.Contains("Skip cannot be negative", exception.Message);
        Assert.Contains(invalidSkip.ToString(), exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Take_WithInvalidValue_ThrowsWithCorrectMessage(int invalidTake)
    {
        // Arrange
        var query = new Query<Company>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => query.Take = invalidTake);
        Assert.Equal("Take", exception.ParamName);
        Assert.Contains("Take must be greater than zero", exception.Message);
        Assert.Contains(invalidTake.ToString(), exception.Message);
    }

    private class CompanyDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
