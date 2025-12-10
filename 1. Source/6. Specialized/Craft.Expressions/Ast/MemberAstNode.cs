namespace Craft.Expressions;

/// <summary>
/// Represents a member access AST node (property or field access).
/// </summary>
/// <remarks>
/// Supports nested member access (e.g., "Company.Address.City").
/// </remarks>
public class MemberAstNode : AstNode
{
    /// <summary>
    /// Gets the path of members to access (e.g., ["Company", "Name"]).
    /// </summary>
    public string[] MemberPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberAstNode"/> class.
    /// </summary>
    /// <param name="memberPath">The path of members to access.</param>
    public MemberAstNode(string[] memberPath)
    {
        MemberPath = memberPath;
    }

    /// <summary>
    /// Returns a string representation of this member access.
    /// </summary>
    public override string ToString() => string.Join(".", MemberPath);
}

