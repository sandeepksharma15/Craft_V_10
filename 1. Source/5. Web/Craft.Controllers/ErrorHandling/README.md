# Database Error Handling

This document describes the database error handling architecture implemented in Craft.Controllers.

## Overview

The error handling system uses the **Strategy Pattern** to provide database-specific error messages while keeping the controller code clean and maintainable.

## Architecture

### Components

1. **IDatabaseErrorHandler** - Service interface for handling database exceptions
2. **DatabaseErrorHandler** - Main service that orchestrates error handling
3. **IDatabaseErrorStrategy** - Interface for database-specific strategies
4. **ErrorContext** - Model containing error information

### Strategies

- **PostgreSqlErrorStrategy** - Handles PostgreSQL errors using SqlState codes
- **SqlServerErrorStrategy** - Handles SQL Server errors using error numbers
- **GenericErrorStrategy** - Fallback strategy for pattern matching

## Usage

### 1. Register Services

In your `Program.cs` or startup configuration:

```csharp
using Craft.Controllers.Extensions;

// Add database error handling services
builder.Services.AddDatabaseErrorHandling();
```

### 2. Inject in Controllers

The `EntityChangeController` and `EntityController` base classes automatically inject `IDatabaseErrorHandler`:

```csharp
public class ProductsController : EntityController<Product, ProductDTO, KeyType>
{
    public ProductsController(
        IRepository<Product, KeyType> repository,
        ILogger<EntityController<Product, ProductDTO, KeyType>> logger,
        IDatabaseErrorHandler databaseErrorHandler)
        : base(repository, logger, databaseErrorHandler)
    {
    }
}
```

### 3. Automatic Error Handling

Error handling is automatic in all CRUD operations. Database exceptions are caught and converted to user-friendly messages.

## Supported Databases

### PostgreSQL
- Unique constraint violations (23505)
- Foreign key violations (23503)
- Not null violations (23502)
- Check constraint violations (23514)
- And more... (see PostgreSqlErrorStrategy.cs)

### SQL Server
- Unique constraint violations (2601, 2627)
- Foreign key violations (547)
- Cannot insert null (515)
- String truncation (2628, 8152)
- And more... (see SqlServerErrorStrategy.cs)

### Generic Fallback
- Pattern matching for common error types when specific codes aren't available

## Error Messages

All error messages are:
- **User-friendly**: No technical jargon
- **Actionable**: Tell users what to do
- **Context-aware**: Include entity and field names when available

### Example Messages

- ❌ Technical: `Npgsql.PostgresException: 23505: duplicate key value violates unique constraint "IX_Locations_Name"`
- ✅ User-friendly: `A Location with this Name already exists. Please use a different value.`

## Extending

### Adding New Database Support

1. Create a new strategy implementing `IDatabaseErrorStrategy`:

```csharp
public class MySqlErrorStrategy : IDatabaseErrorStrategy
{
    public bool CanHandle(ErrorContext context) => /* detection logic */;
    public string GetErrorMessage(ErrorContext context) => /* message logic */;
    public int Priority => 3; // Lower = higher priority
}
```

2. Register in `DatabaseErrorHandlingExtensions.cs`:

```csharp
services.AddSingleton<IDatabaseErrorStrategy, MySqlErrorStrategy>();
```

### Custom Error Messages

Override `ReturnProperError` in your controller to customize error handling:

```csharp
protected override ActionResult<Product> ReturnProperError(Exception ex)
{
    // Custom logic
    return base.ReturnProperError(ex);
}
```

## Benefits

✅ **Single Responsibility**: Controllers focus on HTTP concerns  
✅ **Testability**: Error handling logic can be unit tested independently  
✅ **Maintainability**: Easy to add new database providers  
✅ **Reusability**: Service can be used in other contexts  
✅ **Cleaner Code**: Reduced controller from ~500 lines to ~200 lines  

## File Structure

```
Craft.Controllers/
├── Controllers/
│   └── EntityChangeController.cs (~200 lines, down from ~500)
├── ErrorHandling/
│   ├── IDatabaseErrorHandler.cs
│   ├── DatabaseErrorHandler.cs
│   ├── Strategies/
│   │   ├── IDatabaseErrorStrategy.cs
│   │   ├── PostgreSqlErrorStrategy.cs
│   │   ├── SqlServerErrorStrategy.cs
│   │   └── GenericErrorStrategy.cs
│   └── Models/
│       └── ErrorContext.cs
└── Extensions/
    └── DatabaseErrorHandlingExtensions.cs
```

## Technical Details

### Field Name Extraction

The system automatically extracts field names from constraint names:
- `IX_Locations_Name` → "Name"
- `IX_Weeks_YearId_WeekNumber` → "Year Id and Week Number"

### Reflection Usage

Uses reflection to extract database-specific properties:
- `SqlState` (PostgreSQL)
- `Number` (SQL Server)
- `ConstraintName` (All providers)

### Priority System

Strategies are evaluated in priority order:
1. PostgreSQL (Priority 1)
2. SQL Server (Priority 2)
3. ...
999. Generic Fallback (Priority 999)

## Logging

All database errors are logged with technical details while returning sanitized messages to clients:

```
[DatabaseErrorHandler] Database Error - Type: PostgresException, 
SqlState: 23505, Constraint: IX_Locations_Name, Message: duplicate key...
```

---

*For questions or issues, please refer to the Craft.Controllers documentation or create an issue.*
