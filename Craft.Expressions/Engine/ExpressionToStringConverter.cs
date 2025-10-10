using System.Linq.Expressions;

namespace Craft.Expressions;

internal static class ExpressionToStringConverter
{
    public static string Convert(Expression expr)
    {
        return expr switch
        {
            BinaryExpression binary => $"({Convert(binary.Left)} {GetOperator(binary.NodeType)} {Convert(binary.Right)})",
            UnaryExpression unary => $"{GetOperator(unary.NodeType)}{Convert(unary.Operand)}",
            MemberExpression member => SerializeMember(member),
            ConstantExpression constant => SerializeConstant(constant?.Value!),
            MethodCallExpression methodCall => SerializeMethodCall(methodCall),
            _ => throw new NotSupportedException($"Expression type '{expr.NodeType}' is not supported."),
        };
    }

    private static string SerializeMember(MemberExpression member)
    {
        return member.Expression is MemberExpression inner
            ? $"{SerializeMember(inner)}.{member.Member.Name}"
            : member.Expression is ParameterExpression
            ? member.Member.Name
            : throw new NotSupportedException("Only member access on the parameter or its properties is supported.");
    }

    private static string SerializeConstant(object value)
    {
        return value switch
        {
            string s => $"\"{s}\"",
            char c => $"'{c}'",
            bool b => b ? "true" : "false",
            null => "null",
            _ => value?.ToString()!
        };
    }

    private static string SerializeMethodCall(MethodCallExpression methodCall)
    {
        var methodName = methodCall.Method.Name;

        var objectStr = Convert(methodCall?.Object!);

        var args = string.Join(", ", methodCall?.Arguments?.Select(Convert)!);

        return $"{objectStr}.{methodName}({args})";
    }

    private static string GetOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.AndAlso => "&&",
            ExpressionType.OrElse => "||",
            ExpressionType.Equal => "==",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.Not => "!",
            _ => throw new NotSupportedException($"Operator '{nodeType}' is not supported.")
        };
    }
}
