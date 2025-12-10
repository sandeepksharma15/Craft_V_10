namespace Craft.Expressions;

/// <summary>
/// Represents a method call AST node with a target, method name, and arguments.
/// </summary>
/// <remarks>
/// Supports instance method calls (e.g., "Name.Contains(\"John\")").
/// </remarks>
public class MethodCallAstNode : AstNode
{
    /// <summary>
    /// Gets the target expression on which the method is called.
    /// </summary>
    public AstNode Target { get; }

    /// <summary>
    /// Gets the name of the method to call.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Gets the list of arguments to pass to the method.
    /// </summary>
    public List<AstNode> Arguments { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodCallAstNode"/> class.
    /// </summary>
    /// <param name="target">The target expression on which the method is called.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="arguments">The arguments to pass to the method.</param>
    public MethodCallAstNode(AstNode target, string methodName, List<AstNode> arguments)
    {
        Target = target;
        MethodName = methodName;
        Arguments = arguments;
    }

    /// <summary>
    /// Returns a string representation of this method call.
    /// </summary>
    public override string ToString() => $"{Target}.{MethodName}({string.Join(", ", Arguments)})";
}

