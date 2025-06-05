using System.Linq.Expressions;
using Craft.Extensions.Expressions;

namespace Craft.Extensions.Tests.Expressions;

public class ExpressionSemanticEqualityComparerTests
{
    [Fact]
    public void Equals_ReturnsTrue_ForReferenceEqual()
    {
        var expr = Expression.Constant(5);
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.True(comparer.Equals(expr, expr));
    }

    [Fact]
    public void Equals_ReturnsFalse_IfEitherNull()
    {
        var expr = Expression.Constant(5);
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.False(comparer.Equals(expr, null));
        Assert.False(comparer.Equals(null, expr));
        Assert.True(comparer.Equals(null, null)); // ReferenceEquals(null, null) is true
    }

    [Fact]
    public void Equals_ReturnsFalse_IfNodeTypeDiffers()
    {
        var expr1 = Expression.Constant(1);
        var expr2 = Expression.Parameter(typeof(int));
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.False(comparer.Equals(expr1, expr2));
    }

    [Fact]
    public void Equals_ReturnsTrue_ForEqualExpressions()
    {
        var expr1 = Expression.Equal(Expression.Constant(1), Expression.Constant(2));
        var expr2 = Expression.Equal(Expression.Constant(1), Expression.Constant(2));
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.True(comparer.Equals(expr1, expr2));
    }

    [Fact]
    public void Equals_ReturnsFalse_ForNotEqualExpressions()
    {
        var expr1 = Expression.NotEqual(Expression.Constant(1), Expression.Constant(2));
        var expr2 = Expression.Equal(Expression.Constant(1), Expression.Constant(2));
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.False(comparer.Equals(expr1, expr2));
    }

    [Fact]
    public void Equals_ReturnsTrue_ForCommutativeEquality()
    {
        var a = Expression.Parameter(typeof(int), "a");
        var b = Expression.Parameter(typeof(int), "b");
        var expr1 = Expression.Equal(a, b);
        var expr2 = Expression.Equal(b, a);
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.True(comparer.Equals(expr1, expr2));
    }

    [Fact]
    public void Equals_ReturnsTrue_ForCommutativeNotEqual()
    {
        var a = Expression.Parameter(typeof(int), "a");
        var b = Expression.Parameter(typeof(int), "b");
        var expr1 = Expression.NotEqual(a, b);
        var expr2 = Expression.NotEqual(b, a);
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.True(comparer.Equals(expr1, expr2));
    }

    [Fact]
    public void Equals_ReturnsFalse_ForNonCommutativeBinary()
    {
        var a = Expression.Parameter(typeof(int), "a");
        var b = Expression.Parameter(typeof(int), "b");
        var expr1 = Expression.Subtract(a, b);
        var expr2 = Expression.Subtract(b, a);
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.False(comparer.Equals(expr1, expr2));
    }

    [Fact]
    public void GetHashCode_IsConsistent_ForEqualExpressions()
    {
        var expr1 = Expression.Equal(Expression.Constant(1), Expression.Constant(2));
        var expr2 = Expression.Equal(Expression.Constant(1), Expression.Constant(2));
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.Equal(comparer.GetHashCode(expr1), comparer.GetHashCode(expr2));
    }

    [Fact]
    public void GetHashCode_IsConsistent_ForCommutativeEquality()
    {
        var a = Expression.Parameter(typeof(int), "a");
        var b = Expression.Parameter(typeof(int), "b");
        var expr1 = Expression.Equal(a, b);
        var expr2 = Expression.Equal(b, a);
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.Equal(comparer.GetHashCode(expr1), comparer.GetHashCode(expr2));
    }

    [Fact]
    public void GetHashCode_Differs_ForDifferentExpressions()
    {
        var expr1 = Expression.Equal(Expression.Constant(1), Expression.Constant(2));
        var expr2 = Expression.Equal(Expression.Constant(2), Expression.Constant(3));
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.NotEqual(comparer.GetHashCode(expr1), comparer.GetHashCode(expr2));
    }

    [Fact]
    public void Equals_Works_ForComplexExpressions()
    {
        var a = Expression.Parameter(typeof(int), "a");
        var b = Expression.Parameter(typeof(int), "b");
        var expr1 = Expression.AndAlso(Expression.Equal(a, b), Expression.Constant(true));
        var expr2 = Expression.AndAlso(Expression.Equal(b, a), Expression.Constant(true));
        var comparer = new ExpressionSemanticEqualityComparer();
        Assert.True(comparer.Equals(expr1, expr2));
    }
}
