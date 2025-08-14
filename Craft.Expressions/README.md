# Craft.Expressions

Craft.Expressions is a .NET 10 library for parsing, serializing, and building strongly-typed LINQ expression trees from string-based filter expressions. It enables dynamic querying, filtering, and rule evaluation scenarios by converting user-defined or serialized expressions into executable code.

## Features
- **Expression Parsing**: Converts string filter expressions (e.g., `Age > 18 && Name == "John"`) into abstract syntax trees (AST) and LINQ expression trees.
- **Expression Serialization/Deserialization**: Serialize LINQ expressions to string and deserialize them back to executable expressions.
- **AST Model**: Rich set of AST node types for extensible expression analysis and manipulation.
- **Dynamic Filtering**: Build dynamic filter criteria for querying collections or databases.
- **Type Safety**: Supports strong typing and validation for property access and method calls.
- **Extensible**: Easily extendable for custom operators, methods, or expression types.

## Getting Started

### Installation
Add a project reference to `Craft.Expressions` in your .NET 10 solution:

```
dotnet add reference ../Craft.Expressions/Craft.Expressions.csproj
```

### Usage Example
```csharp
using Craft.Expressions;
using System.Linq.Expressions;

// Parse a string expression into a LINQ Expression<Func<T, bool>>
var serializer = new ExpressionSerializer<Person>();
Expression<Func<Person, bool>> expr = serializer.Deserialize("Age > 18 && Name == \"John\"");

// Serialize an expression back to string
string exprString = serializer.Serialize(expr);
```

## Key Components
- **ExpressionSerializer<T>**: Serializes and deserializes expressions.
- **ExpressionStringParser**: Parses tokens into AST nodes.
- **ExpressionTreeBuilder<T>**: Builds LINQ expression trees from AST.
- **FilterCriteria**: Represents filter conditions for properties.
- **AST Nodes**: `AstNode`, `BinaryAstNode`, `UnaryAstNode`, `MemberAstNode`, `ConstantAstNode`, `MethodCallAstNode`.

## Dependencies
- .NET 10
- Depends on `Craft.Extensions` for utility extensions

## License
See the `LICENSE` file in this directory for details.

---
For more details, review the source code and XML documentation in the project.
