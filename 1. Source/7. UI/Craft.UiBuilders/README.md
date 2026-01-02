# Craft.UiBuilders

A comprehensive library of reusable Blazor components designed to simplify common UI patterns and enhance developer productivity. This library provides declarative, composable components for conditional rendering, data loading, collections, timing operations, and more.

---

## Table of Contents

- [Installation](#installation)
- [Generic Components](#generic-components)
  - [Conditional Rendering](#conditional-rendering)
  - [Data Loading & States](#data-loading--states)
  - [Collection Rendering](#collection-rendering)
  - [Timing & Performance](#timing--performance)
  - [Error Handling](#error-handling)
  - [Layout Helpers](#layout-helpers)
- [UI Components](#ui-components)
- [Best Practices](#best-practices)
- [Dependencies](#dependencies)

---

## Installation

Add a reference to the `Craft.UiBuilders` project or package in your Blazor application.

```xml
<PackageReference Include="Craft.UiBuilders" Version="1.0.0" />
```

Add the namespace to your `_Imports.razor`:

```razor
@using Craft.UiBuilders.Generic
@using Craft.UiBuilders.Components
```

---

## Generic Components

### Conditional Rendering

#### **If**

Conditionally renders different content based on a boolean condition.

**Purpose:**
- Provides cleaner, more declarative syntax than `@if` statements
- Improves readability for true/false branching
- Makes conditional logic explicit in markup

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Condition` | `bool` | Yes | - | The boolean condition |
| `True` | `RenderFragment?` | No | `null` | Content when true |
| `False` | `RenderFragment?` | No | `null` | Content when false |

**Usage:**

```razor
<If Condition="@isUserAuthenticated">
    <True>
        <MudText>Welcome back, @userName!</MudText>
        <MudButton>View Dashboard</MudButton>
    </True>
    <False>
        <MudText>Please log in to continue</MudText>
        <MudButton OnClick="@Login">Login</MudButton>
    </False>
</If>
```

---

#### **Show**

Conditionally renders content only when the condition is true.

**Purpose:**
- Simplest way to show/hide content
- Cleaner than `If` when you don't need an else case
- Perfect for optional UI elements

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `When` | `bool` | Yes | - | Show content when true |
| `ChildContent` | `RenderFragment?` | No | - | Content to display |

**Usage:**

```razor
<Show When="@isAdmin">
    <MudButton Color="Color.Error">Delete All Users</MudButton>
</Show>

<Show When="@hasErrors">
    <MudAlert Severity="Severity.Error">@errorMessage</MudAlert>
</Show>
```

---

#### **Hide**

Conditionally renders content only when the condition is false (inverse of Show).

**Purpose:**
- Semantic opposite of `Show`
- Useful when the logic reads better as "hide when X"
- Makes intent explicit in code

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `When` | `bool` | Yes | - | Hide content when true |
| `ChildContent` | `RenderFragment?` | No | - | Content to display |

**Usage:**

```razor
<Hide When="@isLoading">
    <div>Your actual content here</div>
</Hide>

<Hide When="@isOffline">
    <MudButton OnClick="@SaveData">Save</MudButton>
</Hide>
```

---

#### **Toggle**

Switches between two content states based on a boolean flag.

**Purpose:**
- More semantic than `If` for toggling between two known states
- Perfect for toggle-based UI patterns
- Explicitly shows both states in markup

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `IsActive` | `bool` | No | `false` | Toggle state |
| `Active` | `RenderFragment?` | No | `null` | Content when active |
| `Inactive` | `RenderFragment?` | No | `null` | Content when inactive |

**Usage:**

```razor
<Toggle IsActive="@isExpanded">
    <Active>
        <MudIcon Icon="@Icons.Material.Filled.ExpandLess" />
        <MudText>Click to collapse</MudText>
    </Active>
    <Inactive>
        <MudIcon Icon="@Icons.Material.Filled.ExpandMore" />
        <MudText>Click to expand</MudText>
    </Inactive>
</Toggle>
```

---

#### **Switch**

Renders different content based on matching a value against multiple cases (like a switch statement).

**Purpose:**
- Declarative alternative to multiple if-else statements
- Type-safe pattern matching
- Clean syntax for multi-way branching

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Value` | `TValue?` | Yes | - | Value to match |
| `Cases` | `List<Case<TValue>>` | Yes | `[]` | Collection of cases |
| `Default` | `RenderFragment?` | No | `null` | Default content |

**Usage:**

```razor
<Switch Value="@userRole">
    <Case When="UserRole.Admin">
        <AdminDashboard />
    </Case>
    <Case When="UserRole.Manager">
        <ManagerDashboard />
    </Case>
    <Case When="UserRole.User">
        <UserDashboard />
    </Case>
    <Default>
        <MudText>Unknown role</MudText>
    </Default>
</Switch>
```

---

### Data Loading & States

#### **DataLoader**

Manages async data loading states with loading, error, and success states.

**Purpose:**
- Simplifies handling of async operations
- Provides consistent loading and error UX
- Reduces boilerplate for data fetching
- Built-in retry functionality

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `IsLoading` | `bool` | Yes | - | Loading state flag |
| `HasError` | `bool` | No | `false` | Error state flag |
| `Loading` | `RenderFragment?` | No | `null` | Custom loading template |
| `Error` | `RenderFragment?` | No | `null` | Custom error template |
| `LoadingColor` | `Color` | No | `Color.Primary` | Spinner color |
| `ErrorTitle` | `string` | No | `"Error"` | Error title text |
| `ErrorMessage` | `string` | No | `"Something went wrong..."` | Error message |
| `RetryText` | `string` | No | `"Retry"` | Retry button text |
| `OnRetry` | `EventCallback` | No | - | Retry callback |
| `ChildContent` | `RenderFragment?` | No | - | Success content |

**Usage:**

```razor
<DataLoader IsLoading="@isLoading" 
            HasError="@hasError" 
            OnRetry="@LoadDataAsync">
    <div>
        <MudText Typo="Typo.h4">@data.Title</MudText>
        <MudText>@data.Description</MudText>
    </div>
</DataLoader>

@code {
    private bool isLoading = true;
    private bool hasError = false;
    private MyData data;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        isLoading = true;
        hasError = false;
        
        try
        {
            data = await DataService.GetDataAsync();
        }
        catch (Exception ex)
        {
            hasError = true;
            Logger.LogError(ex, "Failed to load data");
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

**Custom Templates:**

```razor
<DataLoader IsLoading="@isLoading" HasError="@hasError">
    <Loading>
        <MudProgressLinear Indeterminate="true" />
        <MudText Align="Align.Center" Class="mt-4">
            Loading your data...
        </MudText>
    </Loading>
    <Error>
        <MudAlert Severity="Severity.Warning" Variant="Variant.Filled">
            <MudText Typo="Typo.h6">Oh no!</MudText>
            <MudText>We couldn't load your data.</MudText>
            <MudButton OnClick="@LoadDataAsync" Class="mt-2">
                Try Again
            </MudButton>
        </MudAlert>
    </Error>
    <div>Your data: @data</div>
</DataLoader>
```

---

#### **Placeholder**

Displays skeleton/placeholder content during loading.

**Purpose:**
- Better UX than blank screens
- Indicates content structure while loading
- Reduces perceived loading time

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Loading` | `bool` | No | `false` | Loading state |
| `Lines` | `int` | No | `3` | Number of lines |
| `Variant` | `PlaceholderVariant` | No | `Text` | Visual variant |
| `Content` | `RenderFragment?` | No | `null` | Actual content |

**Usage:**

```razor
<Placeholder Loading="@isLoading" Lines="5">
    <Content>
        <MudCard>
            <MudCardContent>
                <MudText Typo="Typo.h5">@article.Title</MudText>
                <MudText>@article.Body</MudText>
            </MudCardContent>
        </MudCard>
    </Content>
</Placeholder>
```

---

#### **Spinner**

A simple loading spinner with random color support.

**Purpose:**
- Quick and easy loading indicator
- Minimal configuration needed
- Visual variety with random colors

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Color` | `Color` | No | Random | Spinner color |
| `Visible` | `bool` | No | `true` | Visibility |

**Usage:**

```razor
<Spinner Color="Color.Primary" />
<Spinner Visible="@isProcessing" />
```

---

### Collection Rendering

#### **ForEach**

Iterates over a collection with support for item context, empty state, and separators.

**Purpose:**
- Declarative collection rendering
- Access to item and index
- Built-in empty state handling
- Separator support between items

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Collection` | `IEnumerable<T>?` | Yes | - | Items to iterate |
| `ItemContent` | `RenderFragment<ItemContext>?` | No | `null` | Item template |
| `Empty` | `RenderFragment?` | No | `null` | Empty state template |
| `Separator` | `RenderFragment?` | No | `null` | Separator between items |

**ItemContext Properties:**
- `Item` - The current item
- `Index` - Zero-based index
- `IsFirst` - True if first item

**Usage:**

```razor
<ForEach Collection="@users">
    <ItemContent Context="ctx">
        <MudCard Class="mb-2">
            <MudCardContent>
                <MudText>@ctx.Index. @ctx.Item.Name</MudText>
                <Show When="@ctx.IsFirst">
                    <MudChip Size="Size.Small" Color="Color.Primary">
                        First User
                    </MudChip>
                </Show>
            </MudCardContent>
        </MudCard>
    </ItemContent>
    <Empty>
        <MudText>No users found</MudText>
    </Empty>
    <Separator>
        <MudDivider />
    </Separator>
</ForEach>
```

---

#### **Repeat**

Simpler collection rendering without index tracking or separators.

**Purpose:**
- Lightweight alternative to ForEach
- When you only need the items
- Cleaner syntax for simple lists

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Items` | `IEnumerable<TItem>?` | No | `null` | Items to render |
| `ItemTemplate` | `RenderFragment<TItem>?` | Yes | - | Template per item |
| `EmptyTemplate` | `RenderFragment?` | No | `null` | Empty state template |

**Usage:**

```razor
<Repeat Items="@products" Context="product">
    <ItemTemplate>
        <MudCard>
            <MudCardHeader>
                <MudText>@product.Name</MudText>
            </MudCardHeader>
            <MudCardContent>
                <MudText>Price: @product.Price.ToString("C")</MudText>
            </MudCardContent>
        </MudCard>
    </ItemTemplate>
    <EmptyTemplate>
        <MudText Color="Color.Secondary">
            No products available
        </MudText>
    </EmptyTemplate>
</Repeat>
```

---

#### **Empty**

Displays content only when a collection is empty or null.

**Purpose:**
- Dedicated empty state component
- Cleaner than manual null checks
- Semantic and readable

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Items` | `IEnumerable<TItem>?` | No | `null` | Collection to check |
| `ChildContent` | `RenderFragment?` | No | - | Content when empty |

**Usage:**

```razor
<Empty Items="@orders">
    <MudPaper Class="pa-4 text-center">
        <MudIcon Icon="@Icons.Material.Filled.ShoppingCart" Size="Size.Large" />
        <MudText Typo="Typo.h6">No orders yet</MudText>
        <MudText>Start shopping to see your orders here</MudText>
        <MudButton Color="Color.Primary" Href="/shop" Class="mt-2">
            Start Shopping
        </MudButton>
    </MudPaper>
</Empty>
```

---

### Timing & Performance

#### **Debounce**

Debounces rendering based on a trigger value (waits for input to stabilize).

**Purpose:**
- Reduces unnecessary renders during rapid changes
- Perfect for search inputs
- Improves performance with expensive operations

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Value` | `TValue?` | No | - | Value to watch |
| `DelayMs` | `int` | No | `300` | Debounce delay |
| `OnDebounced` | `EventCallback<TValue?>` | No | - | Stabilization callback |
| `ChildContent` | `RenderFragment?` | No | - | Content to render |

**Usage:**

```razor
<MudTextField @bind-Value="searchTerm" Label="Search" />

<Debounce Value="@searchTerm" DelayMs="500" OnDebounced="@SearchAsync">
    <MudText>Search results for: @searchTerm</MudText>
    <SearchResults Query="@searchTerm" />
</Debounce>

@code {
    private string searchTerm = "";

    private async Task SearchAsync(string? query)
    {
        // This is called only after user stops typing for 500ms
        await Task.Delay(100); // Simulate search
    }
}
```

---

#### **Delay**

Delays rendering of content for a specified duration.

**Purpose:**
- Progressive content loading
- Prevents UI flicker for fast operations
- Reduces visual clutter during initialization

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Milliseconds` | `int` | No | `500` | Delay duration |
| `ChildContent` | `RenderFragment?` | No | - | Delayed content |

**Usage:**

```razor
<!-- Only show spinner if loading takes more than 500ms -->
<Delay Milliseconds="500">
    <Show When="@isLoading">
        <MudProgressCircular Indeterminate="true" />
    </Show>
</Delay>

<!-- Show tip after 3 seconds -->
<Delay Milliseconds="3000">
    <MudAlert Severity="Severity.Info">
        ?? Pro tip: Use keyboard shortcuts to navigate faster!
    </MudAlert>
</Delay>
```

---

#### **Timeout**

Renders content and automatically removes it after a timeout.

**Purpose:**
- Self-dismissing notifications
- Temporary messages
- Auto-hiding tooltips

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `DurationMs` | `int` | No | `5000` | Duration before expiry |
| `OnExpired` | `EventCallback` | No | - | Expiration callback |
| `ChildContent` | `RenderFragment?` | No | - | Temporary content |

**Usage:**

```razor
<Show When="@showSuccessMessage">
    <Timeout DurationMs="3000" OnExpired="@(() => showSuccessMessage = false)">
        <MudAlert Severity="Severity.Success">
            ? Changes saved successfully!
        </MudAlert>
    </Timeout>
</Show>

@code {
    private bool showSuccessMessage = false;

    private async Task SaveAsync()
    {
        await DataService.SaveAsync();
        showSuccessMessage = true;
    }
}
```

---

#### **Lazy**

Lazy loads content when it becomes visible in the viewport (uses Intersection Observer).

**Purpose:**
- Performance optimization for long pages
- Defers loading of off-screen content
- Reduces initial page load time

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Threshold` | `double` | No | `0.1` | Visibility threshold (0-1) |
| `RootMargin` | `string` | No | `"0px"` | Margin for detection |
| `ChildContent` | `RenderFragment?` | No | - | Lazy-loaded content |

**Usage:**

```razor
<!-- Only load when scrolled into view -->
<Lazy Threshold="0.5">
    <ExpensiveComponent />
</Lazy>

<!-- Load slightly before visible (preload) -->
<Lazy Threshold="0.1" RootMargin="200px">
    <MudImage Src="@largeImageUrl" />
</Lazy>
```

---

### Error Handling

#### **ErrorBoundary**

Catches exceptions from child components and displays custom error UI.

**Purpose:**
- Prevents entire app crashes from component errors
- Provides graceful error recovery
- Enhanced version of Blazor's built-in ErrorBoundary

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `OnError` | `EventCallback<Exception>` | No | - | Error callback |
| `ShowDetails` | `bool` | No | `false` | Show error details |
| `ErrorContent` | `RenderFragment<ErrorContext>?` | No | `null` | Custom error template |
| `AutoRetry` | `bool` | No | `false` | Auto-retry on error |
| `RetryDelayMs` | `int` | No | `1000` | Retry delay |
| `ChildContent` | `RenderFragment?` | No | - | Protected content |

**Usage:**

```razor
<ErrorBoundary OnError="@LogError" ShowDetails="@IsDevelopment">
    <ErrorContent Context="error">
        <MudAlert Severity="Severity.Error">
            <MudText Typo="Typo.h6">Something went wrong</MudText>
            <MudText>@error.Exception.Message</MudText>
            <Show When="@error.ShowDetails">
                <MudText Typo="Typo.body2" Class="mt-2">
                    @error.Exception.StackTrace
                </MudText>
            </Show>
        </MudAlert>
    </ErrorContent>
    <PotentiallyFailingComponent />
</ErrorBoundary>

@code {
    private void LogError(Exception ex)
    {
        Logger.LogError(ex, "Component error occurred");
    }
}
```

**With Auto-Retry:**

```razor
<ErrorBoundary AutoRetry="true" RetryDelayMs="2000">
    <UnstableComponent />
</ErrorBoundary>
```

---

### Layout Helpers

#### **Fragment**

Renders content without adding wrapper elements to the DOM.

**Purpose:**
- Avoids unnecessary DOM nodes
- Similar to React.Fragment
- Useful when parent expects specific structure

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `ChildContent` | `RenderFragment?` | No | - | Content to render |

**Usage:**

```razor
<MudList>
    <Fragment>
        <MudListItem>Item 1</MudListItem>
        <MudListItem>Item 2</MudListItem>
        <MudListItem>Item 3</MudListItem>
    </Fragment>
</MudList>

<!-- Instead of wrapping in a div which would break the list structure -->
```

---

## UI Components

### **DarkModeToggle**

A component for toggling between light and dark themes.

**Purpose:**
- Consistent dark mode switching UI
- Theme preference management
- Works with MudBlazor themes

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `DarkMode` | `bool` | Yes | - | Current theme state |
| `DarkModeChanged` | `EventCallback<bool>` | Yes | - | State change callback |

**Usage:**

```razor
<DarkModeToggle DarkMode="@isDarkMode" 
                DarkModeChanged="@OnDarkModeChanged" />

@code {
    private bool isDarkMode = false;

    private async Task OnDarkModeChanged(bool darkMode)
    {
        isDarkMode = darkMode;
        // Save preference, update theme, etc.
        await LocalStorage.SetItemAsync("darkMode", darkMode);
    }
}
```

---

## Best Practices

### Component Selection

**Conditional Rendering:**
- Use `Show` for simple show/hide logic
- Use `If` when you need both true and false cases
- Use `Hide` when "hide when X" reads more naturally
- Use `Toggle` for explicit state toggles
- Use `Switch` for multi-way branching

**Collections:**
- Use `Repeat` for simple list rendering
- Use `ForEach` when you need index or separators
- Use `Empty` for dedicated empty states

**Performance:**
- Use `Lazy` for off-screen content
- Use `Debounce` for search and rapid input
- Use `Delay` to prevent UI flicker
- Use `DataLoader` for consistent async patterns

### Error Handling

- Always wrap risky components in `ErrorBoundary`
- Enable `ShowDetails` only in development
- Provide user-friendly error messages
- Use `OnError` callback for logging

### Async Operations

- Always set `OnRetry` in `DataLoader`
- Provide meaningful error messages
- Consider custom loading templates for long operations
- Use `Placeholder` for better perceived performance

### Code Organization

```razor
<!-- Good: Logical grouping and nesting -->
<DataLoader IsLoading="@isLoading" HasError="@hasError" OnRetry="@LoadAsync">
    <Empty Items="@items">
        <MudText>No items found</MudText>
    </Empty>
    
    <ForEach Collection="@items">
        <ItemContent Context="ctx">
            <ErrorBoundary>
                <ItemCard Item="@ctx.Item" />
            </ErrorBoundary>
        </ItemContent>
    </ForEach>
</DataLoader>
```

---

## Component Hierarchy

All components inherit from `CraftComponent` or `ComponentBase`, providing:
- Consistent lifecycle management
- Async disposal support
- Logging capabilities
- Standard Blazor patterns

---

## Dependencies

- **Craft.UiComponents**: Provides base `CraftComponent` class
- **MudBlazor**: Used by some components for default UI (DataLoader, Spinner)
- **Microsoft.AspNetCore.Components**: Core Blazor framework
- **Microsoft.JSInterop**: Required by Lazy component

---

## Advanced Examples

### Combining Components

```razor
<!-- Complex data loading with multiple states -->
<DataLoader IsLoading="@isLoading" HasError="@hasError" OnRetry="@LoadUsersAsync">
    <If Condition="@users.Any()">
        <True>
            <ForEach Collection="@users">
                <ItemContent Context="ctx">
                    <Lazy>
                        <ErrorBoundary>
                            <UserCard User="@ctx.Item" IsFirst="@ctx.IsFirst" />
                        </ErrorBoundary>
                    </Lazy>
                </ItemContent>
                <Separator>
                    <MudDivider Class="my-2" />
                </Separator>
            </ForEach>
        </True>
        <False>
            <Empty Items="@users">
                <MudPaper Class="pa-8 text-center">
                    <MudIcon Icon="@Icons.Material.Filled.People" Size="Size.Large" />
                    <MudText Typo="Typo.h5" Class="mt-2">No Users Found</MudText>
                    <MudButton Color="Color.Primary" OnClick="@InviteUser" Class="mt-4">
                        Invite Users
                    </MudButton>
                </MudPaper>
            </Empty>
        </False>
    </If>
</DataLoader>
```

### Search with Debouncing

```razor
<MudTextField @bind-Value="searchQuery" 
              Label="Search products" 
              Immediate="true" />

<Debounce Value="@searchQuery" DelayMs="300" OnDebounced="@PerformSearch">
    <DataLoader IsLoading="@isSearching" HasError="@searchFailed">
        <Placeholder Loading="@isSearching" Lines="3">
            <Content>
                <Repeat Items="@searchResults" Context="product">
                    <ItemTemplate>
                        <ProductCard Product="@product" />
                    </ItemTemplate>
                    <EmptyTemplate>
                        <MudText>No products match your search</MudText>
                    </EmptyTemplate>
                </Repeat>
            </Content>
        </Placeholder>
    </DataLoader>
</Debounce>
```

### Progressive Loading

```razor
<div>
    <!-- Show immediately -->
    <MudText Typo="Typo.h4">Dashboard</MudText>
    
    <!-- Show after 500ms if still loading -->
    <Delay Milliseconds="500">
        <Show When="@isLoading">
            <Spinner />
        </Show>
    </Delay>
    
    <!-- Show after 3s as a tip -->
    <Delay Milliseconds="3000">
        <Hide When="@isLoading">
            <Timeout DurationMs="5000">
                <MudAlert Severity="Severity.Info">
                    ?? Tip: Use Ctrl+K to search
                </MudAlert>
            </Timeout>
        </Hide>
    </Delay>
</div>
```

---

## Contributing

Contributions are welcome! Please ensure:
- Components follow the existing patterns
- All public APIs are documented
- Components inherit from `CraftComponent` when appropriate
- Unit tests are provided for new components

---

## License

See the main project license.

---

## Related Libraries

- **Craft.UiComponents**: Base components and utilities
- **MudBlazor**: Material Design component library
