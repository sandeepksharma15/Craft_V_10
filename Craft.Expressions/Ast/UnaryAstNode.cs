namespace Craft.Expressions.Ast;

public class UnaryAstNode : AstNode
{
    public string Operator { get; }
    public AstNode Operand { get; }

    public UnaryAstNode(string op, AstNode operand)
    {
        Operator = op;
        Operand = operand;
    }
}
