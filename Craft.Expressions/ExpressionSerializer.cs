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
        // Tokenize the string
        var tokens = ExpressionStringTokenizer.Tokenize(expressionString);

        // Parse tokens into AST
        var ast = new ExpressionStringParser().Parse(tokens);

        // Build expression tree from AST
        var param = Expression.Parameter(typeof(T), "x");
        var body = new ExpressionTreeBuilder<T>().Build(ast, param);

        // Return lambda expression
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
