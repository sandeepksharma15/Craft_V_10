using Craft.CryptKey;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.CryptKey;

public static class KeyTypeExtensions
{
    public static string ToHashKey(this KeyType keyType)
    {
        if (typeof(KeyType).IsIntegral() && keyType < 0)
            throw new ArgumentException("KeyType cannot be negative");

        var options = Activator.CreateInstance<HashKeyOptions>();
        var hashKeys = (HashKeys)Activator.CreateInstance(typeof(HashKeys), options)!;

        return hashKeys!.EncodeLong(keyType);
    }

    public static KeyType ToKeyType(this string hashKey)
    {
        if (hashKey.IsNullOrWhiteSpace())
            throw new ArgumentException("HashKey cannot be null or empty");

        var options = Activator.CreateInstance<HashKeyOptions>();
        var hashKeys = (HashKeys)Activator.CreateInstance(typeof(HashKeys), options)!;

        return hashKeys!.DecodeLong(hashKey)[0];
    }

    // New overloads using DI-registered IHashKeys
    public static string ToHashKey(this KeyType keyType, IHashKeys hashKeys)
    {
        return typeof(KeyType).IsIntegral() && keyType < 0
            ? throw new ArgumentException("KeyType cannot be negative")
            : hashKeys.EncodeLong(keyType);
    }

    public static KeyType ToKeyType(this string hashKey, IHashKeys hashKeys)
    {
        return hashKey.IsNullOrWhiteSpace() ? throw new ArgumentException("HashKey cannot be null or empty") : hashKeys.DecodeLong(hashKey)[0];
    }

    // Optional: Overloads using IServiceProvider for convenience
    public static string ToHashKey(this KeyType keyType, IServiceProvider serviceProvider)
    {
        var hashKeys = serviceProvider.GetRequiredService<IHashKeys>();

        return keyType.ToHashKey(hashKeys);
    }

    public static KeyType ToKeyType(this string hashKey, IServiceProvider serviceProvider)
    {
        var hashKeys = serviceProvider.GetRequiredService<IHashKeys>();

        return hashKey.ToKeyType(hashKeys);
    }
}
