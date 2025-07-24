namespace Craft.Expressions.Tokens;

public enum TokenType
{
    Identifier,         // e.g. Name, Company, City
    StringLiteral,      // e.g. "John"
    NumberLiteral,      // e.g. 42, 3.14
    BooleanLiteral,     // true, false
    NullLiteral,        // null
    Operator,           // ==, !=, >, <, >=, <=, &&, ||, !
    Dot,                // .
    Comma,              // ,
    OpenParen,          // (
    CloseParen,         // )
    EndOfInput          // (optional, for parser convenience)
}
