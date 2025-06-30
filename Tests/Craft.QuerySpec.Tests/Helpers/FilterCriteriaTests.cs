using System;
using System.Linq.Expressions;
using Xunit;

namespace Craft.QuerySpec.Tests.Helpers;

public class FilterCriteriaTests
{
    [Fact]
    public void GetExpression_WithFilterInfo_ReturnsValidExpression()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(int).FullName!, 
            "Length", "1", ComparisonType.EqualTo);

        // Act
        var expression = filterInfo.GetExpression<DummyClass>();

        // Assert
        Assert.Equal(ExpressionType.Equal, expression.Body.NodeType);
    }

    [Fact]
    public void GetFilterInfo_WithPropertyExpression_ReturnsCorrectFilterInfo()
    {
        // Arrange
        Expression<Func<DummyClass, object>> propertyExpression = s => s.Length;

        // Act
        var filterInfo = FilterCriteria.GetFilterInfo(propertyExpression, 10, 
            ComparisonType.GreaterThan);

        // Assert
        Assert.Equal(typeof(int).FullName, filterInfo.TypeName);
        Assert.Equal("Length", filterInfo.Name);
        Assert.Equal("10", filterInfo.Value);
        Assert.Equal(ComparisonType.GreaterThan, filterInfo.Comparison);
    }

    [Fact]
    public void GetFilterInfo_WithValidWhereExpression_ReturnsCorrectFilterInfo()
    {
        // Arrange
        Expression<Func<DummyClass, bool>> whereExpression = s => s.Length > 10;

        // Act
        var filterInfo = FilterCriteria.GetFilterInfo(whereExpression);

        // Assert
        Assert.Equal(typeof(int).FullName, filterInfo.TypeName);
        Assert.Equal("Length", filterInfo.Name);
        Assert.Equal("10", filterInfo.Value);
        Assert.Equal(ComparisonType.GreaterThan, filterInfo.Comparison);
    }

    [Fact]
    public void GetFilterInfo_WithEnumProperty_ReturnsIntTypeName()
    {
        // Arrange
        Expression<Func<EnumClass, object>> propertyExpression = e => e.Status;

        // Act
        var filterInfo = FilterCriteria.GetFilterInfo(propertyExpression, 
            StatusEnum.Active, ComparisonType.EqualTo);

        // Assert
        Assert.Equal(typeof(int).FullName, filterInfo.TypeName);
        Assert.Equal("Status", filterInfo.Name);
        Assert.Equal(((int)StatusEnum.Active).ToString(), filterInfo.Value);
        Assert.Equal(ComparisonType.EqualTo, filterInfo.Comparison);
    }

    [Fact]
    public void GetFilterInfo_WithNullableProperty_ReturnsUnderlyingTypeName()
    {
        // Arrange
        Expression<Func<NullableClass, object>> propertyExpression = n => n.Age!;

        // Act
        var filterInfo = FilterCriteria.GetFilterInfo(propertyExpression, 25, 
            ComparisonType.EqualTo);

        // Assert
        Assert.Equal(typeof(int).FullName, filterInfo.TypeName);
        Assert.Equal("Age", filterInfo.Name);
        Assert.Equal("25", filterInfo.Value);
        Assert.Equal(ComparisonType.EqualTo, filterInfo.Comparison);
    }

    [Fact]
    public void GetFilterInfo_WithInvalidPropertyExpression_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<DummyClass, object>> propertyExpression = s => s;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => FilterCriteria.GetFilterInfo(propertyExpression, 
            1, ComparisonType.EqualTo));
    }

    [Fact]
    public void GetFilterInfo_WithInvalidWhereExpression_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<DummyClass, bool>> whereExpression = s => s.Length > 10 && s.Length < 20;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => FilterCriteria.GetFilterInfo(whereExpression));
    }

    [Fact]
    public void ParseExpression_WithUnsupportedOperator_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<DummyClass, bool>> whereExpression = s => s.Length != 10;

        // Act & Assert
        var filterInfo = FilterCriteria.GetFilterInfo(whereExpression);

        // Assert
        Assert.Equal(ComparisonType.NotEqualTo, filterInfo.Comparison);
    }

    private class DummyClass
    {
        public int Length { get; set; }
    }

    private class NullableClass
    {
        public int? Age { get; set; }
    }

    private class EnumClass
    {
        public StatusEnum Status { get; set; }
    }

    private enum StatusEnum
    {
        Inactive = 0,
        Active = 1
    }
}
