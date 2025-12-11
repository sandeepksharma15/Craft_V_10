# Craft.CryptKey

A .NET 10 library for encoding and decoding integer-based IDs into short, unique, obfuscated hash strings using the Hashids algorithm. This library is ideal for protecting sensitive numeric identifiers (like database primary keys) from exposure while maintaining URL-friendly, reversible hashes.

## Features

- **Encode/Decode Long Integers**: Convert `long` values to hash strings and back
- **Customizable Options**: Configure salt, minimum hash length, alphabet, and separators
- **Dependency Injection Support**: Easy integration with ASP.NET Core and other DI containers
- **Type-Safe Extensions**: Extension methods for seamless `KeyType` (alias for `long`) conversions
- **Thread-Safe**: Singleton registration ensures consistent hashing across your application
- **Strongly Typed**: Full support for nullable reference types and .NET 10 features

## Installation

Add the project reference to your solution:

```xml
<ProjectReference Include="path\to\Craft.CryptKey\Craft.CryptKey.csproj" />
```

## Quick Start

### Basic Usage (Without Dependency Injection)

```csharp
using Craft.CryptKey;

// Using extension methods with default options
long userId = 12345;
string hashedId = userId.ToHashKey();  // e.g., "qkDaOEny5z"
long decodedId = hashedId.ToKeyType(); // 12345
```

### Dependency Injection Setup

```csharp
using Craft.CryptKey;
using Microsoft.Extensions.DependencyInjection;

// In your Startup.cs or Program.cs
services.AddHashKeys();
```

### With Custom Configuration

```csharp
services.AddHashKeys(options =>
{
    options.Salt = "YourSecretSalt";
    options.MinHashLength = 15;
    options.Alphabet = "abcdefghijklmnopqrstuvwxyz1234567890";
    options.Steps = "cfhistu";
});
```

### Using IHashKeys Interface

```csharp
public class UserService
{
    private readonly IHashKeys _hashKeys;

    public UserService(IHashKeys hashKeys)
    {
        _hashKeys = hashKeys;
    }

    public string GetObfuscatedUserId(long userId)
    {
        return _hashKeys.EncodeLong(userId);
    }

    public long GetActualUserId(string hashedId)
    {
        var decoded = _hashKeys.DecodeLong(hashedId);
        return decoded[0];
    }
}
```

### Using Extension Methods with IHashKeys

```csharp
public class ProductController : ControllerBase
{
    private readonly IHashKeys _hashKeys;

    public ProductController(IHashKeys hashKeys)
    {
        _hashKeys = hashKeys;
    }

    [HttpGet("{id}")]
    public IActionResult GetProduct(string id)
    {
        long productId = id.ToKeyType(_hashKeys);
        
        return Ok(new { 
            Id = productId.ToHashKey(_hashKeys),
            Name = "Product Name" 
        });
    }
}
```

### Using Extension Methods with IServiceProvider

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHashKeys();
}

public void ProcessRequest(IServiceProvider serviceProvider)
{
    long id = 42;
    string hash = id.ToHashKey(serviceProvider);
    long decoded = hash.ToKeyType(serviceProvider);
}
```

## API Reference

### HashKeyOptions

Configuration options for hash generation.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Salt` | `string` | `"CraftDomainKeySalt"` | Secret salt to make hashes unique |
| `MinHashLength` | `int` | `10` | Minimum length of generated hashes |
| `Alphabet` | `string` | Hashids default alphabet | Characters used in hash generation |
| `Steps` | `string` | Hashids default separators | Separator characters for the algorithm |

### Extension Methods

#### `ToHashKey(this KeyType keyType)`

Converts a `KeyType` (long) value to a hash string using default options.

```csharp
long id = 999;
string hash = id.ToHashKey(); // "qDaOny5zYE"
```

**Throws**: `ArgumentException` if the value is negative.

#### `ToKeyType(this string hashKey)`

Converts a hash string back to a `KeyType` (long) value using default options.

```csharp
string hash = "qDaOny5zYE";
long id = hash.ToKeyType(); // 999
```

**Throws**: 
- `ArgumentException` if the hash is null, empty, or whitespace
- `IndexOutOfRangeException` if the hash is invalid

#### `ToHashKey(this KeyType keyType, IHashKeys hashKeys)`

Converts a `KeyType` using a specific `IHashKeys` instance.

```csharp
long id = 999;
string hash = id.ToHashKey(hashKeys);
```

#### `ToKeyType(this string hashKey, IHashKeys hashKeys)`

Converts a hash string using a specific `IHashKeys` instance.

```csharp
string hash = "qDaOny5zYE";
long id = hash.ToKeyType(hashKeys);
```

#### `ToHashKey(this KeyType keyType, IServiceProvider serviceProvider)`

Converts a `KeyType` using `IHashKeys` resolved from the service provider.

```csharp
long id = 999;
string hash = id.ToHashKey(serviceProvider);
```

#### `ToKeyType(this string hashKey, IServiceProvider serviceProvider)`

Converts a hash string using `IHashKeys` resolved from the service provider.

```csharp
string hash = "qDaOny5zYE";
long id = hash.ToKeyType(serviceProvider);
```

### IHashKeys Interface

The main interface for hash encoding/decoding operations, inherited from Hashids.net's `IHashids`.

#### Key Methods

- `string EncodeLong(params long[] numbers)` - Encode one or more long values
- `long[] DecodeLong(string hash)` - Decode a hash back to long values
- `string Encode(params int[] numbers)` - Encode one or more int values
- `int[] Decode(string hash)` - Decode a hash back to int values
- `string EncodeHex(string hex)` - Encode a hexadecimal string
- `string DecodeHex(string hash)` - Decode to a hexadecimal string

### ServiceCollectionExtensions

#### `AddHashKeys(this IServiceCollection services, Action<HashKeyOptions> configureOptions = null)`

Registers `IHashKeys` and `HashKeyOptions` as singletons in the DI container.

```csharp
// With default options
services.AddHashKeys();

// With custom options
services.AddHashKeys(options =>
{
    options.Salt = "MyAppSalt";
    options.MinHashLength = 20;
});
```

## Usage Examples

### ASP.NET Core Web API

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IHashKeys _hashKeys;
    private readonly IOrderRepository _orderRepository;

    public OrdersController(IHashKeys hashKeys, IOrderRepository orderRepository)
    {
        _hashKeys = hashKeys;
        _orderRepository = orderRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        try
        {
            long orderId = id.ToKeyType(_hashKeys);
            
            var order = await _orderRepository.GetByIdAsync(orderId);
            
            if (order == null)
                return NotFound();
            
            return Ok(new
            {
                Id = order.Id.ToHashKey(_hashKeys),
                CustomerId = order.CustomerId.ToHashKey(_hashKeys),
                Total = order.Total
            });
        }
        catch (ArgumentException)
        {
            return BadRequest("Invalid order ID format");
        }
        catch (IndexOutOfRangeException)
        {
            return BadRequest("Invalid order ID");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        long customerId = request.CustomerIdHash.ToKeyType(_hashKeys);
        
        var order = new Order
        {
            CustomerId = customerId,
            Total = request.Total
        };
        
        await _orderRepository.AddAsync(order);
        
        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id.ToHashKey(_hashKeys) },
            order
        );
    }
}
```

### Encoding Multiple Values

```csharp
var hashKeys = serviceProvider.GetRequiredService<IHashKeys>();

// Encode multiple IDs into a single hash
long[] ids = { 1, 2, 3, 4, 5 };
string multiHash = hashKeys.EncodeLong(ids);

// Decode back to array
long[] decodedIds = hashKeys.DecodeLong(multiHash);
// decodedIds = [1, 2, 3, 4, 5]
```

### Blazor Component

```csharp
@inject IHashKeys HashKeys

<h3>User Profile</h3>

@code {
    [Parameter]
    public string UserId { get; set; } = string.Empty;
    
    private long _actualUserId;
    
    protected override void OnInitialized()
    {
        try
        {
            _actualUserId = UserId.ToKeyType(HashKeys);
        }
        catch (Exception ex)
        {
            // Handle invalid user ID
        }
    }
    
    private string GetShareableLink(long userId)
    {
        return $"/profile/{userId.ToHashKey(HashKeys)}";
    }
}
```

## Configuration Best Practices

### Security Recommendations

1. **Use a Strong Salt**: Never use the default salt in production
2. **Keep Salt Secret**: Store the salt in environment variables or secure configuration
3. **Consistent Configuration**: Use the same configuration across all instances of your application
4. **Minimum Hash Length**: Use at least 10 characters to prevent enumeration attacks

### Example Production Configuration

```csharp
services.AddHashKeys(options =>
{
    options.Salt = configuration["HashKeys:Salt"] 
        ?? throw new InvalidOperationException("HashKeys salt not configured");
    options.MinHashLength = 16;
    
    options.Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
});
```

### appsettings.json

```json
{
  "HashKeys": {
    "Salt": "YOUR-SECRET-SALT-HERE"
  }
}
```

### Environment Variables

```bash
export HashKeys__Salt="YOUR-SECRET-SALT-HERE"
```

## Error Handling

The library throws specific exceptions for different error conditions:

```csharp
try
{
    long id = hashString.ToKeyType();
}
catch (ArgumentException ex)
{
    // Hash is null, empty, or whitespace
    Console.WriteLine("Invalid hash format");
}
catch (IndexOutOfRangeException ex)
{
    // Hash is invalid or corrupted
    Console.WriteLine("Unable to decode hash");
}
```

## Thread Safety

All implementations are thread-safe:
- `HashKeys` is registered as a singleton
- `IHashKeys` can be safely used across multiple threads
- No mutable state in the core implementation

## Performance Considerations

- Hash generation is fast (microseconds for single values)
- Use singleton registration to avoid creating multiple instances
- Cache frequently used hashes if needed for ultra-high performance scenarios
- Encoding/decoding operations are CPU-bound, not I/O-bound

## Testing

The library includes comprehensive unit tests covering:
- Round-trip encoding/decoding
- Edge cases (zero, max values, negative values)
- Custom configurations
- Multiple value encoding
- DI integration
- All extension method overloads

Run tests:

```bash
dotnet test
```

### Test Coverage

- **Total Tests**: 95
- **Pass Rate**: 100%
- **Categories**:
  - HashKeyOptions: 13 tests
  - HashKeys: 25 tests
  - IHashKeys: 17 tests
  - KeyTypeExtensions: 24 tests
  - ServiceCollectionExtensions: 12 tests

## Dependencies

- .NET 10
- Hashids.net 1.7.0
- Craft.Extensions (for internal extension methods)

## Project Structure

```
Craft.CryptKey/
??? HashKeys.cs                      # Main implementation
??? IHashKeys.cs                     # Interface definition
??? HashKeyOptions.cs                # Configuration options
??? KeyTypeExtensions.cs             # Extension methods for KeyType
??? ServiceCollectionExtensions.cs   # DI registration
??? Usings.cs                        # Global using statements
```

## Migration from Craft.Cryptography

If you're migrating from the obsolete `Craft.Cryptography` library:

1. Replace password hashing with ASP.NET Core Identity or BCrypt.Net
2. Replace AES encryption with .NET's built-in `System.Security.Cryptography.Aes`
3. Use `Craft.CryptKey` for ID obfuscation and URL-safe hash generation

## Examples by Scenario

### URL-Friendly IDs

```csharp
// Instead of: https://example.com/api/users/12345
// Use: https://example.com/api/users/qkDaOEny5z
long userId = 12345;
string urlFriendlyId = userId.ToHashKey();
```

### Database Primary Key Protection

```csharp
public class UserDto
{
    public string Id { get; set; } // Hashed ID for external use
    
    public static UserDto FromEntity(User user, IHashKeys hashKeys)
    {
        return new UserDto
        {
            Id = user.Id.ToHashKey(hashKeys),
            // ... other properties
        };
    }
}
```

### API Response Transformation

```csharp
public class ResponseTransformer
{
    private readonly IHashKeys _hashKeys;
    
    public ResponseTransformer(IHashKeys hashKeys)
    {
        _hashKeys = hashKeys;
    }
    
    public object TransformIds(object response)
    {
        // Transform all long IDs to hashed strings
        // Implementation depends on your needs
        return response;
    }
}
```

## Troubleshooting

### Common Issues

**Issue**: Hashes are different across application instances
- **Solution**: Ensure all instances use the same `Salt` configuration

**Issue**: Cannot decode hash
- **Solution**: Verify the hash was generated with the same configuration (salt, alphabet, etc.)

**Issue**: ArgumentException when decoding
- **Solution**: Check that the input string is not null, empty, or whitespace

**Issue**: IndexOutOfRangeException when decoding
- **Solution**: The hash is invalid or was generated with different options

## Contributing

This is part of the Craft framework. Follow the contribution guidelines in the main repository.

## License

See the solution's license file for details.

## Additional Resources

- [Hashids.net Documentation](https://github.com/ullmark/hashids.net)
- [Hashids Website](https://hashids.org/)
- [Craft Framework GitHub](https://github.com/sandeepksharma15/Craft_V_10)

## Support

For issues, questions, or contributions, please refer to the main Craft repository or open an issue on GitHub.

---

**Note**: This library is designed for ID obfuscation, not cryptographic security. Do not use it for encrypting sensitive data. For cryptographic operations, use .NET's built-in `System.Security.Cryptography` namespace.
