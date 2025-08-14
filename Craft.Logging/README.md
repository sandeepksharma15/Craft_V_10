# Craft.Logging

Craft.Logging is a .NET 10 library providing robust, extensible, and opinionated logging infrastructure for ASP.NET Core and .NET applications. It leverages Serilog for structured logging, context enrichment, and flexible output configuration.

## Features
- **Serilog Integration**: Pre-configured for Serilog, including console sink and log context enrichment.
- **Middleware for Enrichment**: Middleware to enrich logs with tenant, user, and client IDs from HTTP headers.
- **Extension Methods**: Easy integration with `WebApplicationBuilder` for streamlined setup.
- **Custom Logger Utilities**: Static helpers for logger initialization and configuration.
- **.NET 10 & C# 13**: Utilizes the latest language and framework features.

## Usage Example
```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args)
    .AddLogging();

var app = builder.Build();
app.UseSerilogEnrichers();
```

## Key Components
- `LoggingExtensions`: Extension methods for adding Serilog to the builder.
- `SerilogMiddleware`: Middleware for log context enrichment.
- `CraftLogger`: Static logger initialization utility.
- `CraftSerilogOptions`: Options for customizing enrichment property names.

## Integration
Craft.Logging is designed to be referenced by ASP.NET Core and .NET projects to ensure consistent, structured, and context-rich logging across the application.

## Dependencies
- .NET 10
- Serilog, Serilog.AspNetCore, Serilog.Sinks.Console
- Depends on `Craft.Extensions`

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
