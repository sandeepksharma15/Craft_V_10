namespace Craft.Expressions;

/// <summary>
/// Represents a binary operation AST node with a left operand, right operand, and an operator.
/// </summary>
/// <remarks>
/// Binary operations include logical operations (AND, OR), comparison operations (==, !=, &gt;, &lt;, &gt;=, &lt;=),
/// and potentially arithmetic operations.
/// </remarks>
public class BinaryAstNode : AstNode
{
    /// <summary>
    /// Gets the operator for this binary operation (e.g., "&&amp;&amp;", "==", "&gt;").
    /// </summary>
    public string Operator { get; }

    /// <summary>
    /// Gets the left operand of this binary operation.
    /// </summary>
    public AstNode Left { get; }

    /// <summary>
    /// Gets the right operand of this binary operation.
    /// </summary>
    public AstNode Right { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryAstNode"/> class.
    /// </summary>
    /// <param name="op">The operator for this binary operation.</param>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public BinaryAstNode(string op, AstNode left, AstNode right)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    /// <summary>
    /// Returns a string representation of this binary operation.
    /// </summary>
    public override string ToString() => $"({Left} {Operator} {Right})";
}

