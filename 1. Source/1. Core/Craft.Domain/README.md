# Craft.Domain

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A foundational Domain-Driven Design (DDD) library for .NET 10 applications. Provides base classes, contracts, and patterns for building robust domain models with proper identity, equality, concurrency control, and event-driven architecture support.

## Table of Contents

- [Installation](#installation)
- [Features](#features)
- [Quick Start](#quick-start)
- [Architecture Overview](#architecture-overview)
- [Core Concepts](#core-concepts)
  - [Entities](#entities)
  - [Value Objects](#value-objects)
  - [Aggregate Roots](#aggregate-roots)
  - [Domain Events](#domain-events)
  - [Data Transfer Objects](#data-transfer-objects)
- [Contracts (Interfaces)](#contracts-interfaces)
- [Exception Handling](#exception-handling)
- [Localization](#localization)
- [Best Practices](#best-practices)
- [Related Projects](#related-projects)

## Installation

Add a reference to `Craft.Domain` in your project:

```xml
<ProjectReference Include="path/to/Craft.Domain.csproj" />
```

## Features

- ✅ **Base Entity Classes** - `BaseEntity<TKey>` with identity, concurrency, and soft-delete
- ✅ **Value Objects** - `ValueObject` and `SingleValueObject<T>` with structural equality
- ✅ **Aggregate Roots** - `IAggregateRoot` marker interface for DDD boundaries
- ✅ **Domain Events** - `IDomainEvent`, `DomainEventBase`, and `IHasDomainEvents`
- ✅ **Data Transfer Objects** - `BaseDto`, `BaseVm`, `BaseModel` with `IDataTransferObject`
- ✅ **Rich Exception Hierarchy** - Categorized exceptions with HTTP status codes
- ✅ **Localization Support** - Resource-backed error messages
- ✅ **Multi-tenancy Support** - `IHasTenant` interface
- ✅ **Optimistic Concurrency** - Built-in concurrency stamps

## Quick Start

### Define an Entity

```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
```

### Define a Value Object

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

### Define an Aggregate Root with Domain Events

```csharp
public class Order : BaseEntity, IAggregateRoot, IHasDomainEvents
{
    private readonly List<OrderLine> _lines = [];
    private readonly DomainEventCollection _events = new();
    
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.DomainEvents;
    
    public void AddLine(Product product, int quantity)
    {
        var line = new OrderLine(product.Id, quantity, product.Price);
        _lines.Add(line);
        AddDomainEvent(new OrderLineAddedEvent(Id, line));
    }
    
    public void AddDomainEvent(IDomainEvent domainEvent) => _events.AddDomainEvent(domainEvent);
    public bool RemoveDomainEvent(IDomainEvent domainEvent) => _events.RemoveDomainEvent(domainEvent);
    public void ClearDomainEvents() => _events.ClearDomainEvents();
}
```

## Architecture Overview

```
Craft.Domain/
├── Abstractions/           # Interfaces and contracts
│   ├── IEntity.cs
│   ├── IHasId.cs
│   ├── IHasConcurrency.cs
│   ├── ISoftDelete.cs
│   ├── IHasTenant.cs
│   ├── IHasUser.cs
│   ├── IHasActive.cs
│   ├── IHasVersion.cs
│   ├── IModel.cs
│   ├── IDataTransferObject.cs
│   ├── IAggregateRoot.cs
│   ├── IDomainEvent.cs
│   └── IHasDomainEvents.cs
├── Base/                   # Base classes
│   ├── BaseEntity.cs
│   ├── BaseModel.cs
│   ├── BaseDTO.cs
│   ├── BaseVm.cs
│   └── ValueObject.cs
├── Events/                 # Domain events
│   └── DomainEventBase.cs
├── Enums/                  # Domain enumerations
├── Exceptions/             # Exception hierarchy
│   ├── Base/
│   ├── Domain/
│   ├── Security/
│   ├── Infrastructure/
│   ├── Client/
│   ├── Server/
│   └── Factories/
├── Extensions/             # Extension methods
├── Helpers/                # Constants and helpers
└── Resources/              # Localization resources
```

## Core Concepts

### Entities

Entities have identity and lifecycle. Use `BaseEntity<TKey>` or `BaseEntity` (which uses `long` as the key type).

```csharp
// With default long key
public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

// With custom key type
public class Document : BaseEntity<Guid>
{
    public string Title { get; set; } = string.Empty;
}
```

**Built-in features:**
- `Id` - Primary key with database generation
- `ConcurrencyStamp` - GUID-based optimistic concurrency
- `IsDeleted` - Soft-delete support
- `Equals()` / `GetHashCode()` - Identity-based equality with tenant awareness
- `IEquatable<BaseEntity<TKey>>` - Type-safe equality

### Value Objects

Value objects are immutable and compared by their component values.

```csharp
// Multi-component value object
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    
    public Address(string street, string city, string postalCode)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
    }
}

// Single-value wrapper (with validation)
public sealed class Email : SingleValueObject<string>
{
    public Email(string value) : base(value)
    {
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format", nameof(value));
    }
    
    private static bool IsValidEmail(string value) 
        => Regex.IsMatch(value, DomainConstants.EmailRegExpr);
}

// Usage
Email email = new("user@example.com");
string value = email; // Implicit conversion to string
```

### Aggregate Roots

Aggregate roots define consistency boundaries. Only aggregate roots should be loaded via repositories.

```csharp
// Mark aggregate roots with the marker interface
public class Order : BaseEntity, IAggregateRoot
{
    // Child entities are accessed only through the aggregate root
    private readonly List<OrderLine> _lines = [];
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
}

// Repository constraint pattern
public interface IRepository<T> where T : class, IAggregateRoot
{
    Task<T?> GetByIdAsync(long id);
    Task AddAsync(T aggregate);
}
```

### Domain Events

Domain events represent something significant that happened in your domain.

```csharp
// Define a domain event
public sealed class OrderPlacedEvent : DomainEventBase
{
    public long OrderId { get; }
    public decimal TotalAmount { get; }
    
    public OrderPlacedEvent(long orderId, decimal totalAmount)
    {
        OrderId = orderId;
        TotalAmount = totalAmount;
    }
}

// Raise events from aggregate roots
public class Order : BaseEntity, IAggregateRoot, IHasDomainEvents
{
    private readonly DomainEventCollection _events = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.DomainEvents;
    
    public void Place()
    {
        // Business logic...
        _events.AddDomainEvent(new OrderPlacedEvent(Id, TotalAmount));
    }
    
    public void AddDomainEvent(IDomainEvent e) => _events.AddDomainEvent(e);
    public bool RemoveDomainEvent(IDomainEvent e) => _events.RemoveDomainEvent(e);
    public void ClearDomainEvents() => _events.ClearDomainEvents();
}
```

**Domain Event Properties:**
- `EventId` - Unique identifier (auto-generated GUID)
- `OccurredOnUtc` - Timestamp when the event occurred
- `EventType` - Type name for routing/serialization
- `CorrelationId` - Optional correlation for tracing
- `CausationId` - Optional link to causing event

### Data Transfer Objects

Three base classes for API communication, all implementing `IDataTransferObject`:

| Class | Purpose | Use Case |
|-------|---------|----------|
| `BaseDto` | API Input | Create/Update requests from client |
| `BaseVm` | API Output | Responses to client |
| `BaseModel` | General | Internal data transfer |

```csharp
// Input DTO for creating/updating
public class ProductDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Output VM with computed properties
public class ProductVm : BaseVm
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string FormattedPrice => Price.ToString("C");
}
```

**Why include `ConcurrencyStamp` and `IsDeleted`?**
- `ConcurrencyStamp` - Client receives with response, sends back on updates for conflict detection
- `IsDeleted` - Enables soft-delete/restore operations via API

## Contracts (Interfaces)

| Interface | Purpose |
|-----------|---------|
| `IEntity<TKey>` | Entities with identity |
| `IHasId<TKey>` | Objects with an identifier |
| `IHasConcurrency` | Optimistic concurrency control |
| `ISoftDelete` | Soft-delete capability |
| `IHasTenant<TKey>` | Multi-tenant entities |
| `IHasUser<TKey>` | User-associated entities |
| `IHasActive` | Activation/deactivation |
| `IHasVersion` | Version tracking |
| `IModel<TKey>` | Data transfer models |
| `IDataTransferObject<TKey>` | API transfer objects |
| `IAggregateRoot<TKey>` | DDD aggregate roots |
| `IDomainEvent` | Domain events |
| `IHasDomainEvents` | Event-raising entities |

## Exception Handling

Categorized exception hierarchy with HTTP status codes:

### Domain Exceptions (4xx)
- `BadRequestException` - 400
- `NotFoundException` - 404
- `AlreadyExistsException` - 409
- `ConflictException` - 409
- `ConcurrencyException` - 409
- `GoneException` - 410
- `PreconditionFailedException` - 412
- `ModelValidationException` - 400

### Security Exceptions
- `UnauthorizedException` - 401
- `ForbiddenException` - 403
- `InvalidCredentialsException` - 401

### Infrastructure Exceptions
- `DatabaseException` - 500
- `ConfigurationException` - 500
- `ExternalServiceException` - 502

### Server Exceptions (5xx)
- `InternalServerException` - 500
- `BadGatewayException` - 502
- `ServiceUnavailableException` - 503
- `GatewayTimeoutException` - 504

### Usage

```csharp
// Throw exceptions
throw new NotFoundException("Product", productId);
throw CraftExceptionFactory.NotFound("Product", productId);

// Convert to JSON-serializable format (for APIs)
try { /* ... */ }
catch (CraftException ex)
{
    var errorInfo = ex.ToErrorInfo(includeStackTrace: false);
    return BadRequest(errorInfo);
}
```

## Localization

Error messages support localization via resource files.

### Using Constants in Attributes (compile-time)

```csharp
[Required(ErrorMessage = DomainConstants.RequiredError)]
[StringLength(100, ErrorMessage = DomainConstants.MaxLengthError)]
public string Name { get; set; }
```

### Using Localized Messages at Runtime

```csharp
// Access localized messages
var message = DomainConstants.Localized.RequiredError;

// Use formatting helpers
var formatted = DomainConstants.Localized.FormatRequired("FirstName");
// Result: "FirstName is required"

// Or use DomainResources directly
var message = DomainResources.FormatEntityNotFound("Product", 42);
// Result: "Entity "Product" (42) was not found."
```

### Adding Translations

Create satellite resource files:
- `DomainResources.fr.resx` - French
- `DomainResources.de.resx` - German
- etc.

## Best Practices

### 1. Entity Design
- Keep entities focused on domain logic
- Use value objects for complex properties
- Implement `IHasDomainEvents` only on aggregate roots

### 2. Aggregate Root Rules
- External code should only reference aggregate roots
- All changes must go through the aggregate root
- Keep aggregates small for better concurrency

### 3. Value Objects
- Make them immutable (use `init` or constructor-only setters)
- Implement validation in constructor
- Override `GetEqualityComponents()` for all relevant properties

### 4. Domain Events
- Name events in past tense (OrderPlaced, UserRegistered)
- Include all relevant data in the event
- Events are immutable facts

### 5. DTOs
- Use `BaseDto` for input (create/update requests)
- Use `BaseVm` for output (API responses)
- Don't expose domain entities directly via API

## Related Projects

- **Craft.Core** - `ServiceResult<T>` and core abstractions
- **Craft.Auditing** - Audit trail tracking
- **Craft.Data** - Entity Framework Core integration
- **Craft.Repositories** - Generic repository pattern

## License

MIT License - see [LICENSE](LICENSE) for details.

## Author

**Sandeep SHARMA**

---

*Built with ❤️ for .NET 10*
