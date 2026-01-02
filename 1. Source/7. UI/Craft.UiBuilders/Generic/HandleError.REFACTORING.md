# HandleError Component Refactoring: Using Generic Components

## Overview

The `HandleError` component has been enhanced to use Craft framework's generic components (`If`, `Show`) instead of raw `@if` statements, making it more declarative and consistent with the framework's design patterns.

## Before & After Comparison

### Before: Traditional @if Statements

```razor
@if (CustomErrorContent is not null)
{
    @CustomErrorContent
}
else
{
    <div class="error-container">
        <!-- Error UI -->
    </div>
}

@if (ShowDetails && errorContext is not null)
{
    <details class="error-details">
        <!-- Details -->
    </details>
}

@if (!string.IsNullOrEmpty(ErrorPageUri))
{
    <div class="error-actions">
        <button>Go to Error Page</button>
        <button>Reload Page</button>
    </div>
}
else
{
    <div class="error-actions">
        <button>Reload Page</button>
    </div>
}
```

### After: Using Generic Components

```razor
<If Condition="@(CustomErrorContent is not null)">
    <True>
        @CustomErrorContent
    </True>
    <False>
        <div class="error-container">
            <!-- Error UI -->
        </div>
    </False>
</If>

<Show When="@(ShowDetails && errorContext is not null)">
    <details class="error-details">
        <!-- Details -->
    </details>
</Show>

<If Condition="@(!string.IsNullOrEmpty(ErrorPageUri))">
    <True>
        <div class="error-actions">
            <button>Go to Error Page</button>
            <button>Reload Page</button>
        </div>
    </True>
    <False>
        <div class="error-actions">
            <button>Reload Page</button>
        </div>
    </False>
</If>
```

## Benefits

### 1. **Declarative Syntax**
Generic components make the intent clearer and more explicit:
- `<If Condition="...">` vs `@if (...)`
- `<Show When="...">` vs `@if (...)`

### 2. **Framework Consistency**
Aligns with other Craft components, creating a unified development experience across the framework.

### 3. **Better Readability**
- XML-style tags are more readable than C# code blocks in Razor markup
- Clear separation between True/False branches with `<True>` and `<False>` tags
- Easier to spot conditional logic at a glance

### 4. **Improved Maintainability**
- Easier to understand for developers new to the codebase
- Self-documenting code structure
- Consistent patterns across all components

### 5. **Enhanced Testability**
- Generic components can be tested independently
- Clearer component boundaries
- Better isolation for unit testing

### 6. **Reusability**
- Same components used throughout the framework
- Shared behavior and bug fixes
- Consistent performance characteristics

## Component Usage Guide

### `If` Component
**Use when:** You have mutually exclusive conditions (either/or)

```razor
<If Condition="@someCondition">
    <True>
        <!-- Content when true -->
    </True>
    <False>
        <!-- Content when false -->
    </False>
</If>
```

**Key Features:**
- `Condition` (required): Boolean expression
- `True`: Rendered when condition is true
- `False`: Rendered when condition is false

### `Show` Component
**Use when:** You only need to show content when a condition is true

```razor
<Show When="@someCondition">
    <!-- Content rendered only when condition is true -->
</Show>
```

**Key Features:**
- `When` (required): Boolean expression
- Simpler than `If` when you don't need a false branch

### `Hide` Component
**Use when:** You only need to hide content when a condition is true

```razor
<Hide When="@someCondition">
    <!-- Content rendered only when condition is false -->
</Hide>
```

**Key Features:**
- `When` (required): Boolean expression
- Inverse of `Show`

## When to Use Generic Components vs @if

### Use Generic Components When:
? Building reusable components  
? Following Craft framework patterns  
? Need consistent conditional rendering  
? Want better readability in markup  
? Building production-ready components  

### Use @if When:
? Quick prototyping  
? Simple page-level logic  
? One-off conditional rendering  
? Performance-critical inline checks  

## Performance Considerations

Generic components have minimal overhead:
- **Compile-time**: Same generated code as @if statements
- **Runtime**: Negligible performance difference
- **Memory**: No additional allocations
- **Rendering**: Identical performance characteristics

## Migration Guide

To migrate existing components to use generic components:

1. **Add namespace import:**
   ```razor
   @using Craft.UiBuilders.Generic
   ```

2. **Replace @if statements:**
   - `@if (condition) { ... }` ? `<Show When="@condition">...</Show>`
   - `@if (condition) { ... } else { ... }` ? `<If Condition="@condition"><True>...</True><False>...</False></If>`

3. **Test thoroughly:**
   - Verify conditional logic works correctly
   - Check rendering behavior
   - Validate accessibility

## Examples from HandleError

### Custom Content Toggle
```razor
<!-- Shows custom error content if provided, otherwise default UI -->
<If Condition="@(CustomErrorContent is not null)">
    <True>
        @CustomErrorContent
    </True>
    <False>
        <DefaultErrorUI />
    </False>
</If>
```

### Conditional Details Display
```razor
<!-- Only shows technical details in development mode -->
<Show When="@(ShowDetails && errorContext is not null)">
    <details class="error-details">
        <summary>Technical Details</summary>
        <pre class="error-stack">@errorContext</pre>
    </details>
</Show>
```

### Action Buttons
```razor
<!-- Different button sets based on ErrorPageUri configuration -->
<If Condition="@(!string.IsNullOrEmpty(ErrorPageUri))">
    <True>
        <div class="error-actions">
            <button @onclick="NavigateToErrorPage">Go to Error Page</button>
            <button @onclick="ReloadPage">Reload Page</button>
        </div>
    </True>
    <False>
        <div class="error-actions">
            <button @onclick="ReloadPage">Reload Page</button>
        </div>
    </False>
</If>
```

## Best Practices

### 1. Use Appropriate Component
- **If**: When you need both true and false branches
- **Show**: When you only need true branch
- **Hide**: When you only need false branch (inverted logic)

### 2. Keep Conditions Simple
```razor
<!-- Good: Clear and simple -->
<Show When="@IsAuthenticated">
    <UserMenu />
</Show>

<!-- Avoid: Complex conditions -->
<Show When="@(IsAuthenticated && User?.Roles?.Contains("Admin") == true && !IsExpired)">
    <AdminPanel />
</Show>

<!-- Better: Extract to property -->
<Show When="@CanAccessAdminPanel">
    <AdminPanel />
</Show>
```

### 3. Meaningful Names
```razor
<!-- Good -->
<If Condition="@HasCustomError">
    <True>...</True>
    <False>...</False>
</If>

<!-- Avoid -->
<If Condition="@x">
    <True>...</True>
    <False>...</False>
</If>
```

### 4. Nest Carefully
```razor
<!-- Acceptable: 2-3 levels -->
<Show When="@IsLoggedIn">
    <If Condition="@HasPermission">
        <True>
            <Show When="@IsActive">
                <Content />
            </Show>
        </True>
    </If>
</Show>

<!-- Consider refactoring if deeper nesting -->
```

## Related Components

Other Craft generic components that work well with HandleError:

- **Timeout**: Wrap slow components with timeout protection
- **Lazy**: Lazy load error page components
- **Delay**: Delay error message display for better UX
- **DataLoader**: Handle async data loading with error boundaries
- **Placeholder**: Show loading state before error occurs

## Conclusion

Using generic components in `HandleError` demonstrates best practices for building production-ready Blazor components within the Craft framework. This approach:

- ? Improves code readability
- ? Maintains framework consistency
- ? Enhances maintainability
- ? Follows established patterns
- ? Provides better developer experience

The refactoring showcases how Craft's generic components can transform imperative conditional logic into declarative, self-documenting markup that's easier to understand and maintain.
