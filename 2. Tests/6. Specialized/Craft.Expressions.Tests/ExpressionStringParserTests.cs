namespace Craft.Expressions.Tests;

public class ExpressionStringParserTests
{
    private static AstNode Parse(string expr)
    {
        var tokens = ExpressionStringTokenizer.Tokenize(expr);
        return new ExpressionStringParser().Parse(tokens);
    }

    [Fact]
    public void Parse_Identifier_ReturnsMemberAstNode()
    {
        // Arrange
        var expr = "Name";

        // Act
        var ast = Parse(expr);

        // Assert
        var member = Assert.IsType<MemberAstNode>(ast);
        Assert.Equal(new[] { "Name" }, member.MemberPath);
    }

    [Fact]
    public void Parse_DottedMember_ReturnsMemberAstNode()
    {
        // Arrange
        var expr = "Company.Name";

        // Act
        var ast = Parse(expr);

        // Assert
        var member = Assert.IsType<MemberAstNode>(ast);
        Assert.Equal(new[] { "Company", "Name" }, member.MemberPath);
    }

    [Fact]
    public void Parse_StringLiteral_ReturnsConstantAstNode()
    {
        // Arrange
        var expr = "\"John\"";

        // Act
        var ast = Parse(expr);

        // Assert
        var constant = Assert.IsType<ConstantAstNode>(ast);
        Assert.Equal("John", constant.Value);
    }

    [Fact]
    public void Parse_NumberLiteral_ReturnsConstantAstNode()
    {
        // Arrange
        var expr = "42";

        // Act
        var ast = Parse(expr);

        // Assert
        var constant = Assert.IsType<ConstantAstNode>(ast);
        Assert.Equal("42", constant.Value);
    }

    [Fact]
    public void Parse_BooleanLiteral_ReturnsConstantAstNode()
    {
        // Arrange
        var expr = "true";

        // Act
        var ast = Parse(expr);

        // Assert
        var constant = Assert.IsType<ConstantAstNode>(ast);
        Assert.Equal(true, constant.Value);
    }

    [Fact]
    public void Parse_NullLiteral_ReturnsConstantAstNode()
    {
        // Arrange
        var expr = "null";

        // Act
        var ast = Parse(expr);

        // Assert
        var constant = Assert.IsType<ConstantAstNode>(ast);
        Assert.Null(constant.Value);
    }

    [Fact]
    public void Parse_ParenthesizedExpression_Works()
    {
        // Arrange
        var expr = "(Name == \"John\")";

        // Act
        var ast = Parse(expr);

        // Assert
        var binary = Assert.IsType<BinaryAstNode>(ast);
        Assert.Equal("==", binary.Operator);
    }

    [Fact]
    public void Parse_BinaryOperators_All()
    {
        // Arrange
        var expr = "A == B && C != D || E > F && G < H || I >= J && K <= L";

        // Act
        var ast = Parse(expr);

        // Assert
        Assert.IsType<BinaryAstNode>(ast); // Top-level should be ||
    }

    [Fact]
    public void Parse_UnaryOperator_Not()
    {
        // Arrange
        var expr = "!IsActive";

        // Act
        var ast = Parse(expr);

        // Assert
        var unary = Assert.IsType<UnaryAstNode>(ast);
        Assert.Equal("!", unary.Operator);
        Assert.IsType<MemberAstNode>(unary.Operand);
    }

    [Fact]
    public void Parse_MethodCall_SingleArg()
    {
        // Arrange
        var expr = "Name.Contains(\"oh\")";

        // Act
        var ast = Parse(expr);

        // Assert
        var method = Assert.IsType<MethodCallAstNode>(ast);
        Assert.Equal("Contains", method.MethodName);
        Assert.Single(method.Arguments);
        Assert.IsType<MemberAstNode>(method.Target);
    }

    [Fact]
    public void Parse_MethodCall_MultipleArgs()
    {
        // Arrange
        var expr = "Name.Replace(\"a\",\"b\")";

        // Act
        var ast = Parse(expr);

        // Assert
        var method = Assert.IsType<MethodCallAstNode>(ast);
        Assert.Equal("Replace", method.MethodName);
        Assert.Equal(2, method.Arguments.Count);
    }

    [Fact]
    public void Parse_MethodCall_NoArgs()
    {
        // Arrange
        var expr = "Name.ToLower()";

        // Act
        var ast = Parse(expr);

        // Assert
        var method = Assert.IsType<MethodCallAstNode>(ast);
        Assert.Equal("ToLower", method.MethodName);
        Assert.Empty(method.Arguments);
    }

    [Fact]
    public void Parse_NestedMethodCallAndMemberAccess()
    {
        // Arrange
        var expr = "Company.Name.ToLower()";

        // Act
        var ast = Parse(expr);

        // Assert
        var method = Assert.IsType<MethodCallAstNode>(ast);
        Assert.Equal("ToLower", method.MethodName);
        Assert.IsType<MemberAstNode>(method.Target);
        var target = (MemberAstNode)method.Target;
        Assert.Equal(new[] { "Company", "Name" }, target.MemberPath);
    }

    [Fact]
    public void Parse_ComplexExpression()
    {
        // Arrange
        var expr = "Company.Name == \"Acme\" && (City == \"London\" || City == \"Paris\")";

        // Act
        var ast = Parse(expr);

        // Assert
        var binary = Assert.IsType<BinaryAstNode>(ast);
        Assert.Equal("&&", binary.Operator);
    }

    [Fact]
    public void Parse_Throws_On_UnexpectedToken()
    {
        // Arrange
        var expr = "$";

        // Act & Assert
        Assert.Throws<ExpressionTokenizationException>(() => Parse(expr));
    }

    [Fact]
    public void Parse_Throws_On_Missing_Paren()
    {
        // Arrange
        var expr = "(Name == \"John\"";

        // Act & Assert
        Assert.Throws<ExpressionParseException>(() => Parse(expr));
    }

    [Fact]
    public void Parse_Throws_On_Unexpected_End()
    {
        // Arrange
        var expr = "Name == ";

        // Act & Assert
        Assert.Throws<ExpressionParseException>(() => Parse(expr));
    }
}
