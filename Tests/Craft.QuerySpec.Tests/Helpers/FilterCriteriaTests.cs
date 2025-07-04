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
    public void ParseExpression_WithNotEqualOperator_ReturnsNotEqualComparisonType()
    {
        // Arrange
        Expression<Func<DummyClass, bool>> whereExpression = s => s.Length != 10;

        // Act
        var filterInfo = FilterCriteria.GetFilterInfo(whereExpression);

        // Assert
        Assert.Equal(ComparisonType.NotEqualTo, filterInfo.Comparison);
    }

    [Fact]
    public void Constructor_WithNullArguments_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FilterCriteria(null!, "Name", "Value"));
        Assert.Throws<ArgumentNullException>(() => new FilterCriteria("TypeName", null!, "Value"));
        Assert.Throws<ArgumentNullException>(() => new FilterCriteria("TypeName", "Name", null!));
    }

    [Fact]
    public void GetExpression_WithNullFilterInfo_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => FilterCriteria.GetExpression<DummyClass>(null!));
    }

    [Fact]
    public void GetFilterInfo_WithNullPropertyExpression_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => FilterCriteria.GetFilterInfo<DummyClass>(null!, 1, ComparisonType.EqualTo));
    }

    [Fact]
    public void GetFilterInfo_WithNullCompareWith_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<DummyClass, object>> propertyExpression = s => s.Length;
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FilterCriteria.GetFilterInfo(propertyExpression, null!, ComparisonType.EqualTo));
    }

    [Fact]
    public void GetFilterInfo_WithNullWhereExpression_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => FilterCriteria.GetFilterInfo<DummyClass>(null!));
    }

    [Fact]
    public void ParseExpression_WithNonBinaryExpression_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<DummyClass, bool>> expr = s => true;
        // Act & Assert
        Assert.Throws<ArgumentException>(() => FilterCriteria.GetFilterInfo(expr));
    }

    [Fact]
    public void ParseExpression_WithNonMemberLeft_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<DummyClass, bool>> expr = s => (s.Length + 1) == 2;
        // Act & Assert
        Assert.Throws<ArgumentException>(() => FilterCriteria.GetFilterInfo(expr));
    }

    [Fact]
    public void ParseExpression_WithNonConstantRight_ThrowsArgumentException()
    {
        // Arrange
        int value = 2;
        Expression<Func<DummyClass, bool>> expr = s => s.Length == value;
        // Act & Assert
        Assert.Throws<ArgumentException>(() => FilterCriteria.GetFilterInfo(expr));
    }

    [Fact]
    public void ParseExpression_WithUnsupportedOperator_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<DummyClass, bool>> expr = s => s.Length * 2 == 4;
        // Act & Assert
        Assert.Throws<ArgumentException>(() => FilterCriteria.GetFilterInfo(expr));
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
