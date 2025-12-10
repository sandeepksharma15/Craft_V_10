using System.Text;

namespace Craft.Expressions;

/// <summary>
/// Tokenizes an expression string into a sequence of tokens.
/// </summary>
internal class ExpressionStringTokenizer
{
    /// <summary>
    /// Tokenizes the input string into a sequence of tokens.
    /// </summary>
    /// <param name="input">The expression string to tokenize.</param>
    /// <returns>A sequence of tokens representing the expression.</returns>
    /// <exception cref="ExpressionTokenizationException">Thrown when invalid characters or syntax are encountered.</exception>
    public static IEnumerable<Token> Tokenize(string input)
    {
        int pos = 0;

        while (pos < input.Length)
        {
            char c = input[pos];

            // Skip whitespace
            if (char.IsWhiteSpace(c))
            {
                pos++;
                continue;
            }

            // Identifiers or keywords (true, false, null)
            if (char.IsLetter(c) || c == '_')
            {
                yield return TokenizeIdentifierOrKeyword(input, ref pos);
                continue;
            }

            // String literal
            if (c == '"')
            {
                yield return TokenizeStringLiteral(input, ref pos);
                continue;
            }

            // Number literal
            if (char.IsDigit(c))
            {
                yield return TokenizeNumberLiteral(input, ref pos);
                continue;
            }

            // Operators and punctuation (inline, no ref param)
            switch (c)
            {
                case '.':
                    yield return new Token(TokenType.Dot, ".", pos++);
                    break;

                case ',':
                    yield return new Token(TokenType.Comma, ",", pos++);
                    break;

                case '(':
                    yield return new Token(TokenType.OpenParen, "(", pos++);
                    break;

                case ')':
                    yield return new Token(TokenType.CloseParen, ")", pos++);
                    break;

                case '!':
                    if (pos + 1 < input.Length && input[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.Operator, ExpressionOperators.NotEqual, pos);
                        pos += 2;
                    }
                    else
                        yield return new Token(TokenType.Operator, ExpressionOperators.Not, pos++);
                    break;

                case '=':
                    if (pos + 1 < input.Length && input[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.Operator, ExpressionOperators.Equal, pos);
                        pos += 2;
                    }
                    else
                        throw new ExpressionTokenizationException("Unexpected '=' (did you mean '=='?)", pos, '=');
                    break;

                case '>':
                    if (pos + 1 < input.Length && input[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.Operator, ExpressionOperators.GreaterThanOrEqual, pos);
                        pos += 2;
                    }
                    else
                        yield return new Token(TokenType.Operator, ExpressionOperators.GreaterThan, pos++);
                    break;

                case '<':
                    if (pos + 1 < input.Length && input[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.Operator, ExpressionOperators.LessThanOrEqual, pos);
                        pos += 2;
                    }
                    else
                        yield return new Token(TokenType.Operator, ExpressionOperators.LessThan, pos++);
                    break;

                case '&':
                    if (pos + 1 < input.Length && input[pos + 1] == '&')
                    {
                        yield return new Token(TokenType.Operator, ExpressionOperators.And, pos);
                        pos += 2;
                    }
                    else
                        throw new ExpressionTokenizationException("Unexpected '&' (did you mean '&&'?)", pos, '&');
                    break;

                case '|':
                    if (pos + 1 < input.Length && input[pos + 1] == '|')
                    {
                        yield return new Token(TokenType.Operator, ExpressionOperators.Or, pos);
                        pos += 2;
                    }
                    else
                        throw new ExpressionTokenizationException("Unexpected '|' (did you mean '||'?)", pos, '|');
                    break;

                default:
                    throw new ExpressionTokenizationException("Unexpected character", pos, c);
            }
        }

        yield return new Token(TokenType.EndOfInput, string.Empty, pos);
    }

    private static Token TokenizeIdentifierOrKeyword(string input, ref int pos)
    {
        int start = pos;

        while (pos < input.Length && (char.IsLetterOrDigit(input[pos]) || input[pos] == '_'))
            pos++;

        string ident = input[start..pos];

        return ident is "true" or "false"
            ? new Token(TokenType.BooleanLiteral, ident, start)
            : ident == "null" ? new Token(TokenType.NullLiteral, ident, start) : new Token(TokenType.Identifier, ident, start);
    }

    private static Token TokenizeStringLiteral(string input, ref int pos)
    {
        int start = pos;
        pos++; // skip opening quote

        var sb = new StringBuilder();

        while (pos < input.Length)
            if (input[pos] == '\\' && pos + 1 < input.Length)
            {
                sb.Append(input[pos + 1]);
                pos += 2;
            }
            else if (input[pos] == '"')
            {
                pos++; // skip closing quote
                break;
            }
            else
            {
                sb.Append(input[pos]);
                pos++;
            }

        return new Token(TokenType.StringLiteral, sb.ToString(), start);
    }

    private static Token TokenizeNumberLiteral(string input, ref int pos)
    {
        int start = pos;
        bool hasDot = false;

        while (pos < input.Length && (char.IsDigit(input[pos]) || !hasDot && input[pos] == '.'))
        {
            if (input[pos] == '.')
                hasDot = true;

            pos++;
        }

        string num = input[start..pos];

        return new Token(TokenType.NumberLiteral, num, start);
    }
}
