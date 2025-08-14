# Craft.Extensions

Craft.Extensions is a .NET 10 library providing a comprehensive set of extension methods for collections, LINQ, expressions, system types, and more. It is designed to enhance productivity, code readability, and maintainability across .NET applications.

## Features
- **Collection Extensions**: Utilities for null/empty checks, conditional add/remove, and batch operations on collections and enumerables.
- **LINQ & Query Extensions**: Helpers for composing, combining, and manipulating LINQ queries and expressions.
- **System Type Extensions**: Enhancements for `DateTime`, `Enum`, `Object`, and other core .NET types.
- **Expression Extensions**: Dynamic member access, lambda creation, and expression composition for advanced scenarios.
- **EF Core Extensions**: Helpers for working with `DbSet` and Entity Framework Core.
- **Consistent API**: All extensions follow .NET and C# best practices for discoverability and safety.
- **.NET 10 & C# 13**: Utilizes the latest language and framework features.

## Usage Example
```csharp
using System.Collections.Generic;

var list = new List<int> { 1, 2 };
list.AddIfNotContains(3); // Adds 3 if not present
bool isEmpty = list.IsNullOrEmpty();

using System;
DateTime today = DateTime.Now.ClearTime();
```

## Key Components
- `CollectionExtensions`, `EnumerableExtensions`, `ListExtensions`, `QueriableExtensions`
- `DateTimeExtensions`, `EnumExtensions`, `ObjectExtensions`, `OtherExtensions`, `OtherStringExtensions`
- `ExpressionExtensions`, `ConditionRemover`, `ExpressionSemanticEqualityComparer`
- `DbSetExtensions` (for EF Core)

## Integration
Craft.Extensions is designed to be referenced by all other projects in the Craft ecosystem and can be used in any .NET 10 solution to simplify and standardize code.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
