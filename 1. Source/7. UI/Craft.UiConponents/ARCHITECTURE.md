# Craft UI Components - Architecture Overview

## Design Philosophy: Interface-Based Composition

The Craft UI component library uses **interface-based composition** with C# 8.0+ default interface implementations. This provides:

- **True composition** - Implement only the interfaces you need
- **No forced inheritance** - Interfaces can be mixed and matched
- **Clean IntelliSense** - Components only show relevant parameters
- **Default behavior** - Interface implementations provide sensible defaults
- **Easy to extend** - Add new interfaces without breaking existing code

---

## Component Hierarchy

```
CraftComponent (minimal base)
?   ??? ElementRef & OnClick (80% use case)
?   ??? Styling, Theming, Animation
?   ??? Core properties
?
??? CraftInteractiveComponent : CraftComponent
?   ?   Implements: IMouseEvents, IKeyboardEvents, IFocusEvents, ITouchEvents
?   ?   (All event callbacks + default handlers)
?   ?
?   ??? CraftJsComponent : CraftInteractiveComponent, IJsComponent
?   ?   ?   (Adds JS module management via JsModuleHandler)
?   ?
?   ??? CraftInputComponent<T> : CraftInteractiveComponent
?           (Adds EditContext, validation, two-way binding)
```

---

## Available Interfaces

### Event Interfaces

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IMouseEvents` | OnDoubleClick, OnMouseEnter/Leave/Down/Up/Move, OnContextMenu | Components needing extended mouse interaction |
| `IKeyboardEvents` | OnKeyDown, OnKeyUp, OnKeyPress | Components handling keyboard input |
| `IFocusEvents` | OnFocus, OnBlur | Components managing focus state |
| `ITouchEvents` | OnTouchStart/End/Move/Cancel | Touch-enabled components |

### JS Interop Interface

| Interface | Members | When to Use |
|-----------|---------|-------------|
| `IJsComponent` | JsModule, IsJsInitialized | Components requiring JavaScript interop |

---

## Usage Examples

### Example 1: Simple Display Component
```csharp
// Only needs base functionality
public class CraftCard : CraftComponent
{
    protected override string GetComponentCssClass() => "craft-card";
}
```

**Razor:**
```razor
<div @ref="ElementRef" 
     id="@GetId()" 
     class="@BuildCssClass()" 
     style="@BuildStyle()"
     @onclick="HandleClickAsync">
    @ChildContent
</div>
```

---

### Example 2: Interactive Button (Pick and Choose Interfaces)
```csharp
// Implement only the interfaces you need
public class CraftButton : CraftComponent, IMouseEvents, IKeyboardEvents
{
    [Parameter] public EventCallback<MouseEventArgs> OnDoubleClick { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseEnter { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseLeave { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseDown { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseUp { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnMouseMove { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnContextMenu { get; set; }
    
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyPress { get; set; }

    protected override string GetComponentCssClass() => "craft-button";
}
```

**Razor:**
```razor
<button @ref="ElementRef"
        id="@GetId()"
        class="@BuildCssClass()"
        disabled="@Disabled"
        @onclick="HandleClickAsync"
        @ondblclick="this.HandleDoubleClickAsync"
        @onmouseenter="this.HandleMouseEnterAsync"
        @onmouseleave="this.HandleMouseLeaveAsync"
        @onkeydown="this.HandleKeyDownAsync">
    @ChildContent
</button>
```

---

### Example 3: Use CraftInteractiveComponent (All Events)
```csharp
// Already implements all event interfaces
public class CraftMenuItem : CraftInteractiveComponent
{
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string? Href { get; set; }

    protected override string GetComponentCssClass() => "craft-menu-item";
}
```

---

### Example 4: Component with JS Interop
```csharp
public class CraftTooltip : CraftJsComponent
{
    [Parameter] public string? Text { get; set; }
    [Parameter] public string Placement { get; set; } = "top";

    protected override string JsModulePath => 
        "./_content/Craft.UiConponents/js/tooltip.js";

    protected override async Task OnJsModuleInitializedAsync()
    {
        await InvokeJsVoidAsync("initialize", ElementRef, new 
        { 
            text = Text, 
            placement = Placement 
        });
    }

    protected override string GetComponentCssClass() => "craft-tooltip";
}
```

---

### Example 5: Custom Component Using JsModuleHandler Directly
```csharp
public class CraftChart : CraftComponent
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    
    private JsModuleHandler? _chartJs;
    
    [Parameter] public ChartData? Data { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _chartJs = new JsModuleHandler(
                JsRuntime, 
                "./_content/Craft.UiConponents/js/chart.js",
                LogDebug, 
                LogWarning, 
                LogError);
                
            await _chartJs.InitializeAsync(this);
            await _chartJs.InvokeVoidAsync("createChart", ElementRef, Data);
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_chartJs is not null)
            await _chartJs.DisposeAsync();
            
        await base.DisposeAsyncCore();
    }
}
```

---

### Example 6: Form Input Component
```csharp
public class CraftTextBox : CraftInputComponent<string>
{
    [Parameter] public InputType Type { get; set; } = InputType.Text;
    [Parameter] public int? MaxLength { get; set; }

    protected override bool TryParseValueFromString(
        string? value, 
        out string result, 
        out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;
        return true;
    }

    protected override string GetComponentCssClass() => "craft-textbox";
}
```

**Razor:**
```razor
<input @ref="ElementRef"
       type="@Type"
       id="@GetId()"
       class="@BuildCssClass()"
       value="@CurrentValue"
       placeholder="@Placeholder"
       disabled="@Disabled"
       readonly="@ReadOnly"
       maxlength="@MaxLength"
       @onchange="HandleChangeAsync"
       @onfocus="this.HandleFocusAsync"
       @onblur="this.HandleBlurAsync" />
```

---

## Advantages of This Design

### 1. **Minimal Base Class**
```csharp
// CraftComponent only has what 90% of components need
// ElementRef, OnClick, Styling, Theming
public class CraftBadge : CraftComponent { }
```

### 2. **Interface Composition**
```csharp
// Mix and match interfaces as needed
public class CraftSwitch : CraftComponent, IFocusEvents, IKeyboardEvents
{
    // Only implement what you need
}
```

### 3. **Default Implementations Work**
```csharp
// No need to write handler methods - interfaces provide them!
public class CraftChip : CraftComponent, IMouseEvents
{
    // Properties automatically get default HandleMouseEnterAsync(), etc.
}
```

### 4. **Easy to Extend**
```csharp
// Add new interfaces without modifying existing code
public interface IDragDropEvents
{
    [Parameter] EventCallback<DragEventArgs> OnDragStart { get; set; }
    [Parameter] EventCallback<DragEventArgs> OnDrop { get; set; }
    
    Task HandleDragStartAsync(DragEventArgs args) => OnDragStart.InvokeAsync(args);
    Task HandleDropAsync(DragEventArgs args) => OnDrop.InvokeAsync(args);
}

// Use it anywhere
public class CraftFileUpload : CraftComponent, IDragDropEvents
{
    [Parameter] public EventCallback<DragEventArgs> OnDragStart { get; set; }
    [Parameter] public EventCallback<DragEventArgs> OnDrop { get; set; }
}
```

### 5. **Override When Needed**
```csharp
public class CraftButton : CraftComponent, IMouseEvents
{
    // Use default implementations...
    [Parameter] public EventCallback<MouseEventArgs> OnMouseEnter { get; set; }
    
    // ...but override for custom behavior
    Task IMouseEvents.HandleMouseEnterAsync(MouseEventArgs args)
    {
        // Custom logic here
        LogDebug("Button hover started");
        return OnMouseEnter.InvokeAsync(args);
    }
}
```

---

## Migration from Other Patterns

### Before (Awkward "Feature Classes"):
```csharp
public class MyComponent : CraftComponent
{
    private MouseEvents _mouseEvents = new();
    // Can't actually use this - it's not composition!
}
```

### After (True Composition):
```csharp
public class MyComponent : CraftComponent, IMouseEvents
{
    // Interface provides everything - true composition!
    [Parameter] public EventCallback<MouseEventArgs> OnMouseEnter { get; set; }
}
```

---

## Best Practices

1. **Start with `CraftComponent`** - Add interfaces only when needed
2. **Use `CraftInteractiveComponent`** - When you need all events
3. **Implement specific interfaces** - For custom components needing only some events
4. **Use `JsModuleHandler` directly** - For components not inheriting from `CraftJsComponent`
5. **Override defaults sparingly** - Default implementations cover most cases

---

## Summary

This architecture provides the best of both worlds:
- **Simple base class** with essentials (ElementRef, OnClick)
- **Composable interfaces** for additional functionality
- **No forced overhead** - Only pay for what you use
- **Modern C#** - Leverages default interface implementations
- **Extensible** - Easy to add new capabilities
