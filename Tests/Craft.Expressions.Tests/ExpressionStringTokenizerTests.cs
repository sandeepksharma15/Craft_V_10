using Craft.Expressions.Tokens;

namespace Craft.Expressions.Tests;

public class ExpressionStringTokenizerTests
{
    [Fact]
    public void Tokenize_Identifier()
    {
        // Arrange
        var input = "Name";

        // Act
        var tokens = ExpressionStringTokenizer.Tokenize(input).ToList();

        // Assert
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("Name", tokens[0].Value);
        Assert.Equal(TokenType.EndOfInput, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_Keyword_True_False_Null()
    {
        // Arrange
        var input = "true false null";

        // Act
        var tokens = ExpressionStringTokenizer.Tokenize(input).ToList();

        // Assert
        Assert.Equal(TokenType.BooleanLiteral, tokens[0].Type);
        Assert.Equal("true", tokens[0].Value);
        Assert.Equal(TokenType.BooleanLiteral, tokens[1].Type);
        Assert.Equal("false", tokens[1].Value);
        Assert.Equal(TokenType.NullLiteral, tokens[2].Type);
        Assert.Equal("null", tokens[2].Value);
        Assert.Equal(TokenType.EndOfInput, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_StringLiteral()
    {
        // Arrange
        var input = "\"Hello World\"";

        // Act
        var tokens = ExpressionStringTokenizer.Tokenize(input).ToList();

        // Assert
        Assert.Equal(TokenType.StringLiteral, tokens[0].Type);
        Assert.Equal("Hello World", tokens[0].Value);
        Assert.Equal(TokenType.EndOfInput, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_StringLiteral_WithEscapedQuote()
    {
        // Arrange
        var input = "\"Hello \\\"World\\\"\"";

        // Act
        var tokens = ExpressionStringTokenizer.Tokenize(input).ToList();

        // Assert
        Assert.Equal(TokenType.StringLiteral, tokens[0].Type);
        Assert.Equal("Hello \"World\"", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_NumberLiteral_Int_And_Decimal()
    {
        // Arrange
        var input = "42 3.14";

        // Act
        var tokens = ExpressionStringTokenizer.Tokenize(input).ToList();

        // Assert
        Assert.Equal(TokenType.NumberLiteral, tokens[0].Type);
        Assert.Equal("42", tokens[0].Value);
        Assert.Equal(TokenType.NumberLiteral, tokens[1].Type);
        Assert.Equal("3.14", tokens[1].Value);
    }

    [Fact]
    public void Tokenize_Operators()
    {
        // Arrange
        var input = "== != > < >= <= && || !";

        // Act
        var tokens = ExpressionStringTokenizer.Tokenize(input).ToList();

        // Assert
        Assert.Equal("==", tokens[0].Value);
        Assert.Equal("!=", tokens[1].Value);
        Assert.Equal(">", tokens[2].Value);
        Assert.Equal("<", tokens[3].Value);
        Assert.Equal(">=", tokens[4].Value);
        Assert.Equal("<=", tokens[5].Value);
        Assert.Equal("&&", tokens[6].Value);
        Assert.Equal("||", tokens[7].Value);
        Assert.Equal("!", tokens[8].Value);
    }

    [Fact]
    public void Tokenize_Dot_Comma_Parentheses()
    {
        // Arrange
        var input = ". , ( )";

        // Act
        var tokens = ExpressionStringTokenizer.Tokenize(input).ToList();

        // Assert
        Assert.Equal(TokenType.Dot, tokens[0].Type);
        Assert.Equal(TokenType.Comma, tokens[1].Type);
        Assert.Equal(TokenType.OpenParen, tokens[2].Type);
        Assert.Equal(TokenType.CloseParen, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_Complex_Expression()
    {
        // Arrange
        var input = "Company.Name == \"Acme\" && (City == \"London\" || City == \"Paris\")";

        // Act
        var tokens = ExpressionStringTokenizer.Tokenize(input).ToList();

        // Assert
        Assert.Equal(TokenType.Identifier, tokens[0].Type); // Company
        Assert.Equal(TokenType.Dot, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type); // Name
        Assert.Equal(TokenType.Operator, tokens[3].Type); // ==
        Assert.Equal(TokenType.StringLiteral, tokens[4].Type); // "Acme"
        Assert.Equal(TokenType.Operator, tokens[5].Type); // &&
        Assert.Equal(TokenType.OpenParen, tokens[6].Type);
        Assert.Equal(TokenType.Identifier, tokens[7].Type); // City
        Assert.Equal(TokenType.Operator, tokens[8].Type); // ==
        Assert.Equal(TokenType.StringLiteral, tokens[9].Type); // "London"
        Assert.Equal(TokenType.Operator, tokens[10].Type); // ||
        Assert.Equal(TokenType.Identifier, tokens[11].Type); // City
        Assert.Equal(TokenType.Operator, tokens[12].Type); // ==
        Assert.Equal(TokenType.StringLiteral, tokens[13].Type); // "Paris"
        Assert.Equal(TokenType.CloseParen, tokens[14].Type);
        Assert.Equal(TokenType.EndOfInput, tokens[15].Type);
    }

    [Fact]
    public void Tokenize_Skips_Whitespace()
    {
        // Arrange
        var input = "  Name   ==   \"John\"  ";

        // Act
        var tokens = ExpressionStringTokenizer.Tokenize(input).ToList();

        // Assert
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("Name", tokens[0].Value);
        Assert.Equal(TokenType.Operator, tokens[1].Type);
        Assert.Equal("==", tokens[1].Value);
        Assert.Equal(TokenType.StringLiteral, tokens[2].Type);
        Assert.Equal("John", tokens[2].Value);
    }

    [Fact]
    public void Tokenize_Throws_On_Unexpected_Character()
    {
        // Arrange
        var input = "Name$";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ExpressionStringTokenizer.Tokenize(input).ToList());
    }

    [Fact]
    public void Tokenize_Throws_On_Unexpected_Single_Equals()
    {
        // Arrange
        var input = "Name = \"John\"";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ExpressionStringTokenizer.Tokenize(input).ToList());
    }

    [Fact]
    public void Tokenize_Throws_On_Unexpected_Single_Ampersand()
    {
        // Arrange
        var input = "Name & \"John\"";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ExpressionStringTokenizer.Tokenize(input).ToList());
    }

    [Fact]
    public void Tokenize_Throws_On_Unexpected_Single_Pipe()
    {
        // Arrange
        var input = "Name | \"John\"";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ExpressionStringTokenizer.Tokenize(input).ToList());
    }
}
