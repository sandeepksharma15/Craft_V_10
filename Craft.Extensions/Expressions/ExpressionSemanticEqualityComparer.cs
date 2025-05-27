using System.Linq.Expressions;

namespace Craft.Extensions.Expressions;

/// <summary>
/// Provides semantic equality comparison for expression trees, normalizing binary expressions
/// to handle commutative and logical equivalence for "==" and "!=".
/// </summary>
public sealed class ExpressionSemanticEqualityComparer : IEqualityComparer<Expression>
{
    public bool Equals(Expression? x, Expression? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.NodeType != y.NodeType) return false;

        var normX = Normalize(x);
        var normY = Normalize(y);

        return normX.ToString() == normY.ToString();
    }

    public int GetHashCode(Expression obj)
        => Normalize(obj).ToString().GetHashCode();

    private static Expression Normalize(Expression expr)
        => new ExpressionNormalizer().Visit(expr);

    /// <summary>
    /// Normalizes expressions for semantic comparison, especially for commutative binary expressions.
    /// </summary>
    private sealed class ExpressionNormalizer : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            // Normalize both sides
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            // For commutative binary expressions, order operands to ensure a canonical form
            if (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual)
            {
                // If both sides are constants or both are member accesses, order by string for consistency
                if (string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal) > 0)
                    (left, right) = (right, left);

                return Expression.MakeBinary(node.NodeType, left, right);
            }

            return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method, node.Conversion);
        }
    }
}
