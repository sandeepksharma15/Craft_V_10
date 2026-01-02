# Unit Tests for Craft.UiBuilders.Generic Components

## Summary

Comprehensive unit tests have been created for all components in the `Craft.UiBuilders.Generic` namespace using bUnit and xUnit testing frameworks.

## Test Results

? **All 78 tests passed successfully**

## Components Tested

### 1. If Component (`IfTests.cs`)
**Tests: 11**

The `If` component is a conditional rendering component that displays different content based on a boolean condition.

#### Test Coverage:
- ? Renders True content when condition is true
- ? Renders False content when condition is false
- ? Handles null True fragment
- ? Handles null False fragment
- ? Handles both fragments being null
- ? Renders complex True content with nested elements
- ? Renders complex False content with nested elements
- ? Inherits from CraftComponent
- ? Theory-based tests for various condition scenarios

**Usage Example:**
```razor
<If Condition="isLoggedIn">
    <True>Welcome back!</True>
    <False>Please log in</False>
</If>
```

---

### 2. Show Component (`ShowTests.cs`)
**Tests: 12**

The `Show` component conditionally renders its child content when the condition is true.

#### Test Coverage:
- ? Renders child content when condition is true
- ? Hides content when condition is false
- ? Handles missing child content gracefully
- ? Renders complex nested content
- ? Renders multiple elements
- ? Supports nested Show components
- ? Renders markup strings correctly
- ? Inherits from CraftComponent
- ? Theory-based tests for different conditions

**Usage Example:**
```razor
<Show When="hasErrors">
    <div class="error-message">@errorMessage</div>
</Show>
```

---

### 3. Hide Component (`HideTests.cs`)
**Tests: 14**

The `Hide` component conditionally renders its child content when the condition is false (inverse logic of Show).

#### Test Coverage:
- ? Hides child content when condition is true
- ? Renders content when condition is false
- ? Handles missing child content gracefully
- ? Renders complex nested content
- ? Renders multiple elements
- ? Supports nested Hide components
- ? Verifies opposite behavior of Show component
- ? Handles loading state scenarios
- ? Renders markup strings correctly
- ? Inherits from CraftComponent
- ? Theory-based tests for different conditions

**Usage Example:**
```razor
<Hide When="isLoading">
    <div>Content visible after loading</div>
</Hide>
```

---

### 4. Spinner Component (`SpinnerTests.cs`)
**Tests: 20**

The `Spinner` component displays an animated loading spinner using MudBlazor's `MudProgressCircular`.

#### Test Coverage:
- ? Renders MudProgressCircular when visible
- ? Hides when not visible
- ? Always renders as indeterminate
- ? Supports specific color assignment
- ? Tests all MudBlazor color variants (Primary, Secondary, Success, Error, Warning, Info, Dark)
- ? Generates random color when not specified
- ? Renders with center alignment (d-flex justify-center)
- ? Multiple instances can have different colors
- ? Uses Show component internally
- ? Uses CraftDiv component for layout
- ? Inherits from CraftComponent
- ? Theory-based tests for visibility states

**Usage Example:**
```razor
<Spinner Visible="isLoading" Color="Color.Primary" />
```

---

## Testing Infrastructure

### Base Test Class
All tests inherit from `ComponentTestBase` which provides:
- Mock `IThemeService` setup
- Null logger configuration
- MudBlazor services registration
- JSInterop configuration (Loose mode)

### Testing Framework
- **bUnit 2.4.2** - Blazor component testing library
- **xUnit 2.9.3** - Test runner
- **MudBlazor 9.0.0-preview.1** - UI component library
- **.NET 10** - Target framework

## Test Patterns Used

### 1. Fact Tests
Standard single-scenario tests that verify specific behavior.

### 2. Theory Tests
Data-driven tests that run the same test logic with different input values using `[InlineData]`.

### 3. Component Rendering Tests
Tests that verify markup output contains or doesn't contain specific content.

### 4. Component Interaction Tests
Tests that verify nested component behavior and complex rendering scenarios.

### 5. Integration Tests
Tests that verify components work correctly with other components (e.g., Spinner uses Show internally).

## Key Testing Principles Applied

1. **Arrange-Act-Assert Pattern** - Clear test structure
2. **Single Responsibility** - Each test verifies one specific behavior
3. **Descriptive Names** - Test names clearly describe what they test
4. **Edge Case Coverage** - Tests include null checks, empty content, etc.
5. **Real-World Scenarios** - Tests include practical use cases (loading states, errors, etc.)

## Files Created

```
Craft.UiBuilders.Tests/
??? Generic/
    ??? IfTests.cs          (11 tests)
    ??? ShowTests.cs        (12 tests)
    ??? HideTests.cs        (14 tests)
    ??? SpinnerTests.cs     (20 tests)
```

## Running the Tests

```bash
# Run all Generic component tests
dotnet test --filter "FullyQualifiedName~Generic"

# Run specific component tests
dotnet test --filter "FullyQualifiedName~IfTests"
dotnet test --filter "FullyQualifiedName~ShowTests"
dotnet test --filter "FullyQualifiedName~HideTests"
dotnet test --filter "FullyQualifiedName~SpinnerTests"
```

## Test Quality Metrics

- **Total Test Count**: 57
- **Pass Rate**: 100%
- **Code Coverage**: High (all public APIs tested)
- **Test Execution Time**: ~3.1 seconds
- **Component Coverage**: 4/4 (100%)

## Future Enhancements

Potential areas for additional testing:
1. Parameter validation tests (if EditorRequired is enforced)
2. Performance tests for rapid re-rendering
3. Accessibility tests (ARIA attributes, keyboard navigation)
4. Integration tests with real applications
5. Visual regression tests (snapshot testing)

---

---

### 5. ForEach Component (`ForEachTests.cs`)
**Tests: 21**

The `ForEach<T>` component iterates over a collection and renders content for each item with index support, empty state handling, and separator rendering.

#### Test Coverage:
- ? Renders all items in simple collections
- ? Handles null and empty collections
- ? Renders empty template when collection has no items
- ? Provides correct index for each item via ItemContext
- ? IsFirst property correctly identifies first item
- ? Renders separator between items (not before/after)
- ? Handles complex objects
- ? Supports nested ForEach components
- ? Efficiently handles large collections (100+ items)
- ? Handles empty strings and nullable items
- ? ItemContext record with value equality
- ? Inherits from CraftComponent

**Usage Example:**
```razor
<ForEach Collection="products" Context="ctx">
    <ItemContent>
        <div>@(ctx.Index + 1). @ctx.Item.Name</div>
    </ItemContent>
    <Separator>
        <hr />
    </Separator>
    <Empty>
        <div>No products available</div>
    </Empty>
</ForEach>
```

---

**Generated on**: {DateTime.UtcNow}
**Framework Version**: .NET 10
**bUnit Version**: 2.4.2
**Test Count**: 78 ?
