using System.Linq.Expressions;
using System.Reflection;

namespace Craft.Expressions;

internal class ExpressionTreeBuilder<T>
{
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
            "&&" => Expression.AndAlso(left, right),
            "||" => Expression.OrElse(left, right),
            "==" => Expression.Equal(left, right),
            "!=" => Expression.NotEqual(left, right),
            ">" => Expression.GreaterThan(left, right),
            ">=" => Expression.GreaterThanOrEqual(left, right),
            "<" => Expression.LessThan(left, right),
            "<=" => Expression.LessThanOrEqual(left, right),
            _ => throw new NotSupportedException($"Operator '{node.Operator}' is not supported.")
        };
    }

    private Expression BuildUnary(UnaryAstNode node, ParameterExpression param)
    {
        var operand = Build(node.Operand, param);

        return node.Operator switch
        {
            "!" => Expression.Not(operand),
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

            throw new InvalidOperationException($"Member '{member}' not found on type '{expr.Type.Name}'.");
        }
        return expr;
    }

    private static Expression BuildConstant(ConstantAstNode node)
    {
        // Try to infer type from value
        object? value = node.Value;

        if (value is string s)
        {
            // Try to parse as number or bool if possible
            if (int.TryParse(s, out var i)) return Expression.Constant(i);
            if (double.TryParse(s, out var d)) return Expression.Constant(d);
            if (bool.TryParse(s, out var b)) return Expression.Constant(b);

            return Expression.Constant(s);
        }

        return Expression.Constant(value, value?.GetType() ?? typeof(object));
    }

    private Expression BuildMethodCall(MethodCallAstNode node, ParameterExpression param)
    {
        Expression target;

        if (node.Target != null)
            target = Build(node.Target, param);
        else
            throw new InvalidOperationException("Method call must have a target.");

        var argExprs = node.Arguments.Select(arg => Build(arg, param)).ToArray();
        var argTypes = argExprs.Select(a => a.Type).ToArray();

        var method = target.Type.GetMethod(node.MethodName, argTypes);

        if (method == null)
        {
            // Try to find a method with the right name and parameter count (for implicit conversions)
            method = target.Type.GetMethods()
                .FirstOrDefault(m => m.Name == node.MethodName && m.GetParameters().Length == argExprs.Length);

            if (method == null)
                throw new InvalidOperationException($"Method '{node.MethodName}' not found on type '{target.Type.Name}'.");
        }

        // Convert arguments if needed
        var convertedArgs = method.GetParameters()
            .Select((p, i) => Expression.Convert(argExprs[i], p.ParameterType))
            .ToArray();

        return Expression.Call(target, method, convertedArgs);
    }
}
