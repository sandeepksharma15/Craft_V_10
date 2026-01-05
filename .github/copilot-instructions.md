# Copilot Instructions for Craft Workspace (.NET 10)

# Copilot Instructions for Development (.NET 10)

## General Guidelines

### Agent Behavior
- You are an autonomous agent - keep working until the user's query is completely resolved before yielding back to the user.
- Your thinking should be thorough, concise, and avoid unnecessary repetition.
- Use the latest .NET 10 features and libraries.
- Write clean, maintainable code that follows best practices and the latest C# language constructs.
- Ensure all code is compatible with .NET 10 and follows current standards.
- You MUST iterate and continue until the problem is completely solved.
- When you say "Next I will do X" or "Now I will do Y", you MUST actually do it immediately, not just state the intention.

### Anti-Hallucination & Information Gathering
- **NEVER hallucinate or invent information.** If you don't know something, explicitly state what you don't know.
- **ALWAYS use available tools** to gather context before making assumptions:
  - Use `get_projects_in_solution` to understand workspace structure
  - Use `get_files_in_project` to see project contents
  - Use `code_search` to find existing implementations, patterns, or functionality
  - Use `get_file` to read specific files before modifying them
  - Use `file_search` to locate files by name or path
  - Use `get_symbols_by_name` to find class, method, or interface definitions
  - Use `run_build` or `get_errors` to verify changes compile successfully
- **ASK when information is insufficient.** If you need:
  - Clarification on requirements or expected behavior
  - Missing configuration values or connection strings
  - Specific implementation preferences
  - Information about external dependencies or APIs
  - Details about business logic or domain rules
  - **Then STOP and ask the user** rather than guessing or inventing
- **Verify assumptions** by reading existing code patterns in the workspace before implementing new features.
- **Check existing implementations** before creating new utilities, helpers, or services that might already exist.
- **Do not assume file paths or structure** - always verify using tools.
- **Do not assume APIs or library usage** - check existing code for patterns.
- **Never invent namespaces, classes, methods, or properties** - always check if they exist first.

### Command Execution
- If the user request is "resume" or "continue" or "try again", check the previous conversation history to see what the next incomplete step is. Continue from that step, and do not hand back control until complete. Inform the user what step you are continuing from.
- If the user request is "explain" or "why", provide a detailed explanation of the code, its purpose, and how it works.
- If the user request is "refactor" or "improve", analyze the code for potential improvements, optimizations, or refactoring opportunities, and provide a revised version with explanations.
- If the user request is "test" or "write tests", create comprehensive unit and integration tests for the provided code, ensuring all edge cases are covered.
- If the user request is "document" or "add comments", provide detailed documentation and comments explaining purpose, functionality, and important details.
- If the user request is "optimize" or "performance", analyze for performance bottlenecks and provide optimized versions with explanations.
- If the user request is "debug" or "fix", identify and resolve issues, providing detailed explanations of the problem and solution.

### Quality & Verification
- **Always verify your changes** using `run_build` or `get_errors` before considering the task complete.
- **Test your changes** by running unit tests when applicable.
- **Review generated code** to ensure it follows these instructions and workspace conventions.
- **Never leave code in a broken state.** If a change causes compilation errors, fix them before moving on.
- **Cross-check with existing code** to ensure consistency in style, patterns, and architecture.
- **Validate that dependencies exist** before using them in code.
- **Read files before editing them** to understand context and ensure accurate replacements.

### Multi-Model Optimization
- Structure responses to be clear and actionable for both GitHub Copilot and Claude.
- Use explicit, unambiguous language in all instructions and code comments.
- Provide step-by-step reasoning when solving complex problems.
- Use concrete examples when explaining concepts or patterns.
- Be explicit about assumptions and constraints.

## General Coding Standards
- Follow .editorconfig and Roslyn Analyzer rules for code style and quality.
- Use standard .NET naming conventions for classes, methods, variables, and namespaces.
- Place a blank line after any logical set of statements for readability.
- Do not use braces around single statements.
- Prefer `var` for local variables when the type is obvious from the right side.
- Use `string.Empty` instead of `""` for empty strings.
- Use nullable reference types and handle null cases appropriately.
- Prefer `record` types for DTOs and immutable data structures.
- Use pattern matching and switch expressions where appropriate.
- Prefer collection expressions `[]` and spread operators `..` in .NET 10.

## Design Patterns & Architecture
- Prefer Repository and Factory patterns where applicable.
- Organize abstractions, enums, models, and services in dedicated folders.
- Services with multiple files should have their own folder.
- Follow SOLID principles in all design decisions.
- Use interfaces for abstraction and testability.
- Implement the Strategy pattern for interchangeable algorithms.
- Use the Builder pattern for complex object construction.
- Apply the Mediator pattern (e.g., MediatR) for request/response handling in larger applications.

## Dependency Management & Configuration
- Use Dependency Injection for all services and repositories.
- Do not hardcode configuration; use appsettings.json or similar JSON files and IOptions<T> or IConfiguration.
- Prefer NuGet packages for common functionality unless a custom implementation is required.
- Register services with appropriate lifetimes (Singleton, Scoped, Transient).
- Use keyed services in .NET 10 when multiple implementations of the same interface exist.
- Validate configuration on startup using IValidateOptions<T>.

## Logging
- Use Serilog for logging across all projects.
- Use structured logging with named parameters: `_logger.LogInformation("Processing {OrderId} for {CustomerId}", orderId, customerId)`.
- Log at appropriate levels: Trace, Debug, Information, Warning, Error, Critical.
- Do not log sensitive information (passwords, tokens, PII) without redaction.
- Use log scopes to add context: `using (_logger.BeginScope("OrderId: {OrderId}", orderId))`.

## Documentation & Comments
- Use /// XML documentation comments for all public members to clarify intent.
- Add inline comments for complex logic and to clarify intent.
- Document "why" not "what" - the code should be self-explanatory for the "what".
- Include examples in XML documentation for complex APIs.
- Update comments when code changes to prevent misleading documentation.

## Error Handling & Exceptions
- Validate all inputs for public APIs and return standardized error responses.
- Use standard exceptions where possible; use custom exceptions for specific modules.
- Include meaningful error messages that help diagnose the issue.
- Use Result<T> or similar patterns for expected errors rather than exceptions.
- Log exceptions with full stack traces and context.
- Never swallow exceptions silently - at minimum, log them.
- Use exception filters when retrying or handling specific conditions.

## Security & Input Validation
- Validate all inputs for security and correctness.
- Follow standard security practices for authentication and authorization.
- Use parameterized queries to prevent SQL injection.
- Sanitize user input before using in dynamic queries or HTML output.
- Use HTTPS for all external communications.
- Store secrets in secure locations (Azure Key Vault, user secrets, environment variables).
- Implement proper CORS policies for APIs.
- Use [Authorize] attributes and policy-based authorization.
- Validate JWT tokens and check claims appropriately.

## Frontend (Blazor)
- Use CSS isolation for components where required.
- Use shared styles and Bootstrap for CSS.
- Use MudBlazor as the component library.
- Avoid inline styling.
- Use `@rendermode` directives appropriately (InteractiveServer, InteractiveWebAssembly, InteractiveAuto).
- Implement proper form validation using DataAnnotations and EditForm.
- Use cascading parameters for shared state when appropriate.
- Dispose of components properly to prevent memory leaks.
- Use `@key` directive to help with component identity and re-rendering.
- Implement loading states and error boundaries for better UX.

## Testing (Unit & Integration)

### General Testing Practices
- Use xUnit for all unit and integration tests.
- Use Moq for mocking dependencies.
- Organize tests to mirror the structure of the source code.
- Ensure complete code coverage, including edge cases and exceptions.
- Follow the Arrange-Act-Assert pattern in tests, with relevant comments and blank lines after each section.
- Use [Theory] and [InlineData] where appropriate.
- All new code must be covered by tests before merging.
- Run integration tests in CI/CD pipelines.

### Test Naming & Organization
- Name tests descriptively: `MethodName_Scenario_ExpectedBehavior`.
- Group related tests in nested classes using `[Collection]` attributes.
- Use test fixtures for shared setup and teardown.

### Mocking & Assertions
- Mock external dependencies but not the system under test.
- Use Verify() on mocks to ensure methods were called correctly.
- Use FluentAssertions for more readable assertions.
- Test both happy path and error scenarios.

### Integration Testing
- Use WebApplicationFactory for API integration tests.
- Use in-memory databases or test containers for data layer tests.
- Clean up test data after each test to prevent interference.
- Mock external services (email, SMS, payment gateways) in integration tests.

## Performance & Best Practices
- Use async/await for I/O-bound operations.
- Avoid blocking calls (e.g., .Result, .Wait()) on async methods.
- Use ValueTask<T> for high-performance scenarios with frequent synchronous completion.
- Use Span<T> and Memory<T> for high-performance buffer operations.
- Implement pagination for large data sets.
- Use caching appropriately (IMemoryCache, IDistributedCache).
- Profile and benchmark critical paths before optimizing.
- Use `ConfigureAwait(false)` in library code to avoid context capture.

## Data Access
- Use Entity Framework Core for ORM.
- Use migrations for database schema changes.
- Implement the repository pattern for data access abstraction.
- Use specification pattern for complex queries.
- Use AsNoTracking() for read-only queries.
- Batch operations when possible to reduce round trips.
- Use compiled queries for frequently executed queries.
- Index database columns used in WHERE, JOIN, and ORDER BY clauses.

## Global State
- Avoid global state unless absolutely necessary.
- Use dependency injection to manage singleton state when needed.
- Use thread-safe collections for shared state accessed concurrently.

## API Design
- Follow REST principles for API design.
- Use appropriate HTTP verbs (GET, POST, PUT, PATCH, DELETE).
- Return appropriate status codes (200, 201, 204, 400, 401, 403, 404, 500).
- Use versioning for APIs to manage breaking changes.
- Implement rate limiting and throttling for public APIs.
- Use DTOs for API contracts, separate from domain models.
- Document APIs using Swagger/OpenAPI.

## Copilot Output Review
- All Copilot-generated code must be reviewed and modified before committing or merging.
- If code does not follow these instructions, regenerate with more context or split the task.
- Verify that generated code compiles and passes all tests.
- Check for security vulnerabilities in generated code.
- Ensure generated code follows the existing codebase patterns and conventions.

## Branching & Workflow
- Use main, stable, and dev branches for development and releases.
- Ensure all code and tests meet these standards before merging to main or stable.
- Use feature branches for new development.
- Require pull request reviews before merging to main branches.
- Run CI/CD pipelines on all pull requests.
- Use conventional commits for clear history (feat:, fix:, docs:, refactor:, test:, chore:).

## When You Don't Know
If you encounter any of the following situations, STOP and ASK the user:
- Uncertain about the intended behavior or business logic
- Missing information about configuration, connection strings, or external dependencies
- Unclear which approach or pattern the user prefers
- Need to make assumptions about requirements or constraints
- Don't understand the domain-specific terminology or context
- Uncertain about the existing architecture or design patterns in use
- Need clarification on the scope of changes requested

Remember: It's better to ask for clarification than to implement the wrong solution or make incorrect assumptions.
