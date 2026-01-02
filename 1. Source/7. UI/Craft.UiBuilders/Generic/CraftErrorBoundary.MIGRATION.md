# CraftErrorBoundary Migration Guide

## Overview

The `ErrorBoundary` component has been renamed to `CraftErrorBoundary` and refactored to properly extend Microsoft's `ErrorBoundary` component, avoiding naming conflicts and providing enhanced functionality.

## What Changed

### 1. Component Rename
- **Old Name**: `ErrorBoundary`
- **New Name**: `CraftErrorBoundary`
- **Reason**: Avoid naming conflicts with `Microsoft.AspNetCore.Components.Web.ErrorBoundary`

### 2. Proper Inheritance
- **Old Base Class**: `CraftComponent`
- **New Base Class**: `Microsoft.AspNetCore.Components.Web.ErrorBoundary`
- **Benefit**: Now properly catches exceptions from child components using Blazor's built-in error boundary mechanism

### 3. Enhanced Features

The `CraftErrorBoundary` now includes:

#### Core Features (Inherited from Microsoft's ErrorBoundary)
- ? Proper exception catching from child components
- ? `CurrentException` property to access the current exception
- ? `Recover()` method to reset error state
- ? `ErrorContent` parameter for custom error UI

#### Enhanced Features (Added by Craft)
- ? `OnError` callback - Get notified when errors occur
- ? `ShowDetails` - Control whether to show detailed error information
- ? `CraftErrorContent` - Custom error content with `CraftErrorContext`
- ? `AutoRetry` - Automatically retry rendering after errors
- ? `RetryDelayMs` - Configure retry delay (default: 1000ms)
- ? `MaxRetryAttempts` - Limit retry attempts (default: 3)
- ? `RecoverAsync()` - Async method to recover from errors
- ? `ResetRetryCount()` - Reset the retry counter

### 4. Context Object Rename
- **Old Name**: `ErrorContext`
- **New Name**: `CraftErrorContext`
- **Added Properties**: `FullDetails` - Complete exception information when `ShowDetails` is true

### 5. Parameter Changes
- **Old Parameter**: `ErrorContent` (RenderFragment<ErrorContext>)
- **New Parameters**: 
  - `CraftErrorContent` (RenderFragment<CraftErrorContext>) - Custom Craft error content
  - `ErrorContent` (inherited from base) - Standard ErrorBoundary error content

## Migration Steps

### Step 1: Update Component References

**Before:**
```razor
<ErrorBoundary>
    <ChildContent>
        <!-- Your content -->
    </ChildContent>
</ErrorBoundary>
```

**After:**
```razor
<CraftErrorBoundary>
    <ChildContent>
        <!-- Your content -->
    </ChildContent>
</CraftErrorBoundary>
```

### Step 2: Update Using Directives

**Before:**
```csharp
using Craft.UiBuilders.Generic;
// ErrorBoundary was implicitly available
```

**After:**
```csharp
using Craft.UiBuilders.Generic;
// CraftErrorBoundary is now available
```

### Step 3: Update Custom Error Content

**Before:**
```razor
<ErrorBoundary>
    <ErrorContent Context="errorContext">
        <div>@errorContext.Message</div>
        @if (errorContext.StackTrace != null)
        {
            <pre>@errorContext.StackTrace</pre>
        }
    </ErrorContent>
    <ChildContent>
        <!-- Your content -->
    </ChildContent>
</ErrorBoundary>
```

**After (Option 1 - Using CraftErrorContent):**
```razor
<CraftErrorBoundary ShowDetails="true">
    <CraftErrorContent Context="errorContext">
        <div>@errorContext.Message</div>
        @if (errorContext.StackTrace != null)
        {
            <pre>@errorContext.StackTrace</pre>
        }
        @if (errorContext.FullDetails != null)
        {
            <details>
                <summary>Full Details</summary>
                <pre>@errorContext.FullDetails</pre>
            </details>
        }
    </CraftErrorContent>
    <ChildContent>
        <!-- Your content -->
    </ChildContent>
</CraftErrorBoundary>
```

**After (Option 2 - Using base ErrorContent):**
```razor
<CraftErrorBoundary>
    <ErrorContent Context="exception">
        <div>@exception.Message</div>
    </ErrorContent>
    <ChildContent>
        <!-- Your content -->
    </ChildContent>
</CraftErrorBoundary>
```

### Step 4: Update Error Callbacks

**Before:**
```razor
<ErrorBoundary OnError="HandleError">
    <!-- content -->
</ErrorBoundary>

@code {
    private void HandleError(Exception ex)
    {
        // Handle error
    }
}
```

**After:**
```razor
<CraftErrorBoundary OnError="HandleError">
    <!-- content -->
</CraftErrorBoundary>

@code {
    private async Task HandleError(Exception ex)
    {
        // Handle error asynchronously
        await LogErrorAsync(ex);
    }
}
```

### Step 5: Update Auto-Retry Usage

**New Feature - Auto Retry:**
```razor
<CraftErrorBoundary 
    AutoRetry="true" 
    RetryDelayMs="2000" 
    MaxRetryAttempts="3"
    OnError="LogError">
    <!-- Your content -->
</CraftErrorBoundary>

@code {
    private async Task LogError(Exception ex)
    {
        Console.WriteLine($"Error occurred: {ex.Message}");
    }
}
```

### Step 6: Update Programmatic Recovery

**Before:**
```csharp
errorBoundary.Recover();
```

**After:**
```csharp
await errorBoundary.RecoverAsync();

// Or reset retry count
errorBoundary.ResetRetryCount();
await errorBoundary.RecoverAsync();
```

## Usage Examples

### Basic Usage
```razor
<CraftErrorBoundary>
    <ChildContent>
        <MyComponent />
    </ChildContent>
</CraftErrorBoundary>
```

### With Custom Error Display
```razor
<CraftErrorBoundary ShowDetails="true">
    <CraftErrorContent Context="error">
        <div class="alert alert-danger">
            <h3>Error</h3>
            <p>@error.Message</p>
            @if (error.StackTrace != null)
            {
                <details>
                    <summary>Stack Trace</summary>
                    <pre>@error.StackTrace</pre>
                </details>
            }
        </div>
    </CraftErrorContent>
    <ChildContent>
        <MyComponent />
    </ChildContent>
</CraftErrorBoundary>
```

### With Auto-Retry and Error Logging
```razor
<CraftErrorBoundary 
    AutoRetry="true"
    RetryDelayMs="1500"
    MaxRetryAttempts="5"
    ShowDetails="@IsDebugMode"
    OnError="LogErrorAsync">
    <ChildContent>
        <MyComponent />
    </ChildContent>
</CraftErrorBoundary>

@code {
    private bool IsDebugMode => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    private async Task LogErrorAsync(Exception ex)
    {
        await Logger.LogExceptionAsync(ex);
        await NotificationService.ShowErrorAsync($"An error occurred: {ex.Message}");
    }
}
```

### With Manual Recovery
```razor
<CraftErrorBoundary @ref="_errorBoundary">
    <CraftErrorContent Context="error">
        <div class="error-container">
            <p>@error.Message</p>
            <button @onclick="HandleRecovery">Try Again</button>
        </div>
    </CraftErrorContent>
    <ChildContent>
        <MyComponent />
    </ChildContent>
</CraftErrorBoundary>

@code {
    private CraftErrorBoundary? _errorBoundary;

    private async Task HandleRecovery()
    {
        if (_errorBoundary != null)
        {
            _errorBoundary.ResetRetryCount();
            await _errorBoundary.RecoverAsync();
        }
    }
}
```

## Benefits of the Refactoring

1. **No Naming Conflicts**: Avoids ambiguity with Microsoft's `ErrorBoundary`
2. **Proper Exception Catching**: Now actually catches exceptions from child components (the old implementation didn't)
3. **Enhanced Features**: Adds auto-retry, error callbacks, and more control
4. **Better Architecture**: Extends rather than reimplements, maintaining compatibility with Blazor's ecosystem
5. **Improved Testing**: More reliable error boundary behavior in tests
6. **Future-Proof**: Leverages Microsoft's error handling infrastructure

## Breaking Changes

1. Component name changed from `ErrorBoundary` to `CraftErrorBoundary`
2. Context object renamed from `ErrorContext` to `CraftErrorContext`
3. Parameter renamed from `ErrorContent` to `CraftErrorContent` (for typed context)
4. `Recover()` is now `RecoverAsync()` (async method)
5. No longer inherits from `CraftComponent` (inherits from `ErrorBoundary` instead)

## Testing Changes

Update your test file references:

**Before:**
```csharp
var cut = Render<ErrorBoundary>(...);
var context = new ErrorContext(exception, true);
```

**After:**
```csharp
var cut = Render<CraftErrorBoundary>(...);
var context = new CraftErrorContext(exception, true);
```

## Questions or Issues?

If you encounter any issues during migration, please:
1. Check that you've updated all component references
2. Verify that using directives are correct
3. Ensure async/await is used with `RecoverAsync()`
4. Review the examples above for proper usage patterns
