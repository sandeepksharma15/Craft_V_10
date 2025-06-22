using HashidsNet;

namespace Craft.CryptKey;

public class HashKeys(HashKeyOptions options)
    : Hashids(options.Salt, options.MinHashLength, options.Alphabet, options.Steps), IHashKeys
{
}
