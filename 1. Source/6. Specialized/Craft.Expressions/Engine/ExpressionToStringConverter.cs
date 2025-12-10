using System.Linq.Expressions;

namespace Craft.Expressions;

/// <summary>
/// Converts LINQ expression trees to their string representations.
/// </summary>
internal static class ExpressionToStringConverter
{
    /// <summary>
    /// Converts a LINQ expression to a string representation.
    /// </summary>
    /// <param name="expr">The expression to convert.</param>
    /// <returns>A string representation of the expression.</returns>
    /// <exception cref="NotSupportedException">Thrown when an unsupported expression type is encountered.</exception>
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
        ArgumentNullException.ThrowIfNull(methodCall);

        if (methodCall.Object == null)
            throw new NotSupportedException("Static method calls are not supported in serialization.");

        var methodName = methodCall.Method.Name;
        var objectStr = Convert(methodCall.Object);
        var args = string.Join(", ", methodCall.Arguments.Select(Convert));

        return $"{objectStr}.{methodName}({args})";
    }

    private static string GetOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.AndAlso => ExpressionOperators.And,
            ExpressionType.OrElse => ExpressionOperators.Or,
            ExpressionType.Equal => ExpressionOperators.Equal,
            ExpressionType.NotEqual => ExpressionOperators.NotEqual,
            ExpressionType.GreaterThan => ExpressionOperators.GreaterThan,
            ExpressionType.GreaterThanOrEqual => ExpressionOperators.GreaterThanOrEqual,
            ExpressionType.LessThan => ExpressionOperators.LessThan,
            ExpressionType.LessThanOrEqual => ExpressionOperators.LessThanOrEqual,
            ExpressionType.Not => ExpressionOperators.Not,
            _ => throw new NotSupportedException($"Operator '{nodeType}' is not supported.")
        };
    }
}
