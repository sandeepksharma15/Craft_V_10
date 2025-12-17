# Understanding the `Disabled` Property in Event Interfaces

## Why Event Interfaces Need `Disabled`

The event interfaces provide **smart default behavior** that automatically prevents event handling when a component is disabled:

```csharp
public interface IMouseEvents
{
    bool Disabled { get; }  // ?? This is needed for the default implementation below
    
    Task HandleMouseDownAsync(MouseEventArgs args)
    {
        if (Disabled)  // ?? Check before firing event
            return Task.CompletedTask;
            
        return OnMouseDown.InvokeAsync(args);
    }
}
```

## How It Works in Practice

### Scenario 1: Using CraftComponent (Recommended)

```csharp
// ? EASIEST - Disabled is inherited
public class CraftButton : CraftComponent, IMouseEvents
{
    [Parameter] public EventCallback<MouseEventArgs> OnMouseDown { get; set; }
    
    // That's it! Disabled comes from CraftComponent
    // Default handler automatically checks it
}
```

**In Razor:**
```razor
<button @onmousedown="this.HandleMouseDownAsync"
        disabled="@Disabled">
    @ChildContent
</button>

<!-- Usage: -->
<CraftButton Disabled="true">Can't Click Me</CraftButton>
```

### Scenario 2: Using CraftInteractiveComponent

```csharp
// ? EVEN EASIER - Already implements all event interfaces
public class CraftMenuItem : CraftInteractiveComponent
{
    // Disabled inherited
    // All event handlers inherited
    // Just add your custom properties
    [Parameter] public string? Icon { get; set; }
}
```

### Scenario 3: Custom Component Without CraftComponent

```csharp
// ?? MORE WORK - Must implement Disabled yourself
public class MyCustomButton : ComponentBase, IMouseEvents
{
    // Must provide Disabled to satisfy interface
    [Parameter]
    public bool Disabled { get; set; }
    
    // Must provide all event callbacks
    [Parameter] public EventCallback<MouseEventArgs> OnDoubleClick { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseEnter { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseLeave { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseDown { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseUp { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseMove { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnContextMenu { get; set; }
    
    // Handlers come from default interface implementation ?
}
```

## The Magic: Default Interface Implementation

When you call `this.HandleMouseDownAsync()` in your Razor markup:

1. **Blazor calls the method** defined in the interface
2. **Interface checks `Disabled`** using the property you provided
3. **If disabled:** Returns immediately (no event fired)
4. **If enabled:** Fires the `OnMouseDown` event callback

```csharp
// This happens automatically:
Task HandleMouseDownAsync(MouseEventArgs args)
{
    if (Disabled)        // ?? Uses your Disabled property
        return Task.CompletedTask;
        
    return OnMouseDown.InvokeAsync(args);  // ?? Fires your callback
}
```

## Why This Design is Better

### Before (Old Pattern - No Interface):
```csharp
public class OldButton : CraftComponent
{
    protected async Task HandleMouseDownAsync(MouseEventArgs args)
    {
        if (Disabled) return;  // ?? YOU must remember to check Disabled
        await OnMouseDown.InvokeAsync(args);
    }
}
```
**Problem:** Easy to forget the disabled check, inconsistent across components

### After (Interface Pattern):
```csharp
public class NewButton : CraftComponent, IMouseEvents
{
    // Interface handles it automatically ?
}
```
**Benefit:** Can't forget - it's built into the interface!

## Override When Needed

If you need custom behavior, you can override the default:

```csharp
public class CraftButton : CraftComponent, IMouseEvents
{
    [Parameter] public EventCallback<MouseEventArgs> OnMouseDown { get; set; }
    
    // Override the default implementation
    Task IMouseEvents.HandleMouseDownAsync(MouseEventArgs args)
    {
        // Custom logic here
        if (Disabled)
        {
            LogDebug("Button is disabled, ignoring click");
            return Task.CompletedTask;
        }
        
        LogDebug("Button clicked at ({X}, {Y})", args.ClientX, args.ClientY);
        return OnMouseDown.InvokeAsync(args);
    }
}
```

## Summary

- **`Disabled` is required** by event interfaces for smart default behavior
- **`CraftComponent` provides it** - so you don't have to think about it
- **Default implementations check it** - automatic disabled-state handling
- **You can override** if you need custom behavior
- **It's type-safe** - compiler ensures you provide it

This is a key example of how **default interface implementations** make the API safer and easier to use! ??
