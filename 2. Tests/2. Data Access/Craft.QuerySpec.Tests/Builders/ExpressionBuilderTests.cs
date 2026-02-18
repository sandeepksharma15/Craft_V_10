using System.Linq.Expressions;

namespace Craft.QuerySpec.Tests.Builders;

public class ExpressionBuilderTests
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public double Score { get; set; }
        public DateTime Created { get; set; }
        public bool IsActive { get; set; }
    }

    #region CreateWhereExpression(FilterCriteria) Tests

    [Fact]
    public void CreateWhereExpression_String_EqualTo_CaseInsensitive()
    {
        var criteria = new FilterCriteria(typeof(string), nameof(TestEntity.Name), "john", ComparisonType.EqualTo);
        var expr = ExpressionBuilder.CreateWhereExpression<TestEntity>(criteria);
        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "JOHN" }));
        Assert.True(compiled(new TestEntity { Name = "john" }));
        Assert.False(compiled(new TestEntity { Name = "jane" }));
    }

    [Theory]
    [InlineData(ComparisonType.Contains, "oh", true, false)]
    [InlineData(ComparisonType.StartsWith, "jo", true, false)]
    [InlineData(ComparisonType.EndsWith, "hn", true, false)]
    [InlineData(ComparisonType.NotEqualTo, "jane", true, false)]
    public void CreateWhereExpression_String_OtherComparisons(ComparisonType cmp, string value, bool match, bool negMatch)
    {
        var criteria = new FilterCriteria(typeof(string), nameof(TestEntity.Name), value, cmp);
        var expr = ExpressionBuilder.CreateWhereExpression<TestEntity>(criteria);
        var compiled = expr.Compile();
        Assert.Equal(match, compiled(new TestEntity { Name = "John" }));
        Assert.Equal(negMatch, compiled(new TestEntity { Name = "Jane" }));
    }

    [Theory]
    [InlineData(ComparisonType.GreaterThan)]
    [InlineData(ComparisonType.GreaterThanOrEqualTo)]
    [InlineData(ComparisonType.LessThan)]
    [InlineData(ComparisonType.LessThanOrEqualTo)]
    public void CreateWhereExpression_String_ThrowsOnUnsupportedComparison(ComparisonType cmp)
    {
        var criteria = new FilterCriteria(typeof(string), nameof(TestEntity.Name), "abc", cmp);
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<TestEntity>(criteria));
    }

    [Theory]
    [InlineData(ComparisonType.EqualTo, 30, true, false)]
    [InlineData(ComparisonType.NotEqualTo, 30, false, true)]
    [InlineData(ComparisonType.GreaterThan, 20, true, false)]
    [InlineData(ComparisonType.GreaterThanOrEqualTo, 30, true, false)]
    [InlineData(ComparisonType.LessThan, 40, true, true)]
    [InlineData(ComparisonType.LessThanOrEqualTo, 30, true, true)]
    public void CreateWhereExpression_Int_AllComparisons(ComparisonType cmp, int value, bool match, bool negMatch)
    {
        // Arrange
        var criteria = new FilterCriteria(typeof(int), nameof(TestEntity.Age), value, cmp);
        var expr = ExpressionBuilder.CreateWhereExpression<TestEntity>(criteria);

        // Act & Assert
        var compiled = expr.Compile();

        Assert.Equal(match, compiled(new TestEntity { Age = 30 }));
        Assert.Equal(negMatch, compiled(new TestEntity { Age = 10 }));
    }

    [Fact]
    public void CreateWhereExpression_NullFilter_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ExpressionBuilder.CreateWhereExpression<TestEntity>(null!));
    }

    [Fact]
    public void CreateWhereExpression_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<TestEntity>(new FilterCriteria(typeof(int), "", 1)));
    }

    [Fact]
    public void CreateWhereExpression_NonExistentProperty_Throws()
    {
        var criteria = new FilterCriteria(typeof(int), "NotAProp", 1);
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<TestEntity>(criteria));
    }

    [Fact]
    public void CreateWhereExpression_NavigationProperty_String_EqualTo()
    {
        // Arrange
        var criteria = new FilterCriteria(typeof(string), "Location.Name", "new york", ComparisonType.EqualTo);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<TestEntityWithNavigation>(criteria);
        var compiled = expr.Compile();

        // Assert
        var entity1 = new TestEntityWithNavigation { Location = new TestLocation { Name = "New York" } };
        var entity2 = new TestEntityWithNavigation { Location = new TestLocation { Name = "Los Angeles" } };

        Assert.True(compiled(entity1));
        Assert.False(compiled(entity2));
    }

    [Fact]
    public void CreateWhereExpression_NavigationProperty_String_Contains()
    {
        // Arrange
        var criteria = new FilterCriteria(typeof(string), "Location.Name", "york", ComparisonType.Contains);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<TestEntityWithNavigation>(criteria);
        var compiled = expr.Compile();

        // Assert
        var entity1 = new TestEntityWithNavigation { Location = new TestLocation { Name = "New York" } };
        var entity2 = new TestEntityWithNavigation { Location = new TestLocation { Name = "Los Angeles" } };

        Assert.True(compiled(entity1));
        Assert.False(compiled(entity2));
    }

    [Fact]
    public void CreateWhereExpression_DeeplyNestedNavigationProperty_EqualTo()
    {
        // Arrange
        var criteria = new FilterCriteria(typeof(string), "Location.Country.Code", "us", ComparisonType.EqualTo);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<TestEntityWithNavigation>(criteria);
        var compiled = expr.Compile();

        // Assert
        var entity1 = new TestEntityWithNavigation
        {
            Location = new TestLocation { Country = new TestCountry { Code = "US" } }
        };
        var entity2 = new TestEntityWithNavigation
        {
            Location = new TestLocation { Country = new TestCountry { Code = "UK" } }
        };

        Assert.True(compiled(entity1));
        Assert.False(compiled(entity2));
    }

    [Fact]
    public void CreateWhereExpression_NavigationProperty_Int_GreaterThan()
    {
        // Arrange
        var criteria = new FilterCriteria(typeof(int), "Location.Id", 10, ComparisonType.GreaterThan);

        // Act
        var expr = ExpressionBuilder.CreateWhereExpression<TestEntityWithNavigation>(criteria);
        var compiled = expr.Compile();

        // Assert
        var entity1 = new TestEntityWithNavigation { Location = new TestLocation { Id = 20 } };
        var entity2 = new TestEntityWithNavigation { Location = new TestLocation { Id = 5 } };

        Assert.True(compiled(entity1));
        Assert.False(compiled(entity2));
    }

    [Fact]
    public void CreateWhereExpression_InvalidNavigationPath_Throws()
    {
        // Arrange
        var criteria = new FilterCriteria(typeof(string), "Location.InvalidProperty", "test", ComparisonType.EqualTo);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<TestEntityWithNavigation>(criteria));
    }

    #endregion

    #region CreateWhereExpression(Expression<Func<T, object>>, object, ComparisonType) Tests

    [Fact]
    public void CreateWhereExpression_PropertySelector_String_EqualTo()
    {
        Expression<Func<TestEntity, object>> prop = x => x.Name;
        var expr = ExpressionBuilder.CreateWhereExpression(prop, "john", ComparisonType.EqualTo);
        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Name = "JOHN" }));
        Assert.False(compiled(new TestEntity { Name = "jane" }));
    }

    [Fact]
    public void CreateWhereExpression_PropertySelector_Int_GreaterThan()
    {
        Expression<Func<TestEntity, object>> prop = x => x.Age;
        var expr = ExpressionBuilder.CreateWhereExpression(prop, 20, ComparisonType.GreaterThan);
        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 30 }));
        Assert.False(compiled(new TestEntity { Age = 10 }));
    }

    [Fact]
    public void CreateWhereExpression_PropertySelector_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ExpressionBuilder.CreateWhereExpression<TestEntity>(null!, 1, ComparisonType.EqualTo));
    }

    [Fact]
    public void CreateWhereExpression_PropertySelector_InvalidSelector_Throws()
    {
        Expression<Func<TestEntity, object>> prop = x => x;
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression(prop, 1, ComparisonType.EqualTo));
    }

    #endregion

    #region GetPropertyExpression Tests

    [Fact]
    public void GetPropertyExpression_ValidProperty_ReturnsExpression()
    {
        var expr = ExpressionBuilder.GetPropertyExpression<TestEntity>(nameof(TestEntity.Name));
        Assert.NotNull(expr);
        var compiled = expr!.Compile();
        var entity = new TestEntity { Name = "abc" };
        Assert.Equal("abc", compiled(entity));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetPropertyExpression_NullOrWhitespace_ReturnsNull(string? propName)
    {
        var expr = ExpressionBuilder.GetPropertyExpression<TestEntity>(propName!);
        Assert.Null(expr);
    }

    [Fact]
    public void GetPropertyExpression_PrimitiveType_ReturnsNull()
    {
        var expr = ExpressionBuilder.GetPropertyExpression<int>("Value");
        Assert.Null(expr);
    }

    [Fact]
    public void GetPropertyExpression_NonExistentProperty_ReturnsNull()
    {
        var expr = ExpressionBuilder.GetPropertyExpression<TestEntity>("NotAProp");
        Assert.Null(expr);
    }

    [Fact]
    public void GetPropertyExpression_NavigationProperty_ReturnsExpression()
    {
        // Arrange
        var entity = new TestEntityWithNavigation
        {
            Id = 1,
            Location = new TestLocation { Id = 10, Name = "New York" }
        };

        // Act
        var expr = ExpressionBuilder.GetPropertyExpression<TestEntityWithNavigation>("Location.Name");

        // Assert
        Assert.NotNull(expr);
        var compiled = expr!.Compile();
        Assert.Equal("New York", compiled(entity));
    }

    [Fact]
    public void GetPropertyExpression_DeeplyNestedNavigationProperty_ReturnsExpression()
    {
        // Arrange
        var entity = new TestEntityWithNavigation
        {
            Id = 1,
            Location = new TestLocation
            {
                Id = 10,
                Name = "New York",
                Country = new TestCountry { Id = 100, Code = "US", Name = "United States" }
            }
        };

        // Act
        var expr = ExpressionBuilder.GetPropertyExpression<TestEntityWithNavigation>("Location.Country.Code");

        // Assert
        Assert.NotNull(expr);
        var compiled = expr!.Compile();
        Assert.Equal("US", compiled(entity));
    }

    [Fact]
    public void GetPropertyExpression_InvalidNavigationPath_ReturnsNull()
    {
        // Act
        var expr = ExpressionBuilder.GetPropertyExpression<TestEntityWithNavigation>("Location.InvalidProperty");

        // Assert
        Assert.Null(expr);
    }

    [Fact]
    public void GetPropertyExpression_NavigationPropertyValueType_ReturnsExpression()
    {
        // Arrange
        var entity = new TestEntityWithNavigation
        {
            Id = 1,
            Location = new TestLocation { Id = 42, Name = "Test" }
        };

        // Act
        var expr = ExpressionBuilder.GetPropertyExpression<TestEntityWithNavigation>("Location.Id");

        // Assert
        Assert.NotNull(expr);
        var compiled = expr!.Compile();
        Assert.Equal(42, compiled(entity));
    }

    #endregion

    #region Test Classes for Navigation Properties

    private class TestEntityWithNavigation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TestLocation Location { get; set; } = new();
    }

    private class TestLocation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TestCountry Country { get; set; } = new();
    }

    private class TestCountry
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    #endregion
}
