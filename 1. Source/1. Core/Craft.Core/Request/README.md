# Craft.Core Requests: GetPagedRequest

This module provides request models for paginated data retrieval in .NET 10 applications, supporting flexible and type-safe API patterns for pagination.

## Features
- **Generic Pagination**: `GetPagedRequest<TKEY>` allows specifying the key type for advanced scenarios.
- **Default Pagination**: `GetPagedRequest` uses the default key type for convenience.
- **APIRequest Inheritance**: Integrates with the broader API request pattern for consistent request handling.
- **Customizable**: Easily set current page, page size, and whether to include detailed data.
- **.NET 10 & C# 13**: Leverages the latest language and framework features.

## Usage Example
```csharp
// For a custom key type
var request = new GetPagedRequest<Guid>(currentPage: 2, pageSize: 25, includeDetails: true);

// For the default key type
var request = new GetPagedRequest(currentPage: 1, pageSize: 10);
```

## Key Properties
- `CurrentPage`: The page number to retrieve (default: 1).
- `PageSize`: The number of items per page (default: 10).
- `IncludeDetails`: Whether to include detailed data in the response.

## Integration
These request models are typically used with API endpoints or repository methods that support pagination, enabling efficient data access and UI paging.

## License
See the root LICENSE file for details.

---
For more details, review the source code and XML documentation in the project.
