namespace Craft.Expressions.Tests;

public class OperatorConstantsTests
{
    [Fact]
    public void ExpressionOperators_DefinesAllLogicalOperators()
    {
        // Assert
        Assert.Equal("&&", ExpressionOperators.And);
        Assert.Equal("||", ExpressionOperators.Or);
        Assert.Equal("!", ExpressionOperators.Not);
    }

    [Fact]
    public void ExpressionOperators_DefinesAllComparisonOperators()
    {
        // Assert
        Assert.Equal("==", ExpressionOperators.Equal);
        Assert.Equal("!=", ExpressionOperators.NotEqual);
        Assert.Equal(">", ExpressionOperators.GreaterThan);
        Assert.Equal(">=", ExpressionOperators.GreaterThanOrEqual);
        Assert.Equal("<", ExpressionOperators.LessThan);
        Assert.Equal("<=", ExpressionOperators.LessThanOrEqual);
    }

    [Fact]
    public void ExpressionOperators_DefinesPunctuationConstants()
    {
        // Assert
        Assert.Equal(".", ExpressionOperators.Dot);
        Assert.Equal(",", ExpressionOperators.Comma);
        Assert.Equal("(", ExpressionOperators.OpenParen);
        Assert.Equal(")", ExpressionOperators.CloseParen);
    }

    [Fact]
    public void Parser_UsesOperatorConstants()
    {
        // Arrange
        var serializer = new ExpressionSerializer<TestClass>();

        // Act - Should work with all operators
        var andExpression = serializer.Deserialize("Age > 18 && IsActive == true");
        var orExpression = serializer.Deserialize("Age < 18 || Age > 65");
        var notExpression = serializer.Deserialize("!(Age >= 18)");

        // Assert
        Assert.NotNull(andExpression);
        Assert.NotNull(orExpression);
        Assert.NotNull(notExpression);
    }

    private class TestClass
    {
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }
}
