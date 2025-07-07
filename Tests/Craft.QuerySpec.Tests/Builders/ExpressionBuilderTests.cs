using System.Linq.Expressions;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Builders;

public class ExpressionBuilderTests
{
    private readonly IQueryable<Company> queryable;

    public ExpressionBuilderTests()
    {
        queryable = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 2, Name = "Company 2" }
        }.AsQueryable();
    }

    [Fact]
    public void CreateNonStringExpressionBody_ShouldReturnCorrectComparisonExpression()
    {
        // Arrange
        FilterCriteria filterInfo = new(typeof(long).FullName!, "Id", "1", ComparisonType.EqualTo);

        // Act
        var expression = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expression).ToList();

        // Assert
        Assert.NotNull(expression);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void CreateWhereExpression_ShouldCreateValidExpression()
    {
        // Arrange
        Expression<Func<Company, object>> propertyExpression = (x) => x.Name!;
        const string dataValue = "Company 1";
        const ComparisonType comparison = ComparisonType.EqualTo;

        // Act
        var expression = ExpressionBuilder.CreateWhereExpression(propertyExpression, dataValue, comparison);
        var result = queryable.Where(expression).ToList();

        // Assert
        Assert.NotNull(expression);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void CreateWhereExpression_ShouldReturnCorrectExpression()
    {
        // Arrange
        FilterCriteria filterInfo = new(typeof(string).FullName!, "Name", "Company 1", ComparisonType.EqualTo);

        // Act
        var expression = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expression).ToList();

        // Assert
        Assert.NotNull(expression);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void GetPropertyExpression_ShouldReturnLambdaExpressionForValidProperty()
    {
        // Arrange
        Company instance = new() { Name = "Sample" };

        // Act
        var expression = ExpressionBuilder.GetPropertyExpression<Company>("Name");

        // Assert
        Assert.NotNull(expression);
        var compiled = expression.Compile();
        Assert.Equal("Sample", compiled(instance));
    }

    [Fact]
    public void GetPropertyExpression_ShouldReturnNullForInvalidProperty()
    {
        // Act
        var expression = ExpressionBuilder.GetPropertyExpression<Company>("InvalidProperty");

        // Assert
        Assert.Null(expression);
    }

    [Fact]
    public void CreateWhereExpression_InvalidPropertyName_ThrowsArgumentException()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Invalid", "Test", ComparisonType.EqualTo);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<Company>(filterInfo));
    }

    [Fact]
    public void CreateWhereExpression_InvalidTypeName_ThrowsTypeLoadException()
    {
        // Arrange
        var filterInfo = new FilterCriteria("NotAType", "Name", "Test", ComparisonType.EqualTo);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<Company>(filterInfo));
    }

    [Fact]
    public void CreateWhereExpression_StringContainsComparison_Works()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "Company", ComparisonType.Contains);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void CreateWhereExpression_StringStartsWithComparison_Works()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "Company", ComparisonType.StartsWith);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void CreateWhereExpression_StringEndsWithComparison_Works()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "1", ComparisonType.EndsWith);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void CreateWhereExpression_StringNotEqualToComparison_Works()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "Company 1", ComparisonType.NotEqualTo);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Company 2", result[0].Name);
    }

    [Fact]
    public void CreateWhereExpression_IntGreaterThanComparison_Works()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(long).FullName!, "Id", "1", ComparisonType.GreaterThan);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public void GetPropertyExpression_PrimitiveType_ReturnsNull()
    {
        // Act
        var expr = ExpressionBuilder.GetPropertyExpression<int>("Length");

        // Assert
        Assert.Null(expr);
    }

    [Fact]
    public void CreateWhereExpression_PropertyExpressionWithInvalidProperty_Throws()
    {
        // Arrange
        Expression<Func<Company, object>> propExpr = x => x.GetType(); // Not a mapped property

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression(propExpr, "Test", ComparisonType.EqualTo));
    }

    [Fact]
    public void GetPropertyExpression_NullOrWhitespacePropertyName_ReturnsNull()
    {
        // Act & Assert
        Assert.Null(ExpressionBuilder.GetPropertyExpression<Company>(null!));
        Assert.Null(ExpressionBuilder.GetPropertyExpression<Company>(""));
        Assert.Null(ExpressionBuilder.GetPropertyExpression<Company>("   "));
    }

    [Fact]
    public void CreateWhereExpression_EmptyTypeName_ThrowsArgumentException()
    {
        // Arrange
        var filterInfo = new FilterCriteria("", "Name", "Test", ComparisonType.EqualTo);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<Company>(filterInfo));
    }

    [Fact]
    public void CreateWhereExpression_CaseInsensitiveStringComparison_Works()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "company 1", ComparisonType.EqualTo);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void CreateWhereExpression_UnsupportedComparisonTypeForString_Throws()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "Company 1", (ComparisonType)999);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<Company>(filterInfo));
    }

    [Fact]
    public void CreateWhereExpression_UnsupportedComparisonTypeForNonString_UsesEqual()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(long).FullName!, "Id", "1", (ComparisonType)999);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    public class NullableTest
    {
        public int? Age { get; set; }
    }

    [Fact]
    public void CreateWhereExpression_NullableProperty_Works()
    {
        // Arrange
        var data = new[] { new NullableTest { Age = 5 }, new NullableTest { Age = null } }.AsQueryable();

        // Act
        var filterInfo = new FilterCriteria(typeof(int).FullName!, "Age", "5", ComparisonType.EqualTo);
        var expr = ExpressionBuilder.CreateWhereExpression<NullableTest>(filterInfo);
        var result = data.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(5, result[0].Age);
    }

    public class BoolTest
    {
        public bool IsActive { get; set; }
    }

    [Fact]
    public void CreateWhereExpression_BoolProperty_Works()
    {
        // Arrange
        var data = new[] { new BoolTest { IsActive = true }, new BoolTest { IsActive = false } }.AsQueryable();

        // Act
        var filterInfo = new FilterCriteria(typeof(bool).FullName!, "IsActive", "true", ComparisonType.EqualTo);
        var expr = ExpressionBuilder.CreateWhereExpression<BoolTest>(filterInfo);
        var result = data.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.True(result[0].IsActive);
    }

    public class DateTest
    {
        public DateTime Date { get; set; }
        public DateOnly DOnly { get; set; }
        public TimeOnly TOnly { get; set; }
    }

    [Fact]
    public void CreateWhereExpression_DateTimeProperty_Works()
    {
        // Arrange
        var dt = DateTime.Today;

        // Act
        var data = new[] { new DateTest { Date = dt }, new DateTest { Date = dt.AddDays(1) } }.AsQueryable();
        var filterInfo = new FilterCriteria(typeof(DateTime).FullName!, "Date", dt.ToString("d"), ComparisonType.EqualTo);
        var expr = ExpressionBuilder.CreateWhereExpression<DateTest>(filterInfo);
        var result = data.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(dt, result[0].Date);
    }

    [Fact]
    public void CreateWhereExpression_DateOnlyProperty_Works()
    {
        // Arrange
        var d = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var data = new[] { new DateTest { DOnly = d }, new DateTest { DOnly = d.AddDays(1) } }.AsQueryable();
        var filterInfo = new FilterCriteria(typeof(DateOnly).FullName!, "DOnly", d.ToString(), ComparisonType.EqualTo);
        var expr = ExpressionBuilder.CreateWhereExpression<DateTest>(filterInfo);
        var result = data.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(d, result[0].DOnly);
    }

    //[Fact]
    //public void CreateWhereExpression_TimeOnlyProperty_Works()
    //{
    //    // Arrange
    //    var t = TimeOnly.FromDateTime(DateTime.Now);

    //    // Act
    //    var data = new[] { new DateTest { TOnly = t }, new DateTest { TOnly = t.AddHours(1) } }.AsQueryable();
    //    var filterInfo = new FilterCriteria(typeof(TimeOnly).FullName!, "TOnly", t.ToString(), ComparisonType.EqualTo);
    //    var expr = ExpressionBuilder.CreateWhereExpression<DateTest>(filterInfo);
    //    var result = data.Where(expr).ToList();

    //    // Assert
    //    Assert.Single(result);
    //    Assert.Equal(t, result[0].TOnly);
    //}

    [Fact]
    public void CreateWhereExpression_InvalidDateValue_Throws()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(DateTime).FullName!, "Date", "notadate", ComparisonType.EqualTo);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<DateTest>(filterInfo));
    }

    [Fact]
    public void CreateWhereExpression_NullValue_DoesNotThrows()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", null!, ComparisonType.EqualTo);

        // Act & Assert
        var ex = Record.Exception(() => ExpressionBuilder.CreateWhereExpression<Company>(filterInfo));
        Assert.Null(ex);
    }

    [Fact]
    public void CreateWhereExpression_PropertyNameCasingMismatch_Works()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "name", "Company 1", ComparisonType.EqualTo);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    public class NavTest
    {
        public Company? Company { get; set; }
    }

    [Fact]
    public void CreateWhereExpression_NavigationProperty_Throws()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(Company).FullName!, "Company", "1", ComparisonType.EqualTo);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<NavTest>(filterInfo));
    }

    public class WriteOnlyTest
    {
        private string _value = "test";
        public string Value { set { _value = value; } }
    }

    [Fact]
    public void CreateWhereExpression_WriteOnlyProperty_Throws()
    {
        // Arrange
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Value", "test", ComparisonType.EqualTo);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<WriteOnlyTest>(filterInfo));
    }

    public class DefaultValueTypeTest
    {
        public int Number { get; set; }
    }

    [Fact]
    public void CreateWhereExpression_ValueTypeDefaultValue_Works()
    {
        // Arrange
        var data = new[] { new DefaultValueTypeTest { Number = 0 }, new DefaultValueTypeTest { Number = 1 } }.AsQueryable();

        // Act
        var filterInfo = new FilterCriteria(typeof(int).FullName!, "Number", "0", ComparisonType.EqualTo);
        var expr = ExpressionBuilder.CreateWhereExpression<DefaultValueTypeTest>(filterInfo);
        var result = data.Where(expr).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(0, result[0].Number);
    }

    public class NullStringTest
    {
        public string? Name { get; set; }
    }
}
