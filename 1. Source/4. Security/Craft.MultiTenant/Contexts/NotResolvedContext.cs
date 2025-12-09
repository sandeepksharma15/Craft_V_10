namespace Craft.MultiTenant;

public class NotResolvedContext
{
    public object? Context { get; set; }

    public string? Identifier { get; set; }

    public NotResolvedContext() { }

    public NotResolvedContext(object context, string identifier)
    {
        Context = context;
        Identifier = identifier;
    }
}
