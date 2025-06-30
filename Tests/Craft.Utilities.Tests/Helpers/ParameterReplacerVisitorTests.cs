using System.Linq.Expressions;
using Craft.Utilities.Helpers;

namespace Craft.Utilities.Tests.Helpers;

public class ParameterReplacerVisitorTests
{
    [Fact]
    public void Replace_Replaces_Single_Parameter_In_Simple_Expression()
    {
        // Arrange
        ParameterExpression oldParam = Expression.Parameter(typeof(int), "x");
        ParameterExpression newParam = Expression.Parameter(typeof(int), "y");
        Expression<Func<int, int>> expr = Expression.Lambda<Func<int, int>>(Expression.Add(oldParam, Expression.Constant(2)), oldParam);

        // Act
        var replaced = ParameterReplacerVisitor.Replace(expr.Body, oldParam, newParam);

        // Assert
        var lambda = Expression.Lambda<Func<int, int>>(replaced, newParam);
        var compiled = lambda.Compile();
        Assert.Equal(5, compiled(3));
    }

    [Fact]
    public void Replace_Replaces_Parameter_In_Complex_Expression()
    {
        // Arrange
        ParameterExpression oldParam = Expression.Parameter(typeof(int), "a");
        ParameterExpression newParam = Expression.Parameter(typeof(int), "b");
        Expression<Func<int, int>> expr = Expression.Lambda<Func<int, int>>(
            Expression.Multiply(
                Expression.Add(oldParam, Expression.Constant(1)),
                Expression.Subtract(oldParam, Expression.Constant(2))
            ), oldParam);

        // Act
        var replaced = ParameterReplacerVisitor.Replace(expr.Body, oldParam, newParam);

        // Assert
        var lambda = Expression.Lambda<Func<int, int>>(replaced, newParam);
        var compiled = lambda.Compile();
        Assert.Equal((4 + 1) * (4 - 2), compiled(4));
    }

    [Fact]
    public void Replace_Does_Not_Change_Expression_If_Parameter_Not_Found()
    {
        // Arrange
        ParameterExpression oldParam = Expression.Parameter(typeof(int), "x");
        ParameterExpression newParam = Expression.Parameter(typeof(int), "y");
        ParameterExpression unrelated = Expression.Parameter(typeof(int), "z");
        Expression<Func<int, int>> expr = Expression.Lambda<Func<int, int>>(Expression.Add(unrelated, Expression.Constant(2)), unrelated);

        // Act
        var replaced = ParameterReplacerVisitor.Replace(expr.Body, oldParam, newParam);

        // Assert
        var lambda = Expression.Lambda<Func<int, int>>(replaced, unrelated);
        var compiled = lambda.Compile();
        Assert.Equal(7, compiled(5));
    }

    [Fact]
    public void Replace_Throws_If_Expression_Is_Null()
    {
        // Arrange
        ParameterExpression oldParam = Expression.Parameter(typeof(int), "x");
        ParameterExpression newParam = Expression.Parameter(typeof(int), "y");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ParameterReplacerVisitor.Replace(null!, oldParam, newParam));
    }

    [Fact]
    public void Replace_Throws_If_OldParameter_Is_Null()
    {
        // Arrange
        ParameterExpression newParam = Expression.Parameter(typeof(int), "y");
        Expression expr = Expression.Constant(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ParameterReplacerVisitor.Replace(expr, null!, newParam));
    }

    [Fact]
    public void Replace_Throws_If_NewExpression_Is_Null()
    {
        // Arrange
        ParameterExpression oldParam = Expression.Parameter(typeof(int), "x");
        Expression expr = Expression.Constant(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ParameterReplacerVisitor.Replace(expr, oldParam, null!));
    }

    [Fact]
    public void Replace_Works_With_Nested_Lambda_Expressions_Throws()
    {
        // Arrange
        ParameterExpression oldParam = Expression.Parameter(typeof(int), "x");
        ParameterExpression newParam = Expression.Parameter(typeof(int), "y");
        Expression<Func<int, Func<int, int>>> expr = x => (z => x + z);

        // Act & Assert
        var replaced = ParameterReplacerVisitor.Replace(expr.Body, oldParam, newParam);
        var lambda = Expression.Lambda<Func<int, Func<int, int>>>(replaced, newParam);
        Assert.Throws<InvalidOperationException>(() => {
            var compiled = lambda.Compile();
            var inner = compiled(3);
            _ = inner(5);
        });
    }

    [Fact]
    public void Replace_Works_With_Nested_Lambda_Expressions_Safe()
    {
        // Arrange
        ParameterExpression oldParam = Expression.Parameter(typeof(int), "x");
        ParameterExpression newParam = Expression.Parameter(typeof(int), "y");
        // Only use the parameter in the outer lambda
        Expression<Func<int, Func<int, int>>> expr = x => (z => z + 1);

        // Act
        var replaced = ParameterReplacerVisitor.Replace(expr.Body, oldParam, newParam);

        // Assert
        var lambda = Expression.Lambda<Func<int, Func<int, int>>>(replaced, newParam);
        var compiled = lambda.Compile();
        var inner = compiled(3);
        Assert.Equal(6, inner(5));
    }
}
