# Generic UI Components - Implementation Summary

This document provides an overview of the 11 new generic UI components added to `Craft.UiBuilders.Generic` namespace.

## Components Implemented

### 1. **Repeat<TItem>** Component
**Files:** `Repeat.razor`, `Repeat.razor.cs`

A simpler alternative to `ForEach<T>` for rendering collections without index tracking or separators.

**Usage:**
```razor
<Repeat Items="@users" Context="user">
    <ItemTemplate>
        <div>@user.Name</div>
    </ItemTemplate>
    <EmptyTemplate>
        <p>No users found</p>
    </EmptyTemplate>
</Repeat>
```

**Key Features:**
- Generic type support
- Item template rendering
- Empty state handling
- Clean, minimal API

---

### 2. **Switch<TValue>** Component
**Files:** `Switch.razor`, `Switch.razor.cs`

Multi-case conditional rendering component, similar to C# switch statements.

**Usage:**
```razor
<Switch Value="@status">
    <Case When="Status.Active">Active content</Case>
    <Case When="Status.Inactive">Inactive content</Case>
    <Default>Unknown status</Default>
</Switch>
```

**Key Features:**
- Type-safe value matching
- Multiple case support
- Default fallback content
- Cascading parameter pattern for case registration

---

### 3. **Delay** Component
**Files:** `Delay.razor`, `Delay.razor.cs`

Delays rendering of content for a specified duration, useful for progressive loading.

**Usage:**
```razor
<Delay Milliseconds="500">
    <div>Content shown after 500ms</div>
</Delay>
```

**Key Features:**
- Configurable delay duration
- Timer-based implementation
- Proper disposal handling
- Progressive loading pattern support

---

### 4. **Placeholder** Component
**Files:** `Placeholder.razor`, `Placeholder.razor.cs`

Displays skeleton/placeholder content during loading states.

**Usage:**
```razor
<Placeholder Loading="@isLoading" Lines="3" Variant="PlaceholderVariant.Text">
    <Content>@actualContent</Content>
</Placeholder>
```

**Key Features:**
- Multiple variant types (Text, Circle, Rectangle)
- Configurable number of lines
- Loading state toggle
- CSS-based placeholder styling

**Variants:**
- `PlaceholderVariant.Text` - Text line placeholders
- `PlaceholderVariant.Circle` - Circular placeholders for avatars
- `PlaceholderVariant.Rectangle` - Rectangular placeholders for images/cards

---

### 5. **Toggle** Component
**Files:** `Toggle.razor`, `Toggle.razor.cs`

Switches between two content states based on a boolean flag.

**Usage:**
```razor
<Toggle IsActive="@expanded">
    <Active>Expanded content</Active>
    <Inactive>Collapsed content</Inactive>
</Toggle>
```

**Key Features:**
- Simple boolean state switching
- Two content templates
- Cleaner than If/Else for binary states

---

### 6. **ErrorBoundary** Component
**Files:** `ErrorBoundary.razor`, `ErrorBoundary.razor.cs`

Enhanced error boundary with retry support and custom error display.

**Usage:**
```razor
<ErrorBoundary OnError="HandleError" ShowDetails="true" AutoRetry="true">
    <ChildContent>
        <RiskyComponent />
    </ChildContent>
    <ErrorContent Context="error">
        <div class="error">
            <h3>@error.Message</h3>
            <pre>@error.StackTrace</pre>
        </div>
    </ErrorContent>
</ErrorBoundary>
```

**Key Features:**
- Exception catching from child components
- Custom error UI via `ErrorContent` template
- Auto-retry capability with configurable delay
- Show/hide detailed error information
- Manual recovery via `Recover()` method
- `ErrorContext` record with exception details

---

### 7. **Lazy** Component
**Files:** `Lazy.razor`, `Lazy.razor.cs`

Lazy loads content when it becomes visible in the viewport using Intersection Observer API.

**Usage:**
```razor
<Lazy Threshold="0.1" RootMargin="50px">
    <HeavyComponent />
</Lazy>
```

**Key Features:**
- Intersection Observer API integration
- Configurable threshold (0.0 to 1.0)
- Configurable root margin
- Automatic cleanup on disconnect
- Fallback to immediate rendering if JS fails
- Performance optimization for off-screen content

---

### 8. **Debounce<TValue>** Component
**Files:** `Debounce.razor`, `Debounce.razor.cs`

Debounces rendering of child content based on a trigger value.

**Usage:**
```razor
<Debounce Value="@searchText" DelayMs="300" OnDebounced="HandleSearch">
    <SearchResults Query="@searchText" />
</Debounce>
```

**Key Features:**
- Generic value type support
- Configurable delay duration
- Value change detection
- Optional `OnDebounced` callback
- Timer-based debouncing
- Useful for search inputs and live filtering

---

### 9. **Empty<TItem>** Component
**Files:** `Empty.razor`, `Empty.razor.cs`

Displays content only when a collection is empty or null.

**Usage:**
```razor
<Empty Items="@products">
    <div>No products available</div>
</Empty>
```

**Key Features:**
- Generic collection type support
- Null and empty collection handling
- Complementary to `Repeat<TItem>`
- Simple empty state display

---

### 10. **Timeout** Component
**Files:** `Timeout.razor`, `Timeout.razor.cs`

Renders content and automatically removes it after a specified timeout.

**Usage:**
```razor
<Timeout DurationMs="5000" OnExpired="HandleExpired">
    <div class="notification">This will disappear after 5 seconds</div>
</Timeout>
```

**Key Features:**
- Configurable duration
- Automatic content removal
- Optional expiration callback
- Timer-based implementation
- Perfect for temporary notifications

---

### 11. **Fragment** Component
**Files:** `Fragment.razor`, `Fragment.razor.cs`

Renders child content without adding wrapper elements to the DOM.

**Usage:**
```razor
<Fragment>
    <p>First paragraph</p>
    <p>Second paragraph</p>
</Fragment>
```

**Key Features:**
- No wrapper element in DOM
- Similar to React.Fragment
- Useful for grouping without extra HTML
- Simplest possible implementation

---

## Design Patterns Used

### 1. **Consistent Inheritance**
All components (except Fragment) inherit from `CraftComponent` for consistent behavior:
- Common parameters (Visible, Disabled, Class, etc.)
- Theme support
- Logging capabilities
- Proper disposal handling

### 2. **Generic Type Support**
Components like `Repeat<TItem>`, `Switch<TValue>`, `Debounce<TValue>`, and `Empty<TItem>` use generics for type safety.

### 3. **RenderFragment Parameters**
Follows Blazor conventions with:
- `ChildContent` for main content
- Named templates (e.g., `ItemTemplate`, `EmptyTemplate`, `Active`, `Inactive`)
- Context parameters for passing data to templates

### 4. **Async Disposal**
Components with timers and resources implement proper async disposal:
- `Delay`, `Debounce`, `Timeout` - Timer disposal
- `Lazy` - JS module and DotNetObjectReference disposal
- `ErrorBoundary` - Event handler cleanup

### 5. **JavaScript Interop**
`Lazy` component uses modern JS features:
- Intersection Observer API
- Inline module import via data URL
- DotNetObjectReference for callbacks

### 6. **Timer Pattern**
Multiple components use `System.Threading.Timer` for delayed operations:
- Single-shot timers with `System.Threading.Timeout.Infinite`
- Proper disposal in `DisposeAsyncCore()`
- InvokeAsync for thread-safe state updates

## Component Categories

### **Conditional Rendering**
- `Show` (existing)
- `Hide` (existing)
- `If` (existing)
- **`Toggle`** (new)
- **`Switch<TValue>`** (new)

### **Collection Rendering**
- `ForEach<T>` (existing)
- **`Repeat<TItem>`** (new)
- **`Empty<TItem>`** (new)

### **Loading States**
- `DataLoader` (existing)
- `Spinner` (existing)
- **`Placeholder`** (new)
- **`Delay`** (new)
- **`Lazy`** (new)

### **Performance Optimization**
- **`Lazy`** (new)
- **`Debounce<TValue>`** (new)

### **Error Handling**
- **`ErrorBoundary`** (new)

### **Timing & Lifecycle**
- **`Delay`** (new)
- **`Timeout`** (new)
- **`Debounce<TValue>`** (new)

### **Layout Utilities**
- **`Fragment`** (new)

## CSS Classes Added

The components use the following CSS class conventions:

```css
/* Placeholder component */
.craft-placeholder { }
.craft-placeholder-line { }
.craft-placeholder-text { }
.craft-placeholder-circle { }
.craft-placeholder-rectangle { }
```

**Note:** Actual CSS styling should be added to your theme/stylesheet.

## Testing Recommendations

### Unit Tests
1. **Repeat<TItem>** - Test empty collections, null items, template rendering
2. **Switch<TValue>** - Test case matching, default fallback, type safety
3. **Delay** - Test timer initialization and disposal
4. **Placeholder** - Test variant rendering, loading state toggle
5. **Toggle** - Test state switching
6. **ErrorBoundary** - Test exception catching, retry logic
7. **Lazy** - Test intersection observer initialization (with JS mock)
8. **Debounce<TValue>** - Test value change debouncing, callback invocation
9. **Empty<TItem>** - Test null and empty collection handling
10. **Timeout** - Test expiration timing, callback invocation
11. **Fragment** - Test passthrough rendering

### Integration Tests
- Test component composition
- Test cascading parameters (Switch/Case)
- Test JS interop in Lazy component
- Test timer behavior under different scenarios

## Future Enhancements

### Potential Improvements
1. **Portal Component** - Render content at different DOM locations
2. **Accordion/Collapse** - Animated expand/collapse wrapper
3. **Transition** - CSS transition wrapper
4. **AnimatePresence** - Animate components on mount/unmount
5. **Virtualize Enhancement** - Extended virtualization support
6. **InfiniteScroll** - Auto-load more items on scroll
7. **Measure** - Component size measurement utility

### Placeholder Styling
Add CSS animations for loading states:
```css
@keyframes placeholder-shimmer {
    0% { background-position: -468px 0; }
    100% { background-position: 468px 0; }
}

.craft-placeholder-line {
    animation: placeholder-shimmer 1.5s infinite;
    background: linear-gradient(to right, #f0f0f0 8%, #f8f8f8 18%, #f0f0f0 33%);
    background-size: 800px 104px;
}
```

## Migration Notes

### From ForEach to Repeat
```razor
<!-- Before -->
<ForEach Collection="@items" Context="item">
    <ItemContent>
        <div>@item.Item.Name</div>
    </ItemContent>
</ForEach>

<!-- After -->
<Repeat Items="@items" Context="item">
    <ItemTemplate>
        <div>@item.Name</div>
    </ItemTemplate>
</Repeat>
```

### From If to Toggle
```razor
<!-- Before -->
<If Condition="@isExpanded">
    <True>Expanded content</True>
    <False>Collapsed content</False>
</If>

<!-- After -->
<Toggle IsActive="@isExpanded">
    <Active>Expanded content</Active>
    <Inactive>Collapsed content</Inactive>
</Toggle>
```

## Build Status
? All components successfully compiled
? No build errors or warnings
? Ready for testing and integration

## Conclusion

All 11 requested generic components have been successfully implemented following the established patterns in the Craft framework. Each component:
- Follows Blazor best practices
- Integrates with the existing `CraftComponent` base class
- Provides clear, documented APIs
- Handles disposal properly
- Uses modern C# 14.0 and .NET 10 features

The components are ready for use in Blazor applications and complement the existing component library perfectly.
