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

        x = Normalize(x);
        y = Normalize(y);

        return ExpressionComparer.AreEqual(x, y);
    }

    public int GetHashCode(Expression obj)
    {
        var normalized = Normalize(obj);
        return normalized.ToString().GetHashCode(); // Improve if needed
    }

    private static Expression Normalize(Expression expr)
        => new ExpressionNormalizer().Visit(expr)!;

    /// <summary>
    /// Normalizes expressions to canonical forms (e.g., x == true => x)
    /// </summary>
    private sealed class ExpressionNormalizer : ExpressionVisitor
    {
        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand);
            return Expression.MakeUnary(node.NodeType, operand, node.Type, node.Method);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            // Simplify `x == true` => x
            if (node.NodeType == ExpressionType.Equal)
            {
                if (IsBooleanConstant(right, true)) return left;
                if (IsBooleanConstant(left, true)) return right;
            }

            // Simplify `x == false` => !x
            if (node.NodeType == ExpressionType.Equal)
            {
                if (IsBooleanConstant(right, false)) return Expression.Not(left);
                if (IsBooleanConstant(left, false)) return Expression.Not(right);
            }

            // Simplify `x != false` => x
            if (node.NodeType == ExpressionType.NotEqual)
            {
                if (IsBooleanConstant(right, false)) return left;
                if (IsBooleanConstant(left, false)) return right;
            }

            // Simplify `x != true` => !x
            if (node.NodeType == ExpressionType.NotEqual)
            {
                if (IsBooleanConstant(right, true)) return Expression.Not(left);
                if (IsBooleanConstant(left, true)) return Expression.Not(right);
            }

            // Reorder for commutative binary expressions
            if (IsCommutative(node.NodeType))
            {
                if (string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal) > 0)
                    (left, right) = (right, left);
            }

            return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method, node.Conversion);
        }

        private static bool IsBooleanConstant(Expression expr, bool value)
            => expr is ConstantExpression ce && ce.Type == typeof(bool) && (bool)ce.Value! == value;

        private static bool IsCommutative(ExpressionType nodeType)
            => nodeType is ExpressionType.Equal or ExpressionType.NotEqual
               or ExpressionType.And or ExpressionType.Or
               or ExpressionType.Add or ExpressionType.Multiply;
    }
}

internal static class ExpressionComparer
{
    public static bool AreEqual(Expression x, Expression y)
    {
        return new Impl().Equals(x, y);
    }

    private class Impl : ExpressionVisitor
    {
        private Expression? _y;

        public bool Equals(Expression? x, Expression? y)
        {
            _y = y;
            return Visit(x) != null;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == null || _y == null
                ? node == _y ? node : null
                : node.NodeType != _y.NodeType || node.Type != _y.Type ? null : base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var other = (BinaryExpression)_y!;

            if (node.Method != other.Method || node.IsLifted != other.IsLifted || node.IsLiftedToNull != other.IsLiftedToNull)
                return null!;

            // Handle commutative
            if (IsCommutative(node.NodeType))
            {
                var leftRight = Equals(node.Left, other.Left) && Equals(node.Right, other.Right);
                var rightLeft = Equals(node.Left, other.Right) && Equals(node.Right, other.Left);
                return leftRight || rightLeft ? node : null!;
            }

            return Equals(node.Left, other.Left) && Equals(node.Right, other.Right) ? node : null!;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var other = (MemberExpression)_y!;
            return node.Member == other.Member && Equals(node.Expression, other.Expression) ? node : null!;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var other = (ConstantExpression)_y!;
            return Equals(node.Value, other.Value) ? node : null!;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var other = (ParameterExpression)_y!;
            return node.Name == other.Name && node.Type == other.Type ? node : null!;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var other = (UnaryExpression)_y!;
            return node.Method == other.Method && Equals(node.Operand, other.Operand) ? node : null!;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var other = (LambdaExpression)_y!;
            if (node.Parameters.Count != other.Parameters.Count)
                return null!;

            for (int i = 0; i < node.Parameters.Count; i++)
            {
                if (!Equals(node.Parameters[i], other.Parameters[i]))
                    return null!;
            }

            return Equals(node.Body, other.Body) ? node : null!;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (_y is not MethodCallExpression other)
                return null!;

            if (node.Method != other.Method)
                return null!;

            if (!Equals(node.Object, other.Object))
                return null!;

            if (node.Arguments.Count != other.Arguments.Count)
                return null!;

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                if (!Equals(node.Arguments[i], other.Arguments[i]))
                    return null!;
            }

            return node;
        }

        private static bool IsCommutative(ExpressionType nodeType)
        {
            return nodeType is ExpressionType.Equal or
                   ExpressionType.NotEqual or
                   ExpressionType.Add or
                   ExpressionType.Multiply or
                   ExpressionType.And or
                   ExpressionType.Or;
        }
    }
}
