using System.Linq.Expressions;

namespace Craft.Expressions;

public class ExpressionSerializer<T>
{
    public string Serialize(Expression<Func<T, bool>> expression)
    {
        return ExpressionToStringConverter.Convert(expression.Body);
    }

    public Expression<Func<T, bool>> Deserialize(string expressionString)
    {
        return null!;
    }
}
