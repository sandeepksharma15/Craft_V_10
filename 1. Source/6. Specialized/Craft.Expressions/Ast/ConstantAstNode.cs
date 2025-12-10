namespace Craft.Expressions;

/// <summary>
/// Represents a constant value AST node (string, number, boolean, or null).
/// </summary>
public class ConstantAstNode : AstNode
{
    /// <summary>
    /// Gets the constant value represented by this node.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstantAstNode"/> class.
    /// </summary>
    /// <param name="value">The constant value.</param>
    public ConstantAstNode(object value)
    {
        Value = value;
    }

    /// <summary>
    /// Returns a string representation of this constant value.
    /// </summary>
    public override string ToString() => Value?.ToString() ?? "null";
}

