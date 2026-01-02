# HandleError Component

## Overview

The `HandleError` component is a production-ready error handling wrapper for Blazor applications that provides comprehensive error boundary protection with logging, user-friendly error displays, and flexible error recovery options.

## Features

- ? **Error Boundary Protection**: Catches unhandled exceptions in child components
- ? **Structured Logging**: Automatic error logging with correlation IDs
- ? **User-Friendly Error UI**: Clean, accessible error display with customization options
- ? **Error Page Navigation**: Optional redirection to dedicated error pages
- ? **Accessibility**: ARIA attributes for screen readers
- ? **Dark Mode Support**: CSS variables for theme customization
- ? **Error Recovery**: Ability to recover and retry rendering
- ? **Production-Ready**: Configurable detail levels for different environments
- ? **.NET 10 Compatible**: Uses latest C# 14 and .NET 10 features
- ? **Craft Framework Integration**: Uses generic components like `If`, `Show` for declarative rendering

## Basic Usage

```razor
<HandleError>
    <YourComponent />
</HandleError>
```

## Advanced Usage

### With Error Page Navigation

```razor
<HandleError ErrorPageUri="/error" ShowDetails="@IsDevelopment">
    <YourComponent />
</HandleError>
```

### With Custom Error Content

```razor
<HandleError ShowDetails="true">
    <ChildContent>
        <YourComponent />
    </ChildContent>
    <CustomErrorContent>
        <div class="my-custom-error">
            <h1>Oops! Something went wrong</h1>
            <p>Please contact support</p>
        </div>
    </CustomErrorContent>
</HandleError>
```

### With Error Callback

```razor
<HandleError OnError="@HandleError" ShowDetails="false">
    <YourComponent />
</HandleError>

@code {
    private async Task HandleError(Exception ex)
    {
        // Custom error handling logic
        await NotificationService.ShowError("An error occurred");
        await AnalyticsService.TrackError(ex);
    }
}
```

### Programmatic Error Processing

```razor
<HandleError @ref="_handleError" ErrorPageUri="/error">
    <YourComponent />
</HandleError>

@code {
    private HandleError? _handleError;

    private void OnButtonClick()
    {
        try
        {
            // Your code
        }
        catch (Exception ex)
        {
            _handleError?.ProcessError(ex, "Failed to process request");
        }
    }
}
```

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | **Required**. The child content to wrap with error protection |
| `ErrorPageUri` | `string?` | `null` | URI of error page to navigate to (e.g., "/error"). If not set, errors display inline |
| `ShowDetails` | `bool` | `false` | Whether to show detailed error info (stack traces). **Set to false in production** |
| `CustomErrorContent` | `RenderFragment?` | `null` | Custom error UI to display instead of default error message |
| `OnError` | `EventCallback<Exception>` | - | Callback invoked when an error is caught |

## Public Methods

### ProcessError

Manually process an error with logging and optional navigation.

```csharp
public void ProcessError(Exception exception, string? customMessage = null)
```

**Parameters:**
- `exception`: The exception that occurred
- `customMessage`: Optional custom message to display

**Example:**
```csharp
try
{
    await SomeRiskyOperation();
}
catch (Exception ex)
{
    handleErrorComponent.ProcessError(ex, "Operation failed");
}
```

### Recover

Recovers from the error state and attempts to re-render the component.

```csharp
public void Recover()
```

**Example:**
```razor
<HandleError @ref="_errorHandler">
    <YourComponent />
</HandleError>

<button @onclick="() => _errorHandler?.Recover()">Try Again</button>

@code {
    private HandleError? _errorHandler;
}
```

## Styling

The component includes comprehensive CSS styling in `HandleError.razor.css`. You can customize the appearance using CSS variables:

```css
:root {
    --error-bg: #fef2f2;
    --error-color: #dc2626;
    --error-title-color: #991b1b;
    --error-text-color: #7f1d1d;
    --error-button-bg: #dc2626;
    --error-button-hover: #b91c1c;
}

/* Dark mode */
@media (prefers-color-scheme: dark) {
    :root {
        --error-bg-dark: #1f1f1f;
        --error-title-color-dark: #fca5a5;
        --error-text-color-dark: #fecaca;
    }
}
```

## Error ID Tracking

Each error is assigned a unique correlation ID based on:
1. Current Activity ID (if available)
2. Generated GUID (if no Activity ID)

This ID is:
- Logged for traceability
- Displayed to users (in development mode)
- Passed to error pages via query string
- Available for support ticket creation

## Best Practices

### 1. Production vs Development

```csharp
@inject IWebHostEnvironment Environment

<HandleError ShowDetails="@Environment.IsDevelopment()">
    <YourComponent />
</HandleError>
```

### 2. Centralized Error Logging

```csharp
<HandleError OnError="@LogError">
    <YourComponent />
</HandleError>

@code {
    [Inject] private ILogger<MainLayout> Logger { get; set; }
    [Inject] private ITelemetry Telemetry { get; set; }

    private async Task LogError(Exception ex)
    {
        Logger.LogError(ex, "Component error");
        await Telemetry.TrackException(ex);
    }
}
```

### 3. User-Friendly Messages

```csharp
try
{
    await DeleteUserAccount();
}
catch (Exception ex)
{
    _errorHandler?.ProcessError(ex, 
        "Unable to delete account. Please try again or contact support.");
}
```

### 4. Nested Error Boundaries

```razor
<HandleError ErrorPageUri="/error">
    <MainLayout>
        <HandleError ShowDetails="@IsDevelopment">
            <PageContent />
        </HandleError>
    </MainLayout>
</HandleError>
```

## Craft Framework Integration

### Generic Components Usage

`HandleError` leverages Craft's generic components for cleaner, more declarative conditional rendering:

#### `If` Component
Used for mutually exclusive conditional rendering (either/or scenarios):

```razor
<If Condition="@(CustomErrorContent is not null)">
    <True>
        @CustomErrorContent
    </True>
    <False>
        <!-- Default error UI -->
    </False>
</If>
```

#### `Show` Component
Used for simple conditional rendering (show when true):

```razor
<Show When="@(ShowDetails && errorContext is not null)">
    <details class="error-details">
        <summary>Technical Details</summary>
        <pre class="error-stack">@errorContext</pre>
    </details>
</Show>
```

#### Benefits of Generic Components

1. **Declarative**: More readable than inline `@if` statements
2. **Consistent**: Follows Craft framework patterns
3. **Reusable**: Same components used across the framework
4. **Testable**: Easier to unit test conditional logic
5. **Maintainable**: Clear separation of concerns

### Extending with Other Craft Components

You can enhance `HandleError` with additional Craft components:

```razor
<HandleError ErrorPageUri="/error">
    <ChildContent>
        <Timeout Milliseconds="5000">
            <SlowComponent />
        </Timeout>
    </ChildContent>
    <CustomErrorContent>
        <Delay Milliseconds="300">
            <FadeIn>
                <ErrorMessage />
            </FadeIn>
        </Delay>
    </CustomErrorContent>
</HandleError>
```

**Available Generic Components:**
- `If` - Conditional rendering with True/False branches
- `Show` - Show content when condition is true
- `Hide` - Hide content when condition is true
- `Switch` - Multi-case conditional rendering
- `ForEach` - Iterate over collections
- `Repeat` - Repeat content N times
- `Lazy` - Lazy load components
- `Timeout` - Timeout wrapper
- `Delay` - Delayed rendering
- `Debounce` - Debounced input
- `DataLoader` - Async data loading
- `Placeholder` - Loading placeholders

## Error Page Integration

When `ErrorPageUri` is set, errors navigate to the specified page with query parameters:

```
/error?message=Error+message&id=correlation-id
```

**Example error page:**

```razor
@page "/error"
@using Microsoft.AspNetCore.WebUtilities

<h1>Error</h1>

<p>@_errorMessage</p>
<p><small>Error ID: @_errorId</small></p>

@code {
    private string _errorMessage = "An unexpected error occurred";
    private string _errorId = "";

    protected override void OnInitialized()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("message", out var message))
            _errorMessage = message;
            
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("id", out var id))
            _errorId = id;
    }
}
```

## Accessibility

The component follows WCAG guidelines:

- `role="alert"` for error container
- `aria-live="assertive"` for immediate announcement
- Semantic HTML structure
- Keyboard-accessible buttons
- Focus management

## Browser Compatibility

Works with all modern browsers supporting:
- .NET 10 Blazor
- CSS Grid and Flexbox
- CSS custom properties
- SVG

## Dependencies

- `Microsoft.AspNetCore.Components`
- `Microsoft.AspNetCore.Components.Web`
- `Microsoft.Extensions.Logging`
- `System.Diagnostics`

## Related Components

- `ErrorBoundary` (Craft.UiBuilders.Generic) - Custom error boundary implementation
- `Microsoft.AspNetCore.Components.Web.ErrorBoundary` - Built-in Blazor error boundary

## Migration from Old HandleError

**Old Code:**
```razor
<CustomErrorBoundary ErrorPageUri="@ErrorPageUri">
    <ChildContent>
        @ChildContent
    </ChildContent>
    <ErrorContent>
        An Erro has occurred. Still working
    </ErrorContent>
</CustomErrorBoundary>
```

**New Code:**
```razor
<HandleError ErrorPageUri="@ErrorPageUri" ShowDetails="false">
    @ChildContent
</HandleError>
```

## License

Part of the Craft Framework - see project license for details.
