# NavigationExtensions Usage Guide

## Overview

`NavigationExtensions` provides browser history navigation capabilities for Blazor applications using proper dependency injection patterns compatible with .NET 10.

## Key Improvements

? **No static state** - Thread-safe and supports multiple users  
? **Proper dependency injection** - Uses `IJSRuntime` as a parameter  
? **Null-safety** - Uses `ArgumentNullException.ThrowIfNull` (.NET 10 feature)  
? **Error handling** - Handles `JSException`, `JSDisconnectedException`, and `TaskCanceledException`  
? **Cancellation support** - All methods support `CancellationToken`  
? **XML documentation** - Fully documented for IntelliSense  
? **Additional methods** - Includes `GoForwardAsync`, `GoAsync`, and `ReloadAsync`

## Usage Examples

### Basic Usage in Razor Components

```razor
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime

<button @onclick="HandleGoBack">Go Back</button>
<button @onclick="HandleGoForward">Go Forward</button>
<button @onclick="HandleReload">Reload</button>

@code {
    private async Task HandleGoBack()
    {
        // Navigate back with fallback to home page
        await Navigation.GoBackAsync(JSRuntime, fallbackUrl: "/");
    }

    private async Task HandleGoForward()
    {
        await Navigation.GoForwardAsync(JSRuntime);
    }

    private async Task HandleReload()
    {
        // Force reload from server
        await Navigation.ReloadAsync(JSRuntime, forceReload: true);
    }
}
```

### With Cancellation Token Support

```csharp
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime

@code {
    private CancellationTokenSource _cts = new();

    private async Task NavigateWithCancellation()
    {
        try
        {
            await Navigation.GoBackAsync(JSRuntime, cancellationToken: _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

### Navigate to Specific History Position

```csharp
// Go back 2 pages
await Navigation.GoAsync(JSRuntime, delta: -2);

// Go forward 1 page
await Navigation.GoAsync(JSRuntime, delta: 1);
```

### Custom Fallback URL

```csharp
// If history navigation fails, navigate to a specific page
await Navigation.GoBackAsync(JSRuntime, fallbackUrl: "/dashboard");

// No fallback (will stay on current page if navigation fails)
await Navigation.GoBackAsync(JSRuntime, fallbackUrl: null);
```

## Migration Guide

### Old API (Deprecated)

```csharp
// MainLayout.razor.cs
[Inject] private IJSRuntime _jSRuntime { get; set; }

protected override void OnAfterRender(bool firstRender)
{
    if (firstRender)
    {
        NavigationExtensions.SetJsRuntime(_jSRuntime);
    }
}

// Usage
await Navigation.GoBack();
```

### New API (Recommended)

```csharp
// Component.razor
@inject IJSRuntime JSRuntime

// Usage - just pass JSRuntime as parameter
await Navigation.GoBackAsync(JSRuntime);
```

## Available Methods

| Method | Description |
|--------|-------------|
| `GoBackAsync()` | Navigate to the previous page in browser history |
| `GoForwardAsync()` | Navigate to the next page in browser history |
| `GoAsync(delta)` | Navigate to a specific position in history (negative = back, positive = forward) |
| `ReloadAsync()` | Reload the current page (with optional force reload) |

## Error Handling

All methods handle the following exceptions internally:
- **JSException** - JavaScript execution errors (uses fallback URL for `GoBackAsync`)
- **JSDisconnectedException** - When the SignalR circuit is disconnected
- **TaskCanceledException** - When operation is cancelled via `CancellationToken`

## Best Practices

1. **Always inject IJSRuntime** - Pass it as a parameter to navigation methods
2. **Use fallback URLs** - Provide a fallback URL for `GoBackAsync` to handle edge cases
3. **Handle cancellation** - Use `CancellationToken` for operations that might be interrupted
4. **Consider user experience** - Not all users have history to go back to
5. **Test server-side rendering** - Ensure navigation works in both InteractiveServer and InteractiveWebAssembly modes

## Thread Safety

? This implementation is fully thread-safe as it doesn't use any static mutable state.

## Compatibility

- ? .NET 10 (uses modern C# 14 features)
- ? Blazor Server
- ? Blazor WebAssembly
- ? Blazor United / Auto render mode
