using Craft.CryptKey;

namespace Craft.Domain.HashIdentityKey;

public static class KeyTypeExtensions
{
    public static string ToHashKey(this KeyType keyType)
    {
        // Throw an exception if the keyType is numeric and negative
        if (typeof(KeyType).IsIntegral() && keyType < 0)
            throw new ArgumentException("KeyType cannot be negative");

        var options = Activator.CreateInstance<HashKeyOptions>();
        var hashKeys = (HashKeys)Activator.CreateInstance(typeof(HashKeys), options)!;

        return hashKeys!.EncodeLong(keyType);
    }

    public static KeyType ToKeyType(this string hashKey)
    {
        if (hashKey.IsNullOrEmpty())
            throw new ArgumentException("HashKey cannot be null or empty");

        var options = Activator.CreateInstance<HashKeyOptions>();
        var hashKeys = (HashKeys)Activator.CreateInstance(typeof(HashKeys), options)!;

        return hashKeys!.DecodeLong(hashKey)[0];
    }
}
