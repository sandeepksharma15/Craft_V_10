using System.Linq.Expressions;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Core;

public class QuerySelectExtensionsTests
{
    [Fact]
    public void Select_Should_AddColumnToQuerySelectBuilder()
    {
        // Arrange
        var query = new Query<Company, Company>();
        Expression<Func<Company, object>> column = x => x.Name!;

        // Act
        var result = query.Select(column);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(query, result);
        Assert.Single(result.QuerySelectBuilder?.SelectDescriptorList!);
        Assert.Contains("Name", result.QuerySelectBuilder?.SelectDescriptorList[0].Assignor?.Body.ToString());
        Assert.Contains("Name", result.QuerySelectBuilder?.SelectDescriptorList[0].Assignee?.Body.ToString());
    }

    [Fact]
    public void Select_Should_ReturnQueryUnchanged_WhenQueryIsNull()
    {
        // Arrange
        IQuery<Company, Company> query = null!;
        Expression<Func<Company, object>> column = x => x.Name!;

        // Act
        var result = query.Select(column);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Select_Should_ReturnQueryUnchanged_WhenColumnIsNull()
    {
        // Arrange
        var query = new Query<Company, Company>();
        Expression<Func<Company, object>> column = null!;

        // Act
        var result = query.Select(column);

        // Assert
        Assert.Equal(query, result);
    }

    [Fact]
    public void Select_Should_AddAssignorAndAssigneeToQuerySelectBuilder()
    {
        // Arrange
        var query = new Query<Company, MyResult>();
        Expression<Func<Company, object>> assignor = x => x.Name!;
        Expression<Func<MyResult, object>> assignee = x => x.Name;

        // Act
        var result = query.Select(assignor, assignee);

        // Assert
        Assert.Equal(query, result);
        Assert.Single(result?.QuerySelectBuilder?.SelectDescriptorList!);
        Assert.Contains("Name", result?.QuerySelectBuilder?.SelectDescriptorList[0].Assignor?.Body.ToString());
        Assert.Contains("Name", result?.QuerySelectBuilder?.SelectDescriptorList[0].Assignee?.Body.ToString());
    }

    [Fact]
    public void Select_Should_AddAssignorPropNameToQuerySelectBuilder()
    {
        // Arrange
        var query = new Query<Company, MyResult>();

        // Act
        var result = query.Select("Name");

        // Assert
        Assert.Equal(query, result);
        Assert.Single(result?.QuerySelectBuilder?.SelectDescriptorList!);
        Assert.Contains("Name", result?.QuerySelectBuilder?.SelectDescriptorList[0].Assignor?.Body.ToString());
        Assert.Contains("Name", result?.QuerySelectBuilder?.SelectDescriptorList[0].Assignee?.Body.ToString());
    }

    [Fact]
    public void Select_Should_AddAssignorPropNameAndAssigneePropNameToQuerySelectBuilder()
    {
        // Arrange
        var query = new Query<Company, MyResult>();

        // Act
        var result = query.Select("Name", "Name");

        // Assert
        Assert.Equal(query, result);
        Assert.Single(result?.QuerySelectBuilder?.SelectDescriptorList!);
        Assert.Contains("Name", result?.QuerySelectBuilder?.SelectDescriptorList[0].Assignor?.Body.ToString());
        Assert.Contains("Name", result?.QuerySelectBuilder?.SelectDescriptorList[0].Assignee?.Body.ToString());
    }

    private class MyResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
