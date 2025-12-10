namespace Craft.Expressions;

/// <summary>
/// Defines the types of tokens that can appear in an expression string.
/// </summary>
public enum TokenType
{
    /// <summary>
    /// An identifier (e.g., Name, Company, City).
    /// </summary>
    Identifier,

    /// <summary>
    /// A string literal enclosed in quotes (e.g., "John").
    /// </summary>
    StringLiteral,

    /// <summary>
    /// A numeric literal (e.g., 42, 3.14).
    /// </summary>
    NumberLiteral,

    /// <summary>
    /// A boolean literal (true or false).
    /// </summary>
    BooleanLiteral,

    /// <summary>
    /// The null literal.
    /// </summary>
    NullLiteral,

    /// <summary>
    /// An operator (e.g., ==, !=, &gt;, &lt;, &gt;=, &lt;=, &amp;&amp;, ||, !).
    /// </summary>
    Operator,

    /// <summary>
    /// A dot (.) for member access.
    /// </summary>
    Dot,

    /// <summary>
    /// A comma (,) for separating arguments.
    /// </summary>
    Comma,

    /// <summary>
    /// An opening parenthesis (().
    /// </summary>
    OpenParen,

    /// <summary>
    /// A closing parenthesis ()).
    /// </summary>
    CloseParen,

    /// <summary>
    /// Indicates the end of input.
    /// </summary>
    EndOfInput
}

