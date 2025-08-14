# Craft.CryptKey

Craft.CryptKey is a .NET 10 library for secure, customizable, and reversible hash-based key generation and obfuscation. It provides a simple abstraction over the Hashids algorithm, making it easy to encode and decode numeric IDs for URLs, APIs, and database keys.

## Features
- **Hashids Integration**: Uses the Hashids algorithm for encoding/decoding integers to short, unique, non-sequential strings.
- **Customizable Options**: Configure salt, alphabet, minimum hash length, and separator steps via `HashKeyOptions`.
- **Dependency Injection Ready**: Register `IHashKeys` in your DI container with a single extension method.
- **Strong Typing**: Provides interfaces and extension methods for type safety and flexibility.
- **.NET 10 Support**: Built for the latest .NET and C# standards.

## Getting Started

### Installation
Add a project reference to `Craft.CryptKey` in your .NET 10 solution:

```
dotnet add reference ../Craft.CryptKey/Craft.CryptKey.csproj
```

### Usage Example
```csharp
using Craft.CryptKey;

// Register in DI
services.AddHashKeys(options =>
{
    options.Salt = "YourSaltHere";
    options.MinHashLength = 8;
});

// Inject and use IHashKeys
public class MyService
{
    private readonly IHashKeys _hashKeys;
    public MyService(IHashKeys hashKeys) => _hashKeys = hashKeys;

    public string EncodeId(long id) => _hashKeys.EncodeLong(id);
    public long DecodeId(string hash) => _hashKeys.DecodeLong(hash).FirstOrDefault();
}
```

## Key Components
- `IHashKeys`: Interface for hash-based key encoding/decoding.
- `HashKeys`: Implementation using Hashids.
- `HashKeyOptions`: Configuration for salt, alphabet, length, and steps.
- `ServiceCollectionExtensions`: DI registration helper.

## Dependencies
- .NET 10
- [Hashids.net](https://github.com/ullmark/hashids.net)
- Depends on `Craft.Extensions`

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
