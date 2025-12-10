# Craft.Expressions

A powerful and secure .NET library for parsing, serializing, and evaluating LINQ expression trees from string representations. Perfect for dynamic queries, user-defined filters, and expression persistence.

## Features

- ?? **Parse string expressions** into LINQ expression trees
- ?? **Serialize LINQ expressions** back to strings
- ??? **Security hardened** with depth limits and input validation
- ?? **Clean API** with comprehensive XML documentation
- ? **Well-tested** with 76+ unit tests
- ?? **High performance** with minimal allocations
- ?? **Culture-invariant** number parsing for consistency

## Installation

```bash
dotnet add package Craft.Expressions
```

## Quick Start

### Basic Expression Parsing

```csharp
using Craft.Expressions;

// Create a serializer for your type
var serializer = new ExpressionSerializer<Person>();

// Parse a string expression
var expression = serializer.Deserialize("Age > 18 && Name != \"John\"");

// Compile and use it
var filter = expression.Compile();
var isMatch = filter(new Person { Age = 25, Name = "Jane" }); // true
```

### Expression Serialization

```csharp
// Start with a LINQ expression
Expression<Func<Person, bool>> expr = p => p.Age > 18 && p.Name != "John";

// Serialize to string
var serializer = new ExpressionSerializer<Person>();
string text = serializer.Serialize(expr);
// Result: "(Age > 18) && (Name != \"John\")"

// Deserialize back
var restored = serializer.Deserialize(text);
```

## Supported Syntax

### Operators

| Type | Operators | Example |
|------|-----------|---------|
| **Logical** | `&&`, `||`, `!` | `Age > 18 && IsActive` |
| **Comparison** | `==`, `!=`, `>`, `<`, `>=`, `<=` | `Age >= 21` |
| **Grouping** | `(`, `)` | `(Age > 18) && (Age < 65)` |

### Data Types

| Type | Example | Description |
|------|---------|-------------|
| **String** | `"John"`, `"Hello World"` | Enclosed in double quotes |
| **Integer** | `42`, `-10` | Whole numbers |
| **Decimal** | `3.14`, `-0.5` | Floating-point numbers |
| **Boolean** | `true`, `false` | Boolean literals |
| **Null** | `null` | Null literal |

### Member Access

```csharp
// Simple property
"Name == \"John\""

// Nested properties
"Company.Address.City == \"London\""

// Case-insensitive by default
"name == \"John\"" // Works with Name property
```

### Method Calls

```csharp
// String methods
"Name.Contains(\"John\")"
"Name.StartsWith(\"J\")"
"Name.EndsWith(\"son\")"

// Chained methods
"Name.Trim().ToLower() == \"john\""
```

## Advanced Usage

### Dynamic Filtering

```csharp
public IEnumerable<Person> FilterPeople(string filterExpression)
{
    var serializer = new ExpressionSerializer<Person>();
    var expression = serializer.Deserialize(filterExpression);
    var filter = expression.Compile();
    
    return people.Where(filter);
}

// Usage
var adults = FilterPeople("Age >= 18");
var activeUsers = FilterPeople("IsActive == true && LastLogin != null");
```

### With Entity Framework

```csharp
public async Task<List<Person>> GetFilteredPeopleAsync(string filterExpression)
{
    var serializer = new ExpressionSerializer<Person>();
    var expression = serializer.Deserialize(filterExpression);
    
    // Expression is passed directly to EF Core - no compilation needed
    return await dbContext.People.Where(expression).ToListAsync();
}
```

### Storing Filters

```csharp
// Save a user-defined filter
public void SaveUserFilter(string name, Expression<Func<Person, bool>> expression)
{
    var serializer = new ExpressionSerializer<Person>();
    string filterText = serializer.Serialize(expression);
    
    // Store filterText in database
    await db.UserFilters.AddAsync(new UserFilter 
    { 
        Name = name, 
        Expression = filterText 
    });
}

// Load and apply a saved filter
public async Task<List<Person>> ApplySavedFilter(string filterName)
{
    var filter = await db.UserFilters.FirstAsync(f => f.Name == filterName);
    
    var serializer = new ExpressionSerializer<Person>();
    var expression = serializer.Deserialize(filter.Expression);
    
    return await db.People.Where(expression).ToListAsync();
}
```

### Complex Expressions

```csharp
// Multiple conditions
"(Age >= 18 && Age <= 65) && (Status == \"Active\" || Status == \"Pending\")"

// Nested member access with method calls
"Company.Name.Contains(\"Tech\") && Company.Address.City == \"London\""

// Null checking
"Email != null && Email.Contains(\"@example.com\")"
```

## Security Features

### Input Validation

All inputs are validated to prevent common attacks:

```csharp
// Maximum expression length (default: 10,000 characters)
try 
{
    var expr = serializer.Deserialize(veryLongString);
}
catch (ArgumentException ex)
{
    // Expression exceeds maximum length
}
```

### Depth Limits

Protection against stack overflow from deeply nested expressions:

```csharp
// Maximum depth: 100 levels
try 
{
    var expr = serializer.Deserialize(deeplyNestedExpression);
}
catch (ExpressionParseException ex)
{
    // Expression depth exceeds maximum
}
```

### Culture-Invariant Parsing

Numbers are parsed using `InvariantCulture` for consistency across different locales:

```csharp
// Always uses period as decimal separator, regardless of system culture
"Price > 10.50" // Works consistently everywhere
```

## Exception Handling

The library uses specific exception types for better error handling:

### ExpressionTokenizationException

Thrown when the expression string contains invalid syntax:

```csharp
try 
{
    var expr = serializer.Deserialize("Name = \"John\""); // Single = is invalid
}
catch (ExpressionTokenizationException ex)
{
    Console.WriteLine($"Position: {ex.Position}");
    Console.WriteLine($"Character: {ex.Character}");
    // Output: Unexpected '=' (did you mean '==') at position 5: '='
}
```

### ExpressionParseException

Thrown when tokens cannot be parsed into a valid expression:

```csharp
try 
{
    var expr = serializer.Deserialize("Name == "); // Missing right operand
}
catch (ExpressionParseException ex)
{
    Console.WriteLine($"Position: {ex.Position}");
    Console.WriteLine($"Token: {ex.Token}");
}
```

### ExpressionEvaluationException

Thrown when a member or method cannot be found:

```csharp
try 
{
    var expr = serializer.Deserialize("NonExistentProperty == \"test\"");
}
catch (ExpressionEvaluationException ex)
{
    Console.WriteLine($"Type: {ex.TargetType}");
    Console.WriteLine($"Member: {ex.MemberPath}");
}
```

## Performance Considerations

### Compilation Caching

For frequently used expressions, cache the compiled delegates:

```csharp
private readonly ConcurrentDictionary<string, Func<Person, bool>> _cache = new();

public Func<Person, bool> GetCompiledFilter(string expression)
{
    return _cache.GetOrAdd(expression, expr =>
    {
        var serializer = new ExpressionSerializer<Person>();
        return serializer.Deserialize(expr).Compile();
    });
}
```

### EF Core Integration

When using with Entity Framework Core, don't compile the expression:

```csharp
// ? Good - EF translates to SQL
var expression = serializer.Deserialize(filter);
var results = await dbContext.People.Where(expression).ToListAsync();

// ? Bad - Forces client evaluation
var compiled = serializer.Deserialize(filter).Compile();
var results = await dbContext.People.Where(p => compiled(p)).ToListAsync();
```

## Limitations

### Current Limitations

- ? Arithmetic operators (`+`, `-`, `*`, `/`, `%`) not yet supported
- ? Ternary operator (`? :`) not yet supported
- ? Collection operators (`in`, `Any`, `All`) not yet supported
- ? Null-safety operators (`??`, `?.`) not yet supported
- ? Static method calls not supported

### Workarounds

For unsupported features, you can:

1. **Create the expression in code** and serialize it for storage
2. **Use multiple simple expressions** instead of one complex expression
3. **Pre-compute values** and use simple comparisons

## Examples

### Example 1: User Search Filter

```csharp
public class UserSearchFilter
{
    private readonly ExpressionSerializer<User> _serializer = new();
    
    public List<User> Search(List<User> users, string searchCriteria)
    {
        if (string.IsNullOrWhiteSpace(searchCriteria))
            return users;
            
        try
        {
            var expression = _serializer.Deserialize(searchCriteria);
            var filter = expression.Compile();
            return users.Where(filter).ToList();
        }
        catch (Exception ex)
        {
            // Log error and return all users or empty list
            return users;
        }
    }
}

// Usage
var filter = new UserSearchFilter();
var results = filter.Search(allUsers, "Age > 18 && IsActive == true");
```

### Example 2: Saved Report Filters

```csharp
public class ReportFilter
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Expression { get; set; }
}

public class ReportService
{
    private readonly DbContext _db;
    private readonly ExpressionSerializer<Customer> _serializer = new();
    
    public async Task<int> CreateFilter(string name, Expression<Func<Customer, bool>> filter)
    {
        var filterText = _serializer.Serialize(filter);
        var reportFilter = new ReportFilter { Name = name, Expression = filterText };
        
        _db.ReportFilters.Add(reportFilter);
        await _db.SaveChangesAsync();
        
        return reportFilter.Id;
    }
    
    public async Task<List<Customer>> ExecuteFilter(int filterId)
    {
        var filter = await _db.ReportFilters.FindAsync(filterId);
        var expression = _serializer.Deserialize(filter.Expression);
        
        return await _db.Customers.Where(expression).ToListAsync();
    }
}
```

### Example 3: Dynamic Query Builder

```csharp
public class QueryBuilder<T>
{
    private readonly ExpressionSerializer<T> _serializer = new();
    private readonly List<string> _conditions = new();
    
    public QueryBuilder<T> Where(string condition)
    {
        _conditions.Add(condition);
        return this;
    }
    
    public Expression<Func<T, bool>> Build()
    {
        if (_conditions.Count == 0)
            return _ => true;
            
        var combined = string.Join(" && ", _conditions.Select(c => $"({c})"));
        return _serializer.Deserialize(combined);
    }
}

// Usage
var query = new QueryBuilder<Person>()
    .Where("Age >= 18")
    .Where("Age <= 65")
    .Where("IsActive == true")
    .Build();
    
var results = people.Where(query.Compile()).ToList();
```

## Testing

The library includes comprehensive tests covering:

- ? Tokenization of all operator types
- ? Parsing with proper precedence
- ? Expression tree building
- ? Round-trip serialization/deserialization
- ? Error handling and edge cases
- ? Security limits (depth, length)

Run tests:

```bash
dotnet test
```

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Changelog

### Version 1.0.0

- ? Initial release
- ? Core expression parsing and serialization
- ? Security hardening (depth limits, input validation)
- ? Comprehensive XML documentation
- ? Custom exception types
- ? Culture-invariant number parsing

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/sandeepksharma15/Craft_V_10).

---

**Built with ?? using .NET 10**
