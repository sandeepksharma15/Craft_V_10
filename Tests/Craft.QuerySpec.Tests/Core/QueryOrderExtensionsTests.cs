using System.Linq.Expressions;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Core;

public class QueryOrderExtensionsTests
{
    [Fact]
    public void OrderBy_WithNullQuery_ShouldReturnNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.OrderBy(x => x.Name!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void OrderBy_WithNullExpression_ShouldReturnQuery()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = null!;

        // Act
        var result = query.OrderBy(propExpr);

        // Assert
        Assert.Same(query, result);
    }

    [Fact]
    public void OrderBy_WithValidParameters_ShouldAddToOrderBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = x => x.Name!;

        // Act
        var result = query.OrderBy(propExpr);

        // Assert
        Assert.Same(query, result);
        Assert.Single(query.SortOrderBuilder?.OrderDescriptorList!);
        Assert.NotNull(query.SortOrderBuilder?.OrderDescriptorList[0].OrderItem);
        Assert.Equal("x.Name", query.SortOrderBuilder.OrderDescriptorList[0].OrderItem.Body.ToString());
        Assert.Equal(OrderTypeEnum.OrderBy, query.SortOrderBuilder.OrderDescriptorList[0].OrderType);
    }

    [Fact]
    public void OrderByDescending_WithNullQuery_ShouldReturnNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.OrderByDescending(x => x.Name!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void OrderByDescending_WithNullExpression_ShouldReturnQuery()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = null!;

        // Act
        var result = query.OrderByDescending(propExpr);

        // Assert
        Assert.Same(query, result);
    }

    [Fact]
    public void OrderByDescending_WithValidParameters_ShouldAddToOrderBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = x => x.Name!;

        // Act
        var result = query.OrderByDescending(propExpr);

        // Assert
        Assert.Same(query, result);
        Assert.Single(query.SortOrderBuilder?.OrderDescriptorList!);
        Assert.NotNull(query.SortOrderBuilder?.OrderDescriptorList[0].OrderItem);
        Assert.Equal("x.Name", query.SortOrderBuilder.OrderDescriptorList[0].OrderItem.Body.ToString());
        Assert.Equal(OrderTypeEnum.OrderByDescending, query.SortOrderBuilder.OrderDescriptorList[0].OrderType);
    }

    [Fact]
    public void ThenBy_WithNullQuery_ShouldReturnNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.ThenBy(x => x.Name!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ThenBy_WithNullExpression_ShouldReturnQuery()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = null!;

        // Act
        var result = query.ThenBy(propExpr);

        // Assert
        Assert.Same(query, result);
    }

    [Fact]
    public void ThenBy_WithValidParameters_ShouldAddToOrderBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = x => x.Name!;

        // Act
        var result = query.ThenBy(propExpr);

        // Assert
        Assert.Same(query, result);
        Assert.Single(query.SortOrderBuilder?.OrderDescriptorList!);
        Assert.NotNull(query.SortOrderBuilder?.OrderDescriptorList[0].OrderItem);
        Assert.Equal("x.Name", query.SortOrderBuilder.OrderDescriptorList[0].OrderItem.Body.ToString());
        Assert.Equal(OrderTypeEnum.ThenBy, query.SortOrderBuilder.OrderDescriptorList[0].OrderType);
    }

    [Fact]
    public void ThenByDescending_WithNullQuery_ShouldReturnNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.ThenByDescending(x => x.Name!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ThenByDescending_WithNullExpression_ShouldReturnQuery()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = null!;

        // Act
        var result = query.ThenByDescending(propExpr);

        // Assert
        Assert.Same(query, result);
    }

    [Fact]
    public void ThenByDescending_WithValidParameters_ShouldAddToOrderBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = x => x.Name!;

        // Act
        var result = query.ThenByDescending(propExpr);

        // Assert
        Assert.Same(query, result);
        Assert.Single(query.SortOrderBuilder?.OrderDescriptorList!);
        Assert.NotNull(query.SortOrderBuilder?.OrderDescriptorList[0].OrderItem);
        Assert.Equal("x.Name", query.SortOrderBuilder.OrderDescriptorList[0].OrderItem.Body.ToString());
        Assert.Equal(OrderTypeEnum.ThenByDescending, query.SortOrderBuilder.OrderDescriptorList[0].OrderType);
    }

    [Fact]
    public void OrderBy_WithNullQuery_ShouldReturnNull_WithPropertyName()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.OrderBy("Name");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void OrderBy_WithNullPropertyName_ShouldReturnQuery()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        string propName = null!;

        // Act
        var result = query.OrderBy(propName);

        // Assert
        Assert.Same(query, result);
    }

    [Fact]
    public void OrderBy_WithValidParameters_ShouldAddToOrderBuilder_WithPropertyName()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        const string propName = "Name";

        // Act
        var result = query.OrderBy(propName);

        // Assert
        Assert.Same(query, result);
        Assert.Single(query.SortOrderBuilder?.OrderDescriptorList!);
        Assert.NotNull(query.SortOrderBuilder?.OrderDescriptorList[0].OrderItem);
        Assert.Equal(OrderTypeEnum.OrderBy, query.SortOrderBuilder.OrderDescriptorList[0].OrderType);
    }

    [Fact]
    public void OrderByDescending_WithNullQuery_ShouldReturnNull_WithPropertyName()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.OrderByDescending("Name");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void OrderByDescending_WithNullPropertyName_ShouldReturnQuery()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        string propName = null!;

        // Act
        var result = query.OrderByDescending(propName);

        // Assert
        Assert.Same(query, result);
    }

    [Fact]
    public void OrderByDescending_WithValidParameters_ShouldAddToOrderBuilder_WithPropertyName()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        const string propName = "Name";

        // Act
        var result = query.OrderByDescending(propName);

        // Assert
        Assert.Same(query, result);
        Assert.Single(query.SortOrderBuilder!.OrderDescriptorList);
        Assert.NotNull(query.SortOrderBuilder.OrderDescriptorList[0].OrderItem);
        Assert.Equal(OrderTypeEnum.OrderByDescending, query.SortOrderBuilder.OrderDescriptorList[0].OrderType);
    }

    [Fact]
    public void ThenBy_WithNullQuery_ShouldReturnNull_WithPropertyName()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.ThenBy("Name");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ThenBy_WithNullPropertyName_ShouldReturnQuery()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        string propName = null!;

        // Act
        var result = query.ThenBy(propName);

        // Assert
        Assert.Same(query, result);
    }

    [Fact]
    public void ThenBy_WithValidParameters_ShouldAddToOrderBuilder_WithPropertyName()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        const string propName = "Name";

        // Act
        var result = query.ThenBy(propName);

        // Assert
        Assert.Same(query, result);
        Assert.Single(query.SortOrderBuilder?.OrderDescriptorList!);
        Assert.NotNull(query.SortOrderBuilder?.OrderDescriptorList[0].OrderItem);
        Assert.Equal(OrderTypeEnum.ThenBy, query.SortOrderBuilder.OrderDescriptorList[0].OrderType);
    }

    [Fact]
    public void ThenByDescending_WithNullQuery_ShouldReturnNull_WithPropertyName()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.ThenByDescending("Name");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ThenByDescending_WithNullPropertyName_ShouldReturnQuery()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        string propName = null!;

        // Act
        var result = query.ThenByDescending(propName);

        // Assert
        Assert.Same(query, result);
    }

    [Fact]
    public void ThenByDescending_WithValidParameters_ShouldAddToOrderBuilder_WithPropertyName()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        const string propName = "Name";

        // Act
        var result = query.ThenByDescending(propName);

        // Assert
        Assert.Same(query, result);
        Assert.Single(query.SortOrderBuilder?.OrderDescriptorList!);
        Assert.NotNull(query.SortOrderBuilder?.OrderDescriptorList[0].OrderItem);
        Assert.Equal(OrderTypeEnum.ThenByDescending, query.SortOrderBuilder.OrderDescriptorList[0].OrderType);
    }

    [Fact]
    public void MultipleOrderBy_Calls_AddsMultipleOrderDescriptors()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        query.OrderBy(x => x.Id);
        query.OrderBy(x => x.Name!);

        // Assert
        Assert.Equal(2, query.SortOrderBuilder?.OrderDescriptorList.Count);
        Assert.Equal(OrderTypeEnum.OrderBy, query.SortOrderBuilder?.OrderDescriptorList[0].OrderType);
        Assert.Equal(OrderTypeEnum.ThenBy, query.SortOrderBuilder?.OrderDescriptorList[1].OrderType);
    }

    [Fact]
    public void Chaining_OrderBy_ThenBy_ThenByDescending_AddsCorrectOrderTypes()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        query.OrderBy(x => x.Id);
        query.ThenBy(x => x.Name!);
        query.ThenByDescending(x => x.CountryId!);

        // Assert
        Assert.Equal(3, query.SortOrderBuilder?.OrderDescriptorList.Count);
        Assert.Equal(OrderTypeEnum.OrderBy, query.SortOrderBuilder?.OrderDescriptorList[0].OrderType);
        Assert.Equal(OrderTypeEnum.ThenBy, query.SortOrderBuilder?.OrderDescriptorList[1].OrderType);
        Assert.Equal(OrderTypeEnum.ThenByDescending, query.SortOrderBuilder?.OrderDescriptorList[2].OrderType);
    }

    [Fact]
    public void OrderBy_WithInvalidPropertyName_DoesNotThrowOrAdd()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => query.OrderBy("NotAProp"));
    }

    [Fact]
    public void OrderBy_WithDuplicateExpressions_ThrowsError()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        query.OrderBy(x => x.Name!);

        // Assert
        Assert.Throws<ArgumentException>(() => query.OrderBy(x => x.Name!));
    }
}
