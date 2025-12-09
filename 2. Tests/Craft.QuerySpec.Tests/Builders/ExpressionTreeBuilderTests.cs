using System.Linq.Expressions;

namespace Craft.QuerySpec.Tests.Builders;

public class ExpressionTreeBuilderTests
{
    [Fact]
    public void AllRegExPatterns()
    {
        // Assert
        Assert.Equal(@"^\s*(?'leftOperand'[^\|\&]*)\s*(?'evaluator_first'((\|)*|(\&)*))\s*(?'brackets'(\(\s*(.*)s*\)))\s*(?'evaluator_second'((\|{1,2})|(\&{1,2}))*)\s*(?'rightOperand'.*)\s*$", ExpressionTreeBuilder.TestPatterns.HasBracketsValue);
        Assert.Equal(@"^\s*\(\s*(?'leftOperand'([^\(\)])+)\s*\)\s*$", ExpressionTreeBuilder.TestPatterns.HasSurroundingBracketsOnlyValue);
        Assert.Equal(@"^(?'leftOperand'\S{1,}\s*(==|!=|<|<=|>|>=)\s*\S{1,})\s*(?'evaluator_first'((\|{1,2})|(\&{1,2})))\s*(?'rightOperand'.*)\s*$", ExpressionTreeBuilder.TestPatterns.EvalPatternValue);
        Assert.Equal(@"^\s*(?'leftOperand'\w+)\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+)\s*$", ExpressionTreeBuilder.TestPatterns.BinaryPatternValue);
        Assert.Equal(@"^\s*(\""\s*(?'leftOperand'.*)\s*\""\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+)|(?'leftOperand'\w+)\s*(?'operator'(==|!=|<|<=|>|>=))\s*\""\s*(?'rightOperand'.*)\s*\""\s*)\s*$", ExpressionTreeBuilder.TestPatterns.EscapedBinaryPatternValue);
        Assert.Equal(@"^\s*\(\s*(?'leftOperand'\w+)\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+)\s*\)\s*$", ExpressionTreeBuilder.TestPatterns.BinaryWithBracketsPatternValue);
    }

    [Theory]
    [InlineData("==")]
    [InlineData("!=")]
    [InlineData(">")]
    [InlineData(">=")]
    [InlineData("<")]
    [InlineData("<=")]
    public void BinaryExpressionBuilder_Keys_returnBuilder(string key)
    {
        // Assert
        Assert.True(ExpressionTreeBuilder.TestBuilders.BinaryExpressionBuilders.ContainsKey(key));
        Assert.NotNull(ExpressionTreeBuilder.TestBuilders.BinaryExpressionBuilders[key]);
    }

    [Theory]
    [InlineData("&")]
    [InlineData("&&")]
    [InlineData("|")]
    [InlineData("||")]
    public void EvaluationExpressionBuilder_Keys_returnBuilder(string key)
    {
        // Assert
        Assert.True(ExpressionTreeBuilder.TestBuilders.EvaluationExpressionBuilders.ContainsKey(key));
        Assert.NotNull(ExpressionTreeBuilder.TestBuilders.EvaluationExpressionBuilders[key]);
    }

    [Fact]
    public void ToBinaryTree_BuildsExpression()
    {
        // Arrange
        var col = new[] {
            new TestClass{Id = "1", NumericValue  =1, StringValue = "1"},
            new TestClass{Id = "2", NumericValue  =1, StringValue = "2"},
            new TestClass{Id = "3", NumericValue  =3},
        };

        var filter1 = new Dictionary<string, string> { { nameof(TestClass.NumericValue), "1" } };

        // Act
        var f1 = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(filter1);

        // Assert
        Assert.NotNull(f1);

        var res1 = col.Where(f1).ToArray();

        Assert.Equal(2, res1.Length);

        var filter2 = new Dictionary<string, string> {
            {nameof(TestClass.StringValue), "1" } ,
            {nameof(TestClass.NumericValue), "1" } ,
        };

        var f2 = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(filter2);
        Assert.NotNull(f2);

        var res2 = col.Where(f2).ToArray();
        Assert.Single(res2);
    }

    [Theory]
    [ClassData(typeof(ToBinaryTree_EmptyOrNullOrIncorrectFilter_ReturnsNull_Data))]
    public void ToBinaryTree_EmptyOrNullFilter_ReturnsNull(IDictionary<string, string> filter)
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(filter));
    }

    [Theory]
    [InlineData("id == \"this-is-id\"", "x => (x.Id == \"this-is-id\")")]
    [InlineData("id == 2", "x => (x.Id == \"2\")")]
    [InlineData("(id == 2)", "x => (x.Id == \"2\")")]
    [InlineData("NumericValue > 2", "x => (x.NumericValue > 2)")]
    [InlineData("NumericValue >= 3", "x => (x.NumericValue >= 3)")]
    [InlineData("NumericValue < 3", "x => (x.NumericValue < 3)")]
    [InlineData("NumericValue <= 3", "x => (x.NumericValue <= 3)")]
    [InlineData("id == 2 & numericValue ==32", "x => ((x.Id == \"2\") And (x.NumericValue == 32))")]
    [InlineData("id == 2 && numericValue ==32", "x => ((x.Id == \"2\") AndAlso (x.NumericValue == 32))")]
    [InlineData("id == 2 && numericValue ==32 & stringValue==a", "x => ((x.Id == \"2\") AndAlso ((x.NumericValue == 32) And (x.StringValue == \"a\")))")]
    [InlineData("id == 2 & numericValue ==32 & stringValue==a", "x => ((x.Id == \"2\") And ((x.NumericValue == 32) And (x.StringValue == \"a\")))")]
    [InlineData("id == 2 && numericValue ==32 && stringValue==a", "x => ((x.Id == \"2\") AndAlso ((x.NumericValue == 32) AndAlso (x.StringValue == \"a\")))")]
    [InlineData("id == 2 | numericValue ==32", "x => ((x.Id == \"2\") Or (x.NumericValue == 32))")]
    [InlineData("id == 2 || numericValue ==32", "x => ((x.Id == \"2\") OrElse (x.NumericValue == 32))")]
    [InlineData("id == 2 || numericValue ==32 || stringValue==a", "x => ((x.Id == \"2\") OrElse ((x.NumericValue == 32) OrElse (x.StringValue == \"a\")))")]
    [InlineData("id == 2 || numericValue ==32 |  stringValue==a", "x => ((x.Id == \"2\") OrElse ((x.NumericValue == 32) Or (x.StringValue == \"a\")))")]
    [InlineData("(id == 2 || numericvalue <3 && stringValue ==a)", "x => ((x.Id == \"2\") OrElse ((x.NumericValue < 3) AndAlso (x.StringValue == \"a\")))")]
    [InlineData("(id == 2 || numericvalue <3) && stringValue ==a", "x => (((x.Id == \"2\") OrElse (x.NumericValue < 3)) AndAlso (x.StringValue == \"a\"))")]
    [InlineData("id == 2 || (numericvalue ==32 && stringValue ==a)", "x => ((x.Id == \"2\") OrElse ((x.NumericValue == 32) AndAlso (x.StringValue == \"a\")))")]
    [InlineData("id == 2 || (numericvalue ==32 && stringValue ==a) || stringValue ==b", "x => ((x.Id == \"2\") OrElse (((x.NumericValue == 32) AndAlso (x.StringValue == \"a\")) OrElse (x.StringValue == \"b\")))")]
    public void ToBinaryTree_FromString(string query, string expResult)
    {
        // Arrange
        var e = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(query);

        // Assert
        Assert.Equal(expResult, e?.ToString());
    }

    [Theory]
    [InlineData("idsfdsadffffdsfsdf2")]
    [InlineData("id 2")]
    [InlineData("id == == 2")]
    [InlineData("id == != 2")]
    [InlineData("id == <= 2")]
    [InlineData("id == < 2")]
    [InlineData("id == >= 2")]
    [InlineData("id == > 2")]
    [InlineData("id != > 2")]
    [InlineData("id == 2  value1 == 32")]
    [InlineData("(id == 2 || value1 == 32) value2 < 123")]
    [InlineData("(id == 2  value1 ==32)")]
    [InlineData("(id == 2 || value1 == 32) value2 < 123 || claue3 = 9898")]
    [InlineData("\"this-is-id\" == id")]
    public void ToBinaryTree_FromString_ReturnsNull(string query)
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(query));
    }

    [Fact]
    public void ToBinaryTree_PropertyNotExists()
    {
        // Arrange
        var filter = new Dictionary<string, string> { { "p", "d" } };

        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(filter));
    }

    [Fact]
    public void BuildBinaryTreeExpression_EmptyQuery_ReturnsNull()
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(""));
    }

    [Fact]
    public void BuildBinaryTreeExpression_WhitespaceQuery_ReturnsNull()
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("   "));
    }

    [Fact]
    public void BuildBinaryTreeExpression_OnlyBrackets_ReturnsNull()
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("()"));
    }

    [Fact]
    public void BuildBinaryTreeExpression_InvalidPropertyName_ReturnsNull()
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("notAProp == 1"));
    }

    [Fact]
    public void BuildBinaryTreeExpression_InvalidOperator_ReturnsNull()
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("Id %% 1"));
    }

    [Fact]
    public void BuildBinaryTreeExpression_InvalidValueType_ReturnsNull()
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("NumericValue == notAnInt"));
    }

    [Fact]
    public void ToExpression_NullFilter_ReturnsNull()
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.ToExpression<TestClass>(null!));
    }

    [Fact]
    public void ToExpression_EmptyFilter_ReturnsNull()
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.ToExpression<TestClass>(new Dictionary<string, string>()));
    }

    [Fact]
    public void ToExpression_InvalidPropertyInFilter_ReturnsNull()
    {
        // Arrange
        var filter = new Dictionary<string, string> { { "notAProp", "1" } };

        // Assert
        Assert.Null(ExpressionTreeBuilder.ToExpression<TestClass>(filter));
    }

    [Fact]
    public void ToExpression_InvalidValueTypeInFilter_ReturnsNull()
    {
        // Arrange
        var filter = new Dictionary<string, string> { { "NumericValue", "notAnInt" } };

        // Assert
        Assert.Null(ExpressionTreeBuilder.ToExpression<TestClass>(filter));
    }

    [Fact]
    public void BuildBinaryTreeExpression_NestedBracketsAndWhitespace_Works()
    {
        // Arrange
        var expr = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(" ( ( NumericValue == 1 ) ) ");

        // Assert
        Assert.NotNull(expr);
    }

    [Fact]
    public void BuildBinaryTreeExpression_MultipleNestedBrackets_ReturnsNull()
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("((()))"));
    }

    #region Missing Test Cases for Complete Coverage

    /// <summary>
    /// Tests the overload that takes a Type parameter with null type argument
    /// </summary>
    [Fact]
    public void BuildBinaryTreeExpression_NullType_ThrowsArgumentNullException()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() =>
            ExpressionTreeBuilder.BuildBinaryTreeExpression(null!, "Id == 1"));
    }

    /// <summary>
    /// Tests the 5-parameter overload with null arguments
    /// </summary>
    [Fact]
    public void BuildBinaryTreeExpression_FiveParam_NullArguments_ThrowsArgumentNullException()
    {
        // Arrange
        var parameterExpression = Expression.Parameter(typeof(TestClass), "x");

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
            ExpressionTreeBuilder.BuildBinaryTreeExpression(null!, "Id", "==", "1", parameterExpression));
        Assert.Throws<ArgumentNullException>(() =>
            ExpressionTreeBuilder.BuildBinaryTreeExpression(typeof(TestClass), null!, "==", "1", parameterExpression));
        Assert.Throws<ArgumentNullException>(() =>
            ExpressionTreeBuilder.BuildBinaryTreeExpression(typeof(TestClass), "Id", null!, "1", parameterExpression));
        Assert.Throws<ArgumentNullException>(() =>
            ExpressionTreeBuilder.BuildBinaryTreeExpression(typeof(TestClass), "Id", "==", null!, parameterExpression));
        Assert.Throws<ArgumentNullException>(() =>
            ExpressionTreeBuilder.BuildBinaryTreeExpression(typeof(TestClass), "Id", "==", "1", null!));
    }

    /// <summary>
    /// Tests the 5-parameter overload with empty string arguments
    /// </summary>
    [Fact]
    public void BuildBinaryTreeExpression_FiveParam_EmptyStringArguments_ReturnsNull()
    {
        // Arrange
        var parameterExpression = Expression.Parameter(typeof(TestClass), "x");

        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression(typeof(TestClass), "", "==", "1", parameterExpression));
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression(typeof(TestClass), "Id", "", "1", parameterExpression));
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression(typeof(TestClass), "Id", "==", "", parameterExpression));
    }

    /// <summary>
    /// Tests nested property access - fails due to regex pattern limitations
    /// </summary>
    [Fact]
    public void BuildBinaryTreeExpression_NestedProperty_FailsDueToRegexLimitation()
    {
        // Arrange
        var query = "Nested.Value == \"test\"";

        // Act
        var expr = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClassWithNested>(query);

        // Assert
        // This will fail because the BinaryPattern uses \w+ which doesn't match dots (.)
        // Even though GetPropertyByName supports nested properties, the regex parsing doesn't
        // This is a known limitation of the current regex patterns
        Assert.Null(expr);
    }

    /// <summary>
    /// Tests that nested property access fails with dictionary-based method due to Expression.PropertyOrField limitation
    /// </summary>
    [Fact]
    public void BuildBinaryTreeExpression_NestedProperty_FailsWithDictionary_DueToExpressionLimitation()
    {
        // Arrange
        var filter = new Dictionary<string, string> { { "Nested.Value", "test" } };

        // Act
        var predicate = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClassWithNested>(filter);

        // Assert
        // This fails because while GetPropertyByName finds the nested property descriptor,
        // Expression.PropertyOrField(parameterExpression, "Nested.Value") doesn't work
        // Expression.PropertyOrField expects simple property names, not dotted paths
        Assert.Null(predicate);
    }

    /// <summary>
    /// Tests that ToExpression method also fails with nested properties due to same limitation
    /// </summary>
    [Fact]
    public void ToExpression_NestedProperty_FailsDueToExpressionLimitation()
    {
        // Arrange
        var filter = new Dictionary<string, string> { { "Nested.Value", "test" } };

        // Act
        var expr = ExpressionTreeBuilder.ToExpression<TestClassWithNested>(filter);

        // Assert
        // This also fails due to Expression.PropertyOrField not supporting dotted notation
        Assert.Null(expr);
    }

    /// <summary>
    /// Tests what actually works with nested properties - manual expression building
    /// </summary>
    [Fact]
    public void ManualNestedPropertyExpression_Works_WithSimpleProperties()
    {
        // Arrange - use simple properties that don't require dotted notation
        var filter = new Dictionary<string, string> { { "Id", "test" } };

        // Act
        var predicate = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClassWithNested>(filter);

        // Assert
        Assert.NotNull(predicate);

        // Test with actual data
        var testData = new TestClassWithNested
        {
            Id = "test",
            Nested = new NestedClass { Value = "test" }
        };

        Assert.True(predicate(testData));

        var nonMatchingData = new TestClassWithNested
        {
            Id = "different",
            Nested = new NestedClass { Value = "test" }
        };

        Assert.False(predicate(nonMatchingData));
    }

    /// <summary>
    /// Demonstrates how nested property expressions would work if manually constructed
    /// </summary>
    [Fact]
    public void ManualNestedPropertyExpression_ShowsHowItShouldWork()
    {
        // Arrange - manually create the nested property expression
        var parameter = Expression.Parameter(typeof(TestClassWithNested), "x");
        var nestedProperty = Expression.Property(parameter, "Nested");
        var valueProperty = Expression.Property(nestedProperty, "Value");
        var constantValue = Expression.Constant("test");
        var equalExpression = Expression.Equal(valueProperty, constantValue);
        var lambda = Expression.Lambda<Func<TestClassWithNested, bool>>(equalExpression, parameter);

        // Act
        var compiled = lambda.Compile();

        // Assert
        Assert.NotNull(compiled);

        // Test with actual data
        var testData = new TestClassWithNested
        {
            Id = "test",
            Nested = new NestedClass { Value = "test" }
        };

        Assert.True(compiled(testData));

        var nonMatchingData = new TestClassWithNested
        {
            Id = "test",
            Nested = new NestedClass { Value = "different" }
        };

        Assert.False(compiled(nonMatchingData));
    }

    #endregion

    [Fact]
    public void BuildBinaryTreeExpressionWorker_SurroundingBrackets_InnerLogic_Coverage()
    {
        // Arrange: Query with a single set of surrounding brackets and valid inner expression
        var expr = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("(NumericValue == 1)");
        Assert.NotNull(expr); // Should parse and not return null

        // Arrange: Query with only brackets and whitespace (should return null)
        var exprNull = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("(   )");
        Assert.Null(exprNull);

        // Arrange: Query where inner == original after trimming (should return null)
        // This is a crafted edge case: e.g., single character in brackets
        var exprEdge = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("(x)");
        Assert.Null(exprEdge);
    }
}

public class TestClass
{
    public IEnumerable<string> Collection { get; set; } = [];
    public string Id { get; set; } = string.Empty;
    public int NumericValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
}

/// <summary>
/// Test class with nested properties for testing nested property access
/// </summary>
public class TestClassWithNested
{
    public string Id { get; set; } = string.Empty;
    public NestedClass Nested { get; set; } = new();
}

public class NestedClass
{
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Test class with nullable properties
/// </summary>
public class TestClassWithNullables
{
    public string Id { get; set; } = string.Empty;
    public int? NullableInt { get; set; }
    public DateTime? NullableDateTime { get; set; }
    public bool? NullableBool { get; set; }
}

/// <summary>
/// Test class with various data types
/// </summary>
public class TestClassWithMoreTypes
{
    public string Id { get; set; } = string.Empty;
    public bool BoolValue { get; set; }
    public decimal DecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public Guid GuidValue { get; set; }
    public long LongValue { get; set; }
    public short ShortValue { get; set; }
    public byte ByteValue { get; set; }
}

public class ToBinaryTree_EmptyOrNullOrIncorrectFilter_ReturnsNull_Data : TheoryData<IDictionary<string, string>>
{
    public ToBinaryTree_EmptyOrNullOrIncorrectFilter_ReturnsNull_Data()
    {
        Add(null!);
        Add(new Dictionary<string, string>());
        Add(new Dictionary<string, string> { { nameof(TestClass.NumericValue), "d" } });
    }
}
