namespace Craft.Expressions;

/// <summary>
/// Exception thrown when an expression string cannot be parsed.
/// </summary>
public class ExpressionParseException : Exception
{
    /// <summary>
    /// Gets the position in the input string where the error occurred.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets the token or character that caused the error.
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionParseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="position">The position in the input string where the error occurred.</param>
    /// <param name="token">The token that caused the error.</param>
    public ExpressionParseException(string message, int position, string token)
        : base($"{message} at position {position}: '{token}'")
    {
        Position = position;
        Token = token;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionParseException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="position">The position in the input string where the error occurred.</param>
    /// <param name="token">The token that caused the error.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public ExpressionParseException(string message, int position, string token, Exception innerException)
        : base($"{message} at position {position}: '{token}'", innerException)
    {
        Position = position;
        Token = token;
    }
}
