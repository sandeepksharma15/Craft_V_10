namespace Craft.Expressions;

/// <summary>
/// Exception thrown when an expression string cannot be tokenized.
/// </summary>
public class ExpressionTokenizationException : Exception
{
    /// <summary>
    /// Gets the position in the input string where the error occurred.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets the unexpected character that caused the error.
    /// </summary>
    public char Character { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionTokenizationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="position">The position in the input string where the error occurred.</param>
    /// <param name="character">The unexpected character that caused the error.</param>
    public ExpressionTokenizationException(string message, int position, char character)
        : base($"{message} at position {position}: '{character}'")
    {
        Position = position;
        Character = character;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionTokenizationException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="position">The position in the input string where the error occurred.</param>
    /// <param name="character">The unexpected character that caused the error.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public ExpressionTokenizationException(string message, int position, char character, Exception innerException)
        : base($"{message} at position {position}: '{character}'", innerException)
    {
        Position = position;
        Character = character;
    }
}
