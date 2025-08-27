namespace Craft.Data;

public class ConnectionStringService
{
    private readonly IDictionary<string, IConnectionStringHandler> _handlers = new Dictionary<string, IConnectionStringHandler>();

    public ConnectionStringService(IEnumerable<IConnectionStringHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.GetType().Name.Replace("ConnectionStringHandler", ""), StringComparer.OrdinalIgnoreCase);
    }

    private IConnectionStringHandler ResolveHandler(string dbProvider)
    {
        if (!_handlers.TryGetValue(dbProvider, out var handler))
            throw new NotSupportedException($"Provider '{dbProvider}' is not supported.");

        return handler;
    }

    public string Build(string connectionString, string dbProvider = "postgresql") => ResolveHandler(dbProvider).Build(connectionString);

    public bool Validate(string connectionString, string dbProvider = "postgresql") => ResolveHandler(dbProvider).Validate(connectionString);

    public string Mask(string connectionString, string dbProvider = "postgresql") => ResolveHandler(dbProvider).Mask(connectionString);
}
