namespace Craft.Expressions;

/// <summary>
/// Represents a unary operation AST node with a single operand and an operator.
/// </summary>
/// <remarks>
/// Unary operations include logical NOT (!), negation, and other single-operand operations.
/// </remarks>
public class UnaryAstNode : AstNode
{
    /// <summary>
    /// Gets the operator for this unary operation (e.g., "!").
    /// </summary>
    public string Operator { get; }

    /// <summary>
    /// Gets the operand of this unary operation.
    /// </summary>
    public AstNode Operand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnaryAstNode"/> class.
    /// </summary>
    /// <param name="op">The operator for this unary operation.</param>
    /// <param name="operand">The operand.</param>
    public UnaryAstNode(string op, AstNode operand)
    {
        Operator = op;
        Operand = operand;
    }

    /// <summary>
    /// Returns a string representation of this unary operation.
    /// </summary>
    public override string ToString() => $"{Operator}{Operand}";
}

