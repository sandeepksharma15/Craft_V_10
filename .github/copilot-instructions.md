# Copilot Instructions for Craft Workspace (.NET 10)

## General Coding Standards
- Follow .editorconfig and Roslyn Analyzer rules for code style and quality.
- Use standard .NET naming conventions for classes, methods, variables, and namespaces.
- Place a blank line after any logical set of statements for readability.
- Do not use braces around single statements.

## Design Patterns & Architecture
- Prefer Repository and Factory patterns where applicable.
- Organize abstractions, enums, models, and services in dedicated folders.
- Services with multiple files should have their own folder.

## Dependency Management & Configuration
- Use Dependency Injection for all services and repositories.
- Do not hardcode configuration; use appsettings.json or similar JSON files and IOptions.
- Prefer NuGet packages for common functionality unless a custom implementation is required.

## Logging
- Use Serilog for logging across all projects.

## Documentation & Comments
- Use /// XML documentation comments for all public members to clarify intent.
- Add inline comments for complex logic and to clarify intent before invoking Copilot.

## Error Handling & Exceptions
- Validate all inputs for public APIs and return standardized error responses.
- Use standard exceptions where possible; use custom exceptions for specific modules.

## Security & Input Validation
- Validate all inputs for security and correctness.
- Follow standard security practices for authentication and authorization.

## Frontend (Blazor)
- Use CSS isolation for components where required.
- Use shared styles and Bootstrap for CSS.
- Use MudBlazor as the component library.
- Avoid inline styling.

## Testing (Unit & Integration)
- Use xUnit for all unit and integration tests.
- Use Moq for mocking dependencies.
- Organize tests to mirror the structure of the source code.
- Ensure complete code coverage, including edge cases and exceptions.
- Follow the Arrange-Act-Assert pattern in tests, with relevant comments and blank lines after each section.
- Use [Theory] and [InlineData] where appropriate.
- All new code must be covered by tests before merging.
- Run integration tests in CI/CD pipelines.

## Global State
- Avoid global state unless absolutely necessary.

## Copilot Output Review
- All Copilot-generated code must be reviewed and modified before committing or merging.
- If code does not follow these instructions, regenerate with more context or split the task.

## Branching & Workflow
- Use main, stable, and dev branches for development and releases.
- Ensure all code and tests meet these standards before merging to main or stable.