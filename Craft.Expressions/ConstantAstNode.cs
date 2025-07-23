namespace Craft.Expressions;

public class ConstantAstNode : AstNode
{
    public object Value { get; }

    public ConstantAstNode(object value)
    {
        Value = value;
    }
}
