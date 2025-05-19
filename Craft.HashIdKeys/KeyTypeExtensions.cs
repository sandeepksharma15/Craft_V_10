namespace Craft.HashIdKeys;


public static class KeyTypeExtensions
{
    public static string? ToHashKey(this KeyType keyType)
    {
        var options = Activator.CreateInstance<HashKeyOptions>();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        var hashKeys = (HashKeys)Activator.CreateInstance(typeof(HashKeys), options);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        return hashKeys?.EncodeLong(keyType);
    }

    public static KeyType ToKeyType(this string hashKey)
    {
        var options = Activator.CreateInstance<HashKeyOptions>();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        var hashKeys = (HashKeys)Activator.CreateInstance(typeof(HashKeys), options);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        return hashKeys?.DecodeLong(hashKey)[0] ?? default;
    }
}
