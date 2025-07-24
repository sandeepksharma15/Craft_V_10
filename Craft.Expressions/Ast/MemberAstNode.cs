namespace Craft.Expressions;

public class MemberAstNode : AstNode
{
    public string[] MemberPath { get; } // e.g. ["Company", "Name"]

    public MemberAstNode(string[] memberPath)
    {
        MemberPath = memberPath;
    }
}
