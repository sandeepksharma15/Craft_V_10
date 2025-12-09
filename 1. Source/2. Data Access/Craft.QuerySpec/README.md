# Craft.QuerySpec

Craft.QuerySpec is a .NET 10 library that provides a flexible, composable, and type-safe specification pattern for building, combining, and evaluating queries in C# applications. It is designed to enable advanced filtering, sorting, projection, and pagination for LINQ-based data sources, such as Entity Framework Core, with a fluent and extensible API.

## Features
- **Specification Pattern**: Encapsulate query logic as reusable, composable specifications.
- **Fluent Builders**: Create complex filter, sort, and select expressions using builder classes.
- **LINQ Integration**: Apply specifications directly to IQueryable and IEnumerable sources.
- **Advanced Filtering**: Support for property-based, expression-based, and SQL-like search criteria.
- **Sorting**: Multi-level sort order with support for ThenBy/ThenByDescending.
- **Projection**: Select and project results using strongly-typed select builders.
- **Pagination**: Built-in support for skip/take and page-based queries.
- **Post-Processing**: Optional post-processing actions for result transformation.
- **Extensible Evaluators**: Plug in custom evaluators for query customization.
- **Serialization Support**: JSON converters for query objects and builders.
- **.NET 10 & C# 13**: Utilizes the latest language and framework features.

## Getting Started

### Installation
Add a project reference to `Craft.QuerySpec` in your .NET 10 solution:

```
dotnet add reference ../Craft.QuerySpec/Craft.QuerySpec.csproj
```

### Usage Example
```csharp
using Craft.QuerySpec;

// Build a query for filtering and sorting
var query = new Query<MyEntity>
{
    AsNoTracking = true,
    SortOrderBuilder = new SortOrderBuilder<MyEntity>()
        .Add(e => e.Name)
        .Add(e => e.CreatedAt, OrderTypeEnum.OrderByDescending),
    EntityFilterBuilder = new EntityFilterBuilder<MyEntity>()
        .Add(e => e.IsActive)
        .Add("CategoryId", 5)
};

// Apply to IQueryable
var results = query.ApplyTo(dbContext.MyEntities);
```

## Key Components
- `IQuery<T>` / `IQuery<T, TResult>`: Specification interfaces for queries and projections.
- `Query<T>` / `Query<T, TResult>`: Concrete implementations for query specifications.
- `EntityFilterBuilder<T>`: Fluent builder for filter criteria.
- `SortOrderBuilder<T>`: Fluent builder for sort order.
- `QuerySelectBuilder<T, TResult>`: Builder for select/projection expressions.
- `IEvaluator`: Abstraction for query evaluators.
- `QueryEvaluator`: Default evaluator for applying specifications.
- `SqlLikeSearchCriteriaBuilder<T>`: Builder for SQL-like search expressions.
- `OrderTypeEnum`, `ComparisonType`: Enums for sort and filter types.

## Integration
Craft.QuerySpec is designed to be referenced by .NET projects that require advanced, reusable, and testable query logic, especially with Entity Framework Core or any LINQ provider.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
