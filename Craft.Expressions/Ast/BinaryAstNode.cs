namespace Craft.Expressions.Ast;

public class BinaryAstNode : AstNode
{
    public string Operator { get; }
    public AstNode Left { get; }
    public AstNode Right { get; }

    public BinaryAstNode(string op, AstNode left, AstNode right)
    {
        Operator = op;
        Left = left;
        Right = right;
    }
}
