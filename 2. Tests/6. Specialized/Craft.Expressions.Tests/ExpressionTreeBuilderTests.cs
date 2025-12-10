using System.Linq.Expressions;

namespace Craft.Expressions.Tests;

public class ExpressionTreeBuilderTests
{
    private class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public TestClass? Child { get; set; }
    }

    private static ParameterExpression Param => Expression.Parameter(typeof(TestClass), "x");
    private static ExpressionTreeBuilder<TestClass> Builder => new();

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2032:Type assertions based on 'assignable from' are confusingly named", Justification = "<Pending>")]
    public void Build_MemberAstNode_ReturnsMemberExpression()
    {
        // Arrange
        var node = new MemberAstNode(["Name"]);

        // Act
        var expr = Builder.Build(node, Param);

        // Assert
        Assert.IsAssignableFrom<MemberExpression>(expr);
        Assert.Equal("Name", ((MemberExpression)expr).Member.Name);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2032:Type assertions based on 'assignable from' are confusingly named", Justification = "<Pending>")]
    public void Build_MemberAstNode_Nested_ReturnsMemberExpression()
    {
        // Arrange
        var node = new MemberAstNode(["Child", "Name"]);

        // Act
        var expr = Builder.Build(node, Param);

        // Assert
        Assert.IsAssignableFrom<MemberExpression>(expr);
        Assert.Equal("Name", ((MemberExpression)expr).Member.Name);
    }

    [Fact]
    public void Build_ConstantAstNode_String_Int_Bool_Null()
    {
        // Arrange
        var stringNode = new ConstantAstNode("hello");
        var intNode = new ConstantAstNode("42");
        var boolNode = new ConstantAstNode("true");
        var nullNode = new ConstantAstNode(null!);

        // Act
        var exprString = Builder.Build(stringNode, Param);
        var exprInt = Builder.Build(intNode, Param);
        var exprBool = Builder.Build(boolNode, Param);
        var exprNull = Builder.Build(nullNode, Param);

        // Assert
        Assert.Equal("hello", ((ConstantExpression)exprString).Value);
        Assert.Equal(42, ((ConstantExpression)exprInt).Value);
        Assert.Equal(true, ((ConstantExpression)exprBool).Value);
        Assert.Null(((ConstantExpression)exprNull).Value);
    }

    [Fact]
    public void Build_ConstantAstNode_Double()
    {
        // Arrange
        var doubleNode = new ConstantAstNode("3.14");

        // Act
        var expr = Builder.Build(doubleNode, Param);

        // Assert
        Assert.Equal(3.14, ((ConstantExpression)expr).Value);
    }

    [Fact]
    public void Build_BinaryAstNode_AllOperators()
    {
        // Arrange
        var leftInt = new MemberAstNode(["Age"]);
        var rightInt = new ConstantAstNode("18");
        var leftBool = new MemberAstNode(["IsActive"]);
        var rightBool = new ConstantAstNode("true");
        var opCases = new[]
        {
            ("==", leftInt, rightInt),
            ("!=", leftInt, rightInt),
            (">", leftInt, rightInt),
            (">=", leftInt, rightInt),
            ("<", leftInt, rightInt),
            ("<=", leftInt, rightInt),
            ("&&", leftBool, rightBool),
            ("||", leftBool, rightBool)
        };

        // Act & Assert
        foreach (var (op, left, right) in opCases)
        {
            var node = new BinaryAstNode(op, left, right);
            var expr = Builder.Build(node, Param);
            Assert.IsType<BinaryExpression>(expr, exactMatch: false);
        }
    }

    [Fact]
    public void Build_BinaryAstNode_UnsupportedOperator_Throws()
    {
        // Arrange
        var node = new BinaryAstNode("^^", new ConstantAstNode("1"), new ConstantAstNode("2"));

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => Builder.Build(node, Param));
    }

    [Fact]
    public void Build_UnaryAstNode_Not()
    {
        // Arrange
        var operand = new MemberAstNode(["IsActive"]);
        var node = new UnaryAstNode("!", operand);

        // Act
        var expr = Builder.Build(node, Param);

        // Assert
        Assert.IsType<UnaryExpression>(expr);
        Assert.Equal(ExpressionType.Not, ((UnaryExpression)expr).NodeType);
    }

    [Fact]
    public void Build_UnaryAstNode_UnsupportedOperator_Throws()
    {
        // Arrange
        var node = new UnaryAstNode("~", new ConstantAstNode("1"));

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => Builder.Build(node, Param));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2032:Type assertions based on 'assignable from' are confusingly named", Justification = "<Pending>")]
    public void Build_MethodCallAstNode_SingleArg()
    {
        // Arrange
        var target = new MemberAstNode(["Name"]);
        var arg = new ConstantAstNode("h");
        var node = new MethodCallAstNode(target, "Contains", [arg]);

        // Act
        var expr = Builder.Build(node, Param);

        // Assert
        Assert.IsAssignableFrom<MethodCallExpression>(expr);
        Assert.Equal("Contains", ((MethodCallExpression)expr).Method.Name);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2032:Type assertions based on 'assignable from' are confusingly named", Justification = "<Pending>")]
    public void Build_MethodCallAstNode_MultipleArgs()
    {
        // Arrange
        var target = new MemberAstNode(["Name"]);
        var args = new List<AstNode> { new ConstantAstNode("a"), new ConstantAstNode("b") };
        var node = new MethodCallAstNode(target, "Replace", args);

        // Act
        var expr = Builder.Build(node, Param);

        // Assert
        Assert.IsAssignableFrom<MethodCallExpression>(expr);
        Assert.Equal("Replace", ((MethodCallExpression)expr).Method.Name);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2032:Type assertions based on 'assignable from' are confusingly named", Justification = "<Pending>")]
    public void Build_MethodCallAstNode_NoArgs()
    {
        // Arrange
        var target = new MemberAstNode(["Name"]);
        var node = new MethodCallAstNode(target, "ToLower", []);

        // Act
        var expr = Builder.Build(node, Param);

        // Assert
        Assert.IsAssignableFrom<MethodCallExpression>(expr);
        Assert.Equal("ToLower", ((MethodCallExpression)expr).Method.Name);
    }

    [Fact]
    public void Build_MethodCallAstNode_UnsupportedMethod_Throws()
    {
        // Arrange
        var target = new MemberAstNode(["Name"]);
        var node = new MethodCallAstNode(target, "NotAMethod", []);

        // Act & Assert
        Assert.Throws<ExpressionEvaluationException>(() => Builder.Build(node, Param));
    }

    [Fact]
    public void Build_MethodCallAstNode_NullTarget_Throws()
    {
        // Arrange
        var node = new MethodCallAstNode(null!, "ToLower", []);

        // Act & Assert
        Assert.Throws<ExpressionEvaluationException>(() => Builder.Build(node, Param));
    }

    [Fact]
    public void Build_MemberAstNode_UnknownMember_Throws()
    {
        // Arrange
        var node = new MemberAstNode(["NotAProp"]);

        // Act & Assert
        Assert.Throws<ExpressionEvaluationException>(() => Builder.Build(node, Param));
    }

    [Fact]
    public void Build_UnknownAstNodeType_Throws()
    {
        // Arrange
        var node = new DummyAstNode();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => Builder.Build(node, Param));
    }

    private class DummyAstNode : AstNode { }
}
