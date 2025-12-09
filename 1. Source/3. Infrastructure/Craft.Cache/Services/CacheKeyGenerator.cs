using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Craft.Cache;

/// <summary>
/// Default implementation of cache key generator using consistent hashing.
/// </summary>
public class CacheKeyGenerator : ICacheKeyGenerator
{
    private readonly string _prefix;
    private readonly string _separator;

    public CacheKeyGenerator(string prefix = "craft", string separator = ":")
    {
        _prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        _separator = separator ?? throw new ArgumentNullException(nameof(separator));
    }

    public string Generate(Type type, params object[] parameters)
    {
        ArgumentNullException.ThrowIfNull(type);

        var parts = new List<string> { _prefix, type.Name };

        if (parameters != null && parameters.Length > 0)
        {
            var paramHash = GenerateParameterHash(parameters);
            parts.Add(paramHash);
        }

        return string.Join(_separator, parts);
    }

    public string GenerateMethodKey(string typeName, string methodName, params object[] arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        var parts = new List<string> { _prefix, typeName, methodName };

        if (arguments != null && arguments.Length > 0)
        {
            var paramHash = GenerateParameterHash(arguments);
            parts.Add(paramHash);
        }

        return string.Join(_separator, parts);
    }

    public string GenerateEntityKey<TEntity>(object id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var entityName = typeof(TEntity).Name;
        return $"{_prefix}{_separator}{entityName}{_separator}{id}";
    }

    public string GenerateEntityPattern<TEntity>(string? pattern = null)
    {
        var entityName = typeof(TEntity).Name;
        var basePattern = $"{_prefix}{_separator}{entityName}";

        return string.IsNullOrWhiteSpace(pattern)
            ? $"{basePattern}{_separator}*"
            : $"{basePattern}{_separator}{pattern}";
    }

    private static string GenerateParameterHash(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return string.Empty;

        var serialized = JsonSerializer.Serialize(parameters);
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(serialized));
        return Convert.ToHexString(hash).ToLowerInvariant()[..16]; // Use first 16 chars
    }
}
