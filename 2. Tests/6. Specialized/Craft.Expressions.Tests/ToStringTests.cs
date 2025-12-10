namespace Craft.Expressions.Tests;

public class ToStringTests
{
    [Fact]
    public void BinaryAstNode_ToStringReturnsFormattedExpression()
    {
        // Arrange
        var left = new ConstantAstNode(5);
        var right = new ConstantAstNode(10);
        var node = new BinaryAstNode(">", left, right);

        // Act
        var result = node.ToString();

        // Assert
        Assert.Equal("(5 > 10)", result);
    }

    [Fact]
    public void UnaryAstNode_ToStringReturnsFormattedExpression()
    {
        // Arrange
        var operand = new ConstantAstNode(true);
        var node = new UnaryAstNode("!", operand);

        // Act
        var result = node.ToString();

        // Assert
        Assert.Equal("!True", result);
    }

    [Fact]
    public void ConstantAstNode_ToStringReturnsValue()
    {
        // Arrange
        var stringNode = new ConstantAstNode("test");
        var numberNode = new ConstantAstNode(42);
        var boolNode = new ConstantAstNode(true);
        var nullNode = new ConstantAstNode(null!);

        // Act & Assert
        Assert.Equal("test", stringNode.ToString());
        Assert.Equal("42", numberNode.ToString());
        Assert.Equal("True", boolNode.ToString());
        Assert.Equal("null", nullNode.ToString());
    }

    [Fact]
    public void MemberAstNode_ToStringReturnsDottedPath()
    {
        // Arrange
        var simpleMember = new MemberAstNode(["Name"]);
        var nestedMember = new MemberAstNode(["Company", "Address", "City"]);

        // Act & Assert
        Assert.Equal("Name", simpleMember.ToString());
        Assert.Equal("Company.Address.City", nestedMember.ToString());
    }

    [Fact]
    public void MethodCallAstNode_ToStringReturnsMethodCall()
    {
        // Arrange
        var target = new MemberAstNode(["Name"]);
        var arg = new ConstantAstNode("John");
        var node = new MethodCallAstNode(target, "Contains", [arg]);

        // Act
        var result = node.ToString();

        // Assert
        Assert.Equal("Name.Contains(John)", result);
    }

    [Fact]
    public void MethodCallAstNode_ToStringHandlesMultipleArguments()
    {
        // Arrange
        var target = new MemberAstNode(["Text"]);
        var arg1 = new ConstantAstNode("old");
        var arg2 = new ConstantAstNode("new");
        var node = new MethodCallAstNode(target, "Replace", [arg1, arg2]);

        // Act
        var result = node.ToString();

        // Assert
        Assert.Equal("Text.Replace(old, new)", result);
    }

    [Fact]
    public void Token_ToStringReturnsFormattedString()
    {
        // Arrange
        var token = new Token(TokenType.Identifier, "Name", 0);

        // Act
        var result = token.ToString();

        // Assert
        Assert.Contains("Identifier", result);
        Assert.Contains("Name", result);
        Assert.Contains("0", result);
    }

    [Fact]
    public void ComplexExpression_ToStringShowsStructure()
    {
        // Arrange
        var left = new BinaryAstNode(">", new MemberAstNode(["Age"]), new ConstantAstNode(18));
        var right = new BinaryAstNode("==", new MemberAstNode(["IsActive"]), new ConstantAstNode(true));
        var root = new BinaryAstNode("&&", left, right);

        // Act
        var result = root.ToString();

        // Assert
        Assert.Equal("((Age > 18) && (IsActive == True))", result);
    }
}
