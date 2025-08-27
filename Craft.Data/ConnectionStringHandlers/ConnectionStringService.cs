using Microsoft.Extensions.Options;

namespace Craft.Data;

public class ConnectionStringService
{
    private readonly IDictionary<string, IConnectionStringHandler> _handlers = new Dictionary<string, IConnectionStringHandler>();
    private readonly DatabaseOptions _dbOptions;

    public ConnectionStringService(IEnumerable<IConnectionStringHandler> handlers, IOptions<DatabaseOptions> dbOptions)
    {
        _handlers = handlers.ToDictionary(h => h.GetType().Name.Replace("ConnectionStringHandler", ""), StringComparer.OrdinalIgnoreCase);
        _dbOptions = dbOptions.Value;
    }

    private IConnectionStringHandler ResolveHandler(string dbProvider)
    {
        if (!_handlers.TryGetValue(dbProvider, out var handler))
            throw new NotSupportedException($"Provider '{dbProvider}' is not supported.");

        return handler;
    }

    public string Build() => ResolveHandler(_dbOptions.DbProvider).Build(_dbOptions);

    public bool Validate() => ResolveHandler(_dbOptions.DbProvider).Validate(_dbOptions.ConnectionString);

    public string Mask() => ResolveHandler(_dbOptions.DbProvider).Mask(_dbOptions.ConnectionString);
}
