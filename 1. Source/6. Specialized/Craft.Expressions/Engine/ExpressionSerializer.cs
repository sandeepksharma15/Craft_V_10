using System.Linq.Expressions;

namespace Craft.Expressions;

/// <summary>
/// Provides serialization and deserialization of LINQ expression trees to and from string representations.
/// </summary>
/// <typeparam name="T">The type of the parameter in the expression.</typeparam>
/// <remarks>
/// This class enables conversion of <see cref="Expression{TDelegate}"/> objects to human-readable strings
/// and reconstruction of expressions from those strings. This is useful for storing filter criteria,
/// dynamic queries, or user-defined expressions in a text format.
/// </remarks>
/// <example>
/// <code>
/// var serializer = new ExpressionSerializer&lt;Person&gt;();
/// 
/// // Serialize an expression to string
/// Expression&lt;Func&lt;Person, bool&gt;&gt; expr = p =&gt; p.Age &gt; 18 &amp;&amp; p.Name != "John";
/// string text = serializer.Serialize(expr); // "(Age &gt; 18) &amp;&amp; (Name != \"John\")"
/// 
/// // Deserialize string back to expression
/// var deserialized = serializer.Deserialize(text);
/// var compiled = deserialized.Compile();
/// bool result = compiled(new Person { Age = 25, Name = "Jane" }); // true
/// </code>
/// </example>
public class ExpressionSerializer<T>
{
    /// <summary>
    /// The maximum allowed length for an expression string to prevent memory exhaustion attacks.
    /// </summary>
    public const int MaxExpressionLength = 10_000;

    /// <summary>
    /// Serializes a boolean expression to its string representation.
    /// </summary>
    /// <param name="expression">The expression to serialize. Cannot be null.</param>
    /// <returns>A string representation of the expression that can be deserialized back.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the expression contains operations that are not supported by the serializer.
    /// </exception>
    public string Serialize(Expression<Func<T, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return ExpressionToStringConverter.Convert(expression.Body);
    }

    /// <summary>
    /// Deserializes a string representation back to a boolean expression.
    /// </summary>
    /// <param name="expressionString">The string representation of the expression. Cannot be null or empty.</param>
    /// <returns>A compiled <see cref="Expression{TDelegate}"/> that can be used for evaluation.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="expressionString"/> is null, empty, whitespace, or exceeds the maximum allowed length.
    /// </exception>
    /// <exception cref="ExpressionTokenizationException">
    /// Thrown when the string contains invalid syntax that cannot be tokenized.
    /// </exception>
    /// <exception cref="ExpressionParseException">
    /// Thrown when the tokens cannot be parsed into a valid expression tree.
    /// </exception>
    /// <exception cref="ExpressionEvaluationException">
    /// Thrown when the expression references members that don't exist on type <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the expression contains operations that are not supported by the deserializer.
    /// </exception>
    public Expression<Func<T, bool>> Deserialize(string expressionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expressionString);

        if (expressionString.Length > MaxExpressionLength)
            throw new ArgumentException(
                $"Expression exceeds maximum length of {MaxExpressionLength} characters. Length: {expressionString.Length}",
                nameof(expressionString));

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
