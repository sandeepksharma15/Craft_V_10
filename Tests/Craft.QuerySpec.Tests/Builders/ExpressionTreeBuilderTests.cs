using System.Linq.Expressions;
using Xunit;
using Xunit.Sdk;

namespace Craft.QuerySpec.Tests.Builders;

public class ExpressionTreeBuilderTests
{
    [Fact]
    public void AllRegExPatterns()
    {
        Assert.Equal(@"^\s*(?'leftOperand'[^\|\&]*)\s*(?'evaluator_first'((\|)*|(\&)*))\s*(?'brackets'(\(\s*(.*)s*\)))\s*(?'evaluator_second'((\|{1,2})|(\&{1,2}))*)\s*(?'rightOperand'.*)\s*$", ExpressionBuilderForTest.HasBracketValue);
        Assert.Equal(@"^\s*\(\s*(?'leftOperand'([^\(\)])+)\s*\)\s*$", ExpressionBuilderForTest.HasSurroundingBracketsOnlyValue);
        Assert.Equal(@"^(?'leftOperand'\S{1,}\s*(==|!=|<|<=|>|>=)\s*\S{1,})\s*(?'evaluator_first'((\|{1,2})|(\&{1,2})))\s*(?'rightOperand'.*)\s*$", ExpressionBuilderForTest.EvalPatternValue);
        Assert.Equal(@"^\s*(?'leftOperand'\w+)\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+)\s*$", ExpressionBuilderForTest.BinaryPatternValue);
        Assert.Equal(@"^\s*(\""\s*(?'leftOperand'.*)\s*\""\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+)|(?'leftOperand'\w+)\s*(?'operator'(==|!=|<|<=|>|>=))\s*\""\s*(?'rightOperand'.*)\s*\""\s*)\s*$", ExpressionBuilderForTest.EscapedBinaryPatternValue);
        Assert.Equal(@"^\s*\(\s*(?'leftOperand'\w+)\s*(?'operator'(==|!=|<|<=|>|>=))\s*(?'rightOperand'\w+)\s*\)\s*$", ExpressionBuilderForTest.BinaryWithBracketsPatternValue);
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
        Assert.NotNull(ExpressionBuilderForTest.GetBinaryExpressionBuilder(key));
    }

    [Theory]
    [InlineData("&")]
    [InlineData("&&")]
    [InlineData("|")]
    [InlineData("||")]
    public void EvaluationExpressionBuilder_Keys_returnBuilder(string key)
    {
        Assert.NotNull(ExpressionBuilderForTest.GetEvaluationExpressionBuilder(key));
    }

    [Fact]
    public void ToBinaryTree_BuildsExpression()
    {
        var col = new[] {
            new TestClass{Id = "1", NumericValue  =1, StringValue = "1"},
            new TestClass{Id = "2", NumericValue  =1, StringValue = "2"},
            new TestClass{Id = "3", NumericValue  =3},
        };

        var filter1 = new Dictionary<string, string> { { nameof(TestClass.NumericValue), "1" } };
        var f1 = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(filter1);
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
        var e = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(query);
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
    //[InlineData("id == 2 (value1 == 32 && value2 < 123)")]
    [InlineData("\"this-is-id\" == id")]
    public void ToBinaryTree_FromString_ReturnsNull(string query)
    {
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(query));
    }

    [Fact]
    public void ToBinaryTree_PropertyNotExists()
    {
        var filter = new Dictionary<string, string> { { "p", "d" } };
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(filter));
    }

    [Fact]
    public void BuildBinaryTreeExpression_EmptyQuery_ReturnsNull()
    {
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(""));
    }

    [Fact]
    public void BuildBinaryTreeExpression_WhitespaceQuery_ReturnsNull()
    {
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("   "));
    }

    [Fact]
    public void BuildBinaryTreeExpression_OnlyBrackets_ReturnsNull()
    {
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("()"));
    }

    [Fact]
    public void BuildBinaryTreeExpression_InvalidPropertyName_ReturnsNull()
    {
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("notAProp == 1"));
    }

    [Fact]
    public void BuildBinaryTreeExpression_InvalidOperator_ReturnsNull()
    {
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("Id %% 1"));
    }

    [Fact]
    public void BuildBinaryTreeExpression_InvalidValueType_ReturnsNull()
    {
        Assert.Null(ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>("NumericValue == notAnInt"));
    }

    [Fact]
    public void ToExpression_NullFilter_ReturnsNull()
    {
        Assert.Null(ExpressionTreeBuilder.ToExpression<TestClass>(null!));
    }

    [Fact]
    public void ToExpression_EmptyFilter_ReturnsNull()
    {
        Assert.Null(ExpressionTreeBuilder.ToExpression<TestClass>(new Dictionary<string, string>()));
    }

    [Fact]
    public void ToExpression_InvalidPropertyInFilter_ReturnsNull()
    {
        var filter = new Dictionary<string, string> { { "notAProp", "1" } };
        Assert.Null(ExpressionTreeBuilder.ToExpression<TestClass>(filter));
    }

    [Fact]
    public void ToExpression_InvalidValueTypeInFilter_ReturnsNull()
    {
        var filter = new Dictionary<string, string> { { "NumericValue", "notAnInt" } };
        Assert.Null(ExpressionTreeBuilder.ToExpression<TestClass>(filter));
    }

    [Fact]
    public void BuildBinaryTreeExpression_NestedBracketsAndWhitespace_Works()
    {
        var expr = ExpressionTreeBuilder.BuildBinaryTreeExpression<TestClass>(" ( ( NumericValue == 1 ) ) ");
        Assert.NotNull(expr);
    }
}

public class TestClass
{
    public IEnumerable<string> Collection { get; set; } = [];
    public string Id { get; set; } = string.Empty;
    public int NumericValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
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

public class ExpressionBuilderForTest : ExpressionTreeBuilder
{
    internal const string BinaryPatternValue = BinaryPattern;
    internal const string BinaryWithBracketsPatternValue = BinaryWithBracketsPattern;
    internal const string EscapedBinaryPatternValue = EscapedBinaryPattern;
    internal const string EvalPatternValue = EvalPattern;
    internal const string HasBracketValue = HasBrackets;
    internal const string HasSurroundingBracketsOnlyValue = HasSurroundingBracketsOnly;
    internal static Func<MemberExpression, object, Expression> GetBinaryExpressionBuilder(string key) => BinaryExpressionBuilder[key];
    internal static Func<Expression, Expression, Expression> GetEvaluationExpressionBuilder(string key) => EvaluationExpressionBuilder[key];
}
