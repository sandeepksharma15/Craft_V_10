namespace Craft.Expressions;

internal class ExpressionStringParser
{
    private IEnumerator<Token> _tokens = Enumerable.Empty<Token>().GetEnumerator();
    private Token _current = null!;

    public AstNode Parse(IEnumerable<Token> tokens)
    {
        _tokens = tokens.GetEnumerator();

        MoveNext();

        var expr = ParseExpression();

        if (_current.Type != TokenType.EndOfInput)
            throw new InvalidOperationException($"Unexpected token at end: {_current.Value}");

        return expr;
    }

    private void MoveNext()
    {
        if (_tokens.MoveNext())
            _current = _tokens.Current;
        else
            _current = new Token(TokenType.EndOfInput, string.Empty, -1);
    }

    // ParseExpression: handles '||'
    private AstNode ParseExpression()
    {
        var left = ParseAnd();

        while (_current.Type == TokenType.Operator && _current.Value == "||")
        {
            var op = _current.Value;

            MoveNext();

            var right = ParseAnd();

            left = new BinaryAstNode(op, left, right);
        }
        return left;
    }

    // ParseAnd: handles '&&'
    private AstNode ParseAnd()
    {
        var left = ParseEquality();

        while (_current.Type == TokenType.Operator && _current.Value == "&&")
        {
            var op = _current.Value;

            MoveNext();

            var right = ParseEquality();

            left = new BinaryAstNode(op, left, right);
        }
        return left;
    }

    // ParseEquality: handles ==, !=
    private AstNode ParseEquality()
    {
        var left = ParseRelational();

        while (_current.Type == TokenType.Operator && (_current.Value == "==" || _current.Value == "!="))
        {
            var op = _current.Value;

            MoveNext();

            var right = ParseRelational();

            left = new BinaryAstNode(op, left, right);
        }
        return left;
    }

    // ParseRelational: handles >, <, >=, <=
    private AstNode ParseRelational()
    {
        var left = ParseUnary();

        while (_current.Type == TokenType.Operator &&
            (_current.Value == ">" || _current.Value == ">=" || _current.Value == "<" || _current.Value == "<="))
        {
            var op = _current.Value;

            MoveNext();

            var right = ParseUnary();

            left = new BinaryAstNode(op, left, right);
        }
        return left;
    }

    // ParseUnary: handles !
    private AstNode ParseUnary()
    {
        if (_current.Type == TokenType.Operator && _current.Value == "!")
        {
            var op = _current.Value;

            MoveNext();

            var operand = ParseUnary();

            return new UnaryAstNode(op, operand);
        }
        return ParsePrimary();
    }

    // ParsePrimary: identifiers, literals, method calls, member access, parentheses
    private AstNode ParsePrimary()
    {
        // Parenthesized expression
        if (_current.Type == TokenType.OpenParen)
        {
            MoveNext();

            var expr = ParseExpression();

            if (_current.Type != TokenType.CloseParen)
                throw new InvalidOperationException($"Expected ')', got {_current.Value}");

            MoveNext();

            return expr;
        }

        // Identifier: member access or method call
        if (_current.Type == TokenType.Identifier)
        {
            var memberPath = new List<string> { _current.Value };
            MoveNext();

            // Handle dotted member access
            while (_current.Type == TokenType.Dot)
            {
                MoveNext();

                if (_current.Type != TokenType.Identifier)
                    throw new InvalidOperationException($"Expected identifier after '.', got {_current.Value}");

                memberPath.Add(_current.Value);
                MoveNext();
            }

            // Method call
            if (_current.Type == TokenType.OpenParen)
            {
                MoveNext();

                var args = new List<AstNode>();
                if (_current.Type != TokenType.CloseParen)
                    while (true)
                    {
                        args.Add(ParseExpression());

                        if (_current.Type == TokenType.Comma)
                            MoveNext();
                        else
                            break;
                    }

                if (_current.Type != TokenType.CloseParen)
                    throw new InvalidOperationException($"Expected ')', got {_current.Value}");

                MoveNext();

                var methodName = memberPath.Last();
                AstNode? target = memberPath.Count == 1
                    ? null
                    : new MemberAstNode([.. memberPath.Take(memberPath.Count - 1)]);

                return new MethodCallAstNode(target ?? new MemberAstNode([methodName]), methodName, args);
            }

            // Member access
            return new MemberAstNode([.. memberPath]);
        }

        // String literal
        if (_current.Type == TokenType.StringLiteral)
        {
            var value = _current.Value;
            MoveNext();
            return new ConstantAstNode(value);
        }

        // Number literal
        if (_current.Type == TokenType.NumberLiteral)
        {
            var value = _current.Value;
            MoveNext();
            return new ConstantAstNode(value);
        }

        // Boolean literal
        if (_current.Type == TokenType.BooleanLiteral)
        {
            var value = _current.Value == "true";
            MoveNext();
            return new ConstantAstNode(value);
        }

        // Null literal
        if (_current.Type == TokenType.NullLiteral)
        {
            MoveNext();
            return new ConstantAstNode(null!);
        }

        throw new InvalidOperationException($"Unexpected token: {_current.Value}");
    }
}
