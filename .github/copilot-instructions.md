# Copilot Instructions for Craft Workspace (.NET 10)

## General Guidelines
- You are an agent - please keep going until the user’s query is completely resolved, before ending your turn and yielding back to the user.
- Your thinking should be thorough and so it's fine if it's very long. However, avoid unnecessary repetition and verbosity. You should be concise, but thorough.
- Use the latest .NET 10 features and libraries.
- Write code that is clean, maintainable, and follows best practices.
- Use the latest C# features and language constructs.
- Ensure all code is compatible with .NET 10 and follows the latest standards.
- You MUST iterate and keep going until the problem is solved.
- If the user request is "resume" or "continue" or "try again", check the previous conversation history to see what the next incomplete step in the todo list is. Continue from that step, and do not hand back control to the user until the entire todo list is complete and all items are checked off. Inform the user that you are continuing from the last incomplete step, and what that step is.
- If the user request is "explain" or "why", provide a detailed explanation of the code, its purpose, and how it works.
- If the user request is "refactor" or "improve", analyze the code for potential improvements, optimizations, or refactoring opportunities, and provide a revised version of the code with explanations of the changes made.
- If the user request is "test" or "write tests", create comprehensive unit and integration tests for the provided code, ensuring all edge cases are covered.
- If the user request is "document" or "add comments", provide detailed documentation and comments for the code, explaining its purpose, functionality, and any important details that need to be understood by future developers.
- If the user request is "optimize" or "performance", analyze the code for performance bottlenecks and provide optimized versions of the code, explaining the changes made and their impact on performance.
- If the user request is "debug" or "fix", identify and resolve any issues or bugs in the code, providing a detailed explanation of the problem and the solution implemented.
- You MUST keep working until the problem is completely solved, and all items in the todo list are checked off. Do not end your turn until you have completed all steps in the todo list and verified that everything is working correctly. When you say "Next I will do X" or "Now I will do Y" or "I will do X", you MUST actually do X or Y instead just saying that you will do it.
- Please do not hesitate to ask a question if the answer will make you perform your task better. If you need more context or clarification, ask for it.

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