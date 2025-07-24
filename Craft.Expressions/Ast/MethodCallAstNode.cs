namespace Craft.Expressions.Ast;

public class MethodCallAstNode : AstNode
{
    public AstNode Target { get; }
    public string MethodName { get; }
    public List<AstNode> Arguments { get; }

    public MethodCallAstNode(AstNode target, string methodName, List<AstNode> arguments)
    {
        Target = target;
        MethodName = methodName;
        Arguments = arguments;
    }
}
