namespace Craft.Expressions;

/// <summary>
/// Represents a token in an expression string.
/// </summary>
public class Token
{
    /// <summary>
    /// Gets the type of this token.
    /// </summary>
    public TokenType Type { get; }

    /// <summary>
    /// Gets the string value of this token.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the position in the input string where this token starts.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> class.
    /// </summary>
    /// <param name="type">The type of the token.</param>
    /// <param name="value">The string value of the token.</param>
    /// <param name="position">The position in the input string.</param>
    public Token(TokenType type, string value, int position)
    {
        Type = type;
        Value = value;
        Position = position;
    }

    /// <summary>
    /// Returns a string representation of this token.
    /// </summary>
    public override string ToString() => $"{Type}: '{Value}' at {Position}";
}

