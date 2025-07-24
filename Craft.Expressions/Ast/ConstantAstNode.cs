namespace Craft.Expressions.Ast;

public class ConstantAstNode : AstNode
{
    public object Value { get; }

    public ConstantAstNode(object value)
    {
        Value = value;
    }
}
