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
    /// Tests nested property access - should now work with enhanced regex patterns
    /// </summary>
    [Fact]
    public void BuildBinaryTreeExpression_NestedProperty_WorksWithEnhancedPatterns()
    {
        // Arrange
        var query = "Nested.Value == \"test\"";
        
        // Act
        var expr = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClassWithNested>(query);
        
        // Assert
        Assert.NotNull(expr);
        
        // Test with actual data
        var testData = new TestClassWithNested 
        { 
            Id = "test", 
            Nested = new NestedClass { Value = "test" } 
        };
        
        var compiled = expr.Compile();
        Assert.True(compiled(testData));
        
        var nonMatchingData = new TestClassWithNested 
        { 
            Id = "test", 
            Nested = new NestedClass { Value = "different" } 
        };
        
        Assert.False(compiled(nonMatchingData));
    }

    /// <summary>
    /// Tests that nested property access now works with dictionary-based method after fixing expression building
    /// </summary>
    [Fact]
    public void BuildBinaryTreeExpression_NestedProperty_WorksWithDictionary_AfterFix()
    {
        // Arrange
        var filter = new Dictionary<string, string> { { "Nested.Value", "test" } };
        
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
            Id = "test", 
            Nested = new NestedClass { Value = "different" } 
        };
        
        Assert.False(predicate(nonMatchingData));
    }

    /// <summary>
    /// Tests that ToExpression method now works with nested properties after fixing expression building
    /// </summary>
    [Fact]
    public void ToExpression_NestedProperty_WorksAfterExpressionFix()
    {
        // Arrange
        var filter = new Dictionary<string, string> { { "Nested.Value", "test" } };
        
        // Act
        var expr = ExpressionTreeBuilder.ToExpression<TestClassWithNested>(filter);
        
        // Assert
        Assert.NotNull(expr);
        
        // Test with actual data
        var testData = new TestClassWithNested 
        { 
            Id = "test", 
            Nested = new NestedClass { Value = "test" } 
        };
        
        var compiled = expr.Compile();
        Assert.True(compiled(testData));
        
        var nonMatchingData = new TestClassWithNested 
        { 
            Id = "test", 
            Nested = new NestedClass { Value = "different" } 
        };
        
        Assert.False(compiled(nonMatchingData));
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

    #region New Enhanced Features Tests

    /// <summary>
    /// Tests new arithmetic operators
    /// </summary>
    [Theory]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("*")]
    [InlineData("/")]
    [InlineData("%")]
    public void ArithmeticExpressionBuilder_Keys_ReturnBuilder(string key)
    {
        // Assert
        Assert.True(ExpressionTreeBuilder.TestBuilders.ArithmeticExpressionBuilders.ContainsKey(key));
        Assert.NotNull(ExpressionTreeBuilder.TestBuilders.ArithmeticExpressionBuilders[key]);
    }

    /// <summary>
    /// Tests new string method builders
    /// </summary>
    [Theory]
    [InlineData("Contains")]
    [InlineData("StartsWith")]
    [InlineData("EndsWith")]
    public void StringMethodBuilder_Keys_ReturnBuilder(string key)
    {
        // Assert
        Assert.True(ExpressionTreeBuilder.TestBuilders.StringMethodBuilders.ContainsKey(key));
        Assert.NotNull(ExpressionTreeBuilder.TestBuilders.StringMethodBuilders[key]);
    }

    /// <summary>
    /// Tests arithmetic expressions with properties and constants
    /// </summary>
    [Theory]
    [InlineData("NumericValue + 5", 10, true)]  // 10 + 5 = 15
    [InlineData("NumericValue - 3", 7, true)]   // 10 - 3 = 7
    [InlineData("NumericValue * 2", 20, true)]  // 10 * 2 = 20
    [InlineData("NumericValue / 2", 5, true)]   // 10 / 2 = 5
    [InlineData("NumericValue % 3", 1, true)]   // 10 % 3 = 1
    public void BuildBinaryTreeExpression_ArithmeticOperations_Work(string query, object expectedResult, bool shouldWork)
    {
        // Arrange
        var expr = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(query);
        
        if (!shouldWork)
        {
            Assert.Null(expr);
            return;
        }
        
        // Assert
        Assert.NotNull(expr);
        
        // Note: These are arithmetic expressions, not boolean expressions, so we can't directly test them
        // without extending the test framework. For now, we just verify they compile successfully.
        var compiled = expr.Compile();
        Assert.NotNull(compiled);
    }

    /// <summary>
    /// Tests string method expressions
    /// </summary>
    [Theory]
    [InlineData("StringValue.Contains(\"test\")", "test string", true)]
    [InlineData("StringValue.Contains(\"missing\")", "test string", false)]
    [InlineData("StringValue.StartsWith(\"test\")", "test string", true)]
    [InlineData("StringValue.StartsWith(\"string\")", "test string", false)]
    [InlineData("StringValue.EndsWith(\"string\")", "test string", true)]
    [InlineData("StringValue.EndsWith(\"test\")", "test string", false)]
    public void BuildBinaryTreeExpression_StringMethods_Work(string query, string testValue, bool expectedResult)
    {
        // Arrange
        var expr = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(query);
        
        // Assert
        Assert.NotNull(expr);
        
        var testData = new TestClass { StringValue = testValue };
        var compiled = expr.Compile();
        
        // Cast to Func<TestClass, bool> since string methods return bool
        var boolFunc = compiled as Func<TestClass, bool>;
        Assert.NotNull(boolFunc);
        
        Assert.Equal(expectedResult, boolFunc(testData));
    }

    /// <summary>
    /// Tests complex nested property expressions with multiple levels
    /// </summary>
    [Fact]
    public void BuildBinaryTreeExpression_DeepNestedProperties_Work()
    {
        // Arrange
        var expr = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClassWithDeepNesting>("Level1.Level2.Value == \"deep\"");
        
        // Assert
        Assert.NotNull(expr);
        
        var testData = new TestClassWithDeepNesting
        {
            Level1 = new Level1Class
            {
                Level2 = new Level2Class
                {
                    Value = "deep"
                }
            }
        };
        
        var compiled = expr.Compile();
        Assert.True(compiled(testData));
    }

    /// <summary>
    /// Tests combined expressions with nested properties and logical operators
    /// </summary>
    [Theory]
    [InlineData("Nested.Value == \"test\" && Id == \"123\"", "test", "123", true)]
    [InlineData("Nested.Value == \"test\" || Id == \"456\"", "test", "123", true)]
    [InlineData("Nested.Value == \"wrong\" && Id == \"123\"", "test", "123", false)]
    public void BuildBinaryTreeExpression_NestedPropertiesWithLogicalOperators_Work(string query, string nestedValue, string idValue, bool expectedResult)
    {
        // Arrange
        var expr = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClassWithNested>(query);
        
        // Assert
        Assert.NotNull(expr);
        
        var testData = new TestClassWithNested
        {
            Id = idValue,
            Nested = new NestedClass { Value = nestedValue }
        };
        
        var compiled = expr.Compile();
        Assert.Equal(expectedResult, compiled(testData));
    }

    /// <summary>
    /// Tests pattern recognition for new features
    /// </summary>
    [Fact]
    public void TestPatterns_NewPatterns_AreAccessible()
    {
        // Assert patterns are accessible
        Assert.NotNull(ExpressionTreeBuilder.TestPatterns.ArithmeticPatternValue);
        Assert.NotNull(ExpressionTreeBuilder.TestPatterns.StringMethodPatternValue);
        
        // Verify pattern content
        Assert.Contains("+", ExpressionTreeBuilder.TestPatterns.ArithmeticPatternValue);
        Assert.Contains("Contains", ExpressionTreeBuilder.TestPatterns.StringMethodPatternValue);
    }

    /// <summary>
    /// Tests error handling for malformed expressions
    /// </summary>
    [Theory]
    [InlineData("NumericValue + ")]  // Incomplete arithmetic
    [InlineData("StringValue.Contains()")]  // Missing argument
    [InlineData("StringValue.InvalidMethod(\"test\")")]  // Invalid method
    [InlineData("Nested..Value == \"test\"")]  // Double dots
    public void BuildBinaryTreeExpression_MalformedExpressions_ReturnNull(string query)
    {
        // Assert
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(query));
    }

    #endregion
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

/// <summary>
/// Test class for deep nesting scenarios
/// </summary>
public class TestClassWithDeepNesting
{
    public Level1Class Level1 { get; set; } = new();
}

public class Level1Class
{
    public Level2Class Level2 { get; set; } = new();
}

public class Level2Class
{
    public string Value { get; set; } = string.Empty;
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
