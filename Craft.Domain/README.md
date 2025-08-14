# Craft.Domain

Craft.Domain is a foundational .NET 10 library providing core abstractions, base classes, and helpers for domain-driven design (DDD) in modern applications. It standardizes entity, model, and value object patterns, supporting robust, maintainable, and testable business logic.

## Features
- **Base Entity & Model Classes**: Abstracts common entity and model behaviors, including identity, concurrency, and soft deletion.
- **Domain Abstractions**: Interfaces for entities, models, tenants, users, concurrency, and versioning.
- **Equality & Helper Methods**: Utilities for entity equality, default ID checks, and multi-tenancy detection.
- **Constants & Validation**: Centralized domain constants and regular expressions for validation and error messages.
- **.NET 10 & C# 13**: Leverages the latest language and framework features.

## Usage Example
```csharp
public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

// Equality check
var areEqual = customer1.EntityEquals(customer2);
```

## Key Components
- `BaseEntity<TKey>`, `BaseEntity`: Abstract base classes for entities.
- `IEntity`, `IModel`, `IHasTenant`, `ISoftDelete`, etc.: Domain interfaces for DDD patterns.
- `EntityHelper`: Static helper methods for entity operations.
- `DomainConstants`: Centralized error messages and regex patterns.

## Integration
Craft.Domain is designed to be referenced by other projects (e.g., repositories, services, APIs) to ensure consistent domain modeling and validation.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
