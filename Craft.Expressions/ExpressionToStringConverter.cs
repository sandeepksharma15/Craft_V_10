using System.Linq.Expressions;

internal static class ExpressionToStringConverter
{
    public static string Convert(Expression expr)
    {
        switch (expr)
        {
            case BinaryExpression binary:
                return $"({Convert(binary.Left)} {GetOperator(binary.NodeType)} {Convert(binary.Right)})";
            case UnaryExpression unary:
                return $"{GetOperator(unary.NodeType)}{Convert(unary.Operand)}";
            case MemberExpression member:
                return SerializeMember(member);
            case ConstantExpression constant:
                return SerializeConstant(constant?.Value!);
            case MethodCallExpression methodCall:
                return SerializeMethodCall(methodCall);
            default:
                throw new NotSupportedException($"Expression type '{expr.NodeType}' is not supported.");
        }
    }

    private static string SerializeMember(MemberExpression member)
    {
        if (member.Expression is MemberExpression inner)
            return $"{SerializeMember(inner)}.{member.Member.Name}";

        if (member.Expression is ParameterExpression)
            return member.Member.Name;

        throw new NotSupportedException("Only member access on the parameter or its properties is supported.");
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
