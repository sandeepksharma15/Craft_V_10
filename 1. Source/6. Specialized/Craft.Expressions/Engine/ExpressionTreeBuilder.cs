using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Craft.Expressions;

/// <summary>
/// Builds LINQ expression trees from abstract syntax tree (AST) nodes.
/// </summary>
/// <typeparam name="T">The type of the parameter in the expression.</typeparam>
internal class ExpressionTreeBuilder<T>
{
    /// <summary>
    /// Builds a LINQ expression from an AST node.
    /// </summary>
    /// <param name="node">The AST node to build from.</param>
    /// <param name="param">The parameter expression representing the input value.</param>
    /// <returns>A LINQ expression representing the AST node.</returns>
    /// <exception cref="NotSupportedException">Thrown when an unsupported AST node type or operator is encountered.</exception>
    /// <exception cref="ExpressionEvaluationException">Thrown when a member cannot be resolved on the target type.</exception>
    public Expression Build(AstNode node, ParameterExpression param)
    {
        return node switch
        {
            BinaryAstNode binary => BuildBinary(binary, param),
            UnaryAstNode unary => BuildUnary(unary, param),
            MemberAstNode member => BuildMember(member, param),
            ConstantAstNode constant => BuildConstant(constant),
            MethodCallAstNode methodCall => BuildMethodCall(methodCall, param),
            _ => throw new NotSupportedException($"AST node type '{node.GetType().Name}' is not supported."),
        };
    }

    private Expression BuildBinary(BinaryAstNode node, ParameterExpression param)
    {
        var left = Build(node.Left, param);
        var right = Build(node.Right, param);

        return node.Operator switch
        {
            ExpressionOperators.And => Expression.AndAlso(left, right),
            ExpressionOperators.Or => Expression.OrElse(left, right),
            ExpressionOperators.Equal => Expression.Equal(left, right),
            ExpressionOperators.NotEqual => Expression.NotEqual(left, right),
            ExpressionOperators.GreaterThan => Expression.GreaterThan(left, right),
            ExpressionOperators.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left, right),
            ExpressionOperators.LessThan => Expression.LessThan(left, right),
            ExpressionOperators.LessThanOrEqual => Expression.LessThanOrEqual(left, right),
            _ => throw new NotSupportedException($"Operator '{node.Operator}' is not supported.")
        };
    }

    private Expression BuildUnary(UnaryAstNode node, ParameterExpression param)
    {
        var operand = Build(node.Operand, param);

        return node.Operator switch
        {
            ExpressionOperators.Not => Expression.Not(operand),
            _ => throw new NotSupportedException($"Unary operator '{node.Operator}' is not supported.")
        };
    }

    private static Expression BuildMember(MemberAstNode node, ParameterExpression param)
    {
        Expression expr = param;

        foreach (var member in node.MemberPath)
        {
            var prop = expr.Type.GetProperty(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (prop != null)
            {
                expr = Expression.Property(expr, prop);
                continue;
            }

            var field = expr.Type.GetField(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (field != null)
            {
                expr = Expression.Field(expr, field);
                continue;
            }

            throw new ExpressionEvaluationException("Member not found", expr.Type, member);
        }

        return expr;
    }

    private static Expression BuildConstant(ConstantAstNode node)
    {
        // Try to infer type from value
        object? value = node.Value;

        if (value is string s)
        {
            // Try to parse as number or bool if possible, using invariant culture for consistency
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
                ? Expression.Constant(i)
                : double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d)
                ? Expression.Constant(d)
                : bool.TryParse(s, out var b) ? Expression.Constant(b) : Expression.Constant(s);
        }

        return Expression.Constant(value, value?.GetType() ?? typeof(object));
    }

    private Expression BuildMethodCall(MethodCallAstNode node, ParameterExpression param)
    {
        Expression target = node.Target != null
            ? Build(node.Target, param)
            : throw new ExpressionEvaluationException("Method call must have a target", typeof(T), node.MethodName);

        var argExprs = node.Arguments.Select(arg => Build(arg, param)).ToArray();
        var argTypes = argExprs.Select(a => a.Type).ToArray();

        var method = target.Type.GetMethod(node.MethodName, argTypes);

        if (method == null)
        {
            // Try to find a method with the right name and parameter count (for implicit conversions)
            method = target.Type.GetMethods()
                .FirstOrDefault(m => m.Name == node.MethodName && m.GetParameters().Length == argExprs.Length);

            if (method == null)
                throw new ExpressionEvaluationException($"Method '{node.MethodName}' not found", target.Type, node.MethodName);
        }

        // Convert arguments if needed
        var convertedArgs = method.GetParameters()
            .Select((p, i) => Expression.Convert(argExprs[i], p.ParameterType))
            .ToArray();

        return Expression.Call(target, method, convertedArgs);
    }
}
