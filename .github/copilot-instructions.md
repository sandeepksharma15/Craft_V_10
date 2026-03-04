# Copilot Instructions

> **Scope:** These instructions apply to all repos that include this file.
> **Target:** .NET 10 · C# latest · Blazor · EF Core
> **Memory:** Read `.github/copilot-memory.md` at the start of every session for accumulated project learnings.

---

## 1 · Core Behavior

### Clarify First, Then Implement

Before writing any code, **always**:

1. **State your understanding** of the request in 1–2 sentences.
2. **List assumptions** you are making (tech choices, scope boundaries, affected files).
3. **Ask clarifying questions** if anything is ambiguous — requirements, config values, business rules, domain terms, or scope.
4. **Recommend alternatives** when you see a better approach, pattern, or library — explain trade-offs briefly and let the user choose.
5. Only proceed to implementation after the user confirms or when the request is unambiguous.

> It is always better to ask one round of questions than to implement the wrong thing.

### Autonomous Execution

Once confirmed, work autonomously until the task is fully resolved:

- Verify the workspace (structure, existing code, symbols) before editing — never invent paths, namespaces, or APIs.
- Build and confirm zero errors before considering the task complete.
- Run relevant unit tests after changes.
- Never leave code in a broken state.

### Cross-Session Memory

- At session start, read `.github/copilot-memory.md` for prior learnings.
- When the user corrects you, states a preference, or a non-obvious decision is made, persist it to `.github/copilot-memory.md` under the appropriate section.
- When in doubt whether something is worth remembering, save it — false positives are cheap, forgotten lessons are expensive.

---

## 2 · Code Style

- **Follow `.editorconfig` and Roslyn analyzers** — they are the single source of truth for formatting, naming, and style. Do not duplicate or contradict them.
- Use `string.Empty` instead of `""`.
- Prefer `record` types for DTOs and immutable data.
- Prefer collection expressions `[]` and spread `..` syntax.
- Place a blank line after each logical block of statements.
- Document "why" in comments, not "what" — use `///` XML docs on all public members.

---

## 3 · Architecture & Patterns

- **Patterns:** Repository, Factory, Strategy, Builder. Use MediatR for request/response in larger flows.
- **Folder structure:** Dedicated folders for Abstractions, Enums, Models, Services. Services with multiple files get their own folder.
- **Configuration:** `IOptions<T>` / `IConfiguration` bound from `appsettings.json`. Validate on startup with `IValidateOptions<T>`. Never hardcode config.
- **DI:** Register everything via DI. Use keyed services when multiple implementations of one interface exist.
- **Errors:** Use `Result<T>` for expected failures. Reserve exceptions for unexpected conditions. Never swallow exceptions silently.

---

## 4 · Stack-Specific Rules

### Logging
- **Serilog** everywhere. Structured logging with named parameters:
  ```csharp
  _logger.LogInformation("Processing {OrderId} for {CustomerId}", orderId, customerId);
  ```

### Blazor (Frontend)
- **MudBlazor** as the component library. Bootstrap for shared styles.
- CSS isolation per component — no inline styles.
- Use `@rendermode`, `@key`, `EditForm` + `DataAnnotations`, cascading parameters, loading states, and error boundaries as appropriate.

### Data Access
- **EF Core** with migrations. Repository pattern + specification pattern for complex queries.

### Testing
- **xUnit** · **Moq** · **FluentAssertions**.
- Arrange-Act-Assert with section comments and blank lines.
- Name tests: `MethodName_Scenario_ExpectedBehavior`.
- `[Theory]` + `[InlineData]` for parameterized tests.
- `WebApplicationFactory` for integration tests.
- Mirror source folder structure in test projects.

---

## 5 · Workflow

- **Branches:** `main`, `stable`, `dev`. Feature branches for new work.
- **Commits:** Conventional format — `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`.
- **PR rules:** All code must compile, pass tests, and be reviewed before merging to `main` or `stable`.
