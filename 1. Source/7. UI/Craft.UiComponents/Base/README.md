# CraftComponent Architecture

## Overview

The Craft UI Components library follows a lightweight, opt-in architecture for component features. The base `CraftComponent` class provides only core functionality, while additional capabilities like theming and animation are available through interfaces.

## Base Components

### CraftComponent (Lightweight Base)

The `CraftComponent` provides essential component functionality:

- **Common Parameters**: Id, Class, Style, Visible, Disabled, Tag, ChildContent
- **Element Reference**: For JavaScript interop
- **Event Handling**: Basic click event support
- **Lifecycle Management**: Initialization and disposal
- **Service Access**: Optional IThemeService and ILogger
- **Utility Methods**: Refresh(), RefreshAsync(), BuildCssClass(), BuildStyle()

**Usage Example:**
```csharp
public class CraftCustom : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-custom";
}
```

### ThemedCraftComponent (Full-Featured Base)

For components that need both theming and animation support, extend `ThemedCraftComponent` which implements both `IThemeable` and `IAnimatable`:

**Usage Example:**
```csharp
public class CraftButton : ThemedCraftComponent
{
    protected override string? GetComponentCssClass() => "craft-button";
}
```

## Interfaces

### IThemeable

Provides theming capabilities including size and variant styling.

**Properties:**
- `ComponentSize Size { get; set; }` - Size variant (ExtraSmall, Small, Medium, Large, ExtraLarge)
- `ComponentVariant Variant { get; set; }` - Style variant (Default, Primary, Secondary, Success, Warning, Danger, Info, Light, Dark, Outlined, Text)

**Methods:**
- `string? GetSizeCssClass()` - Returns CSS class for size
- `string? GetVariantCssClass()` - Returns CSS class for variant

**Usage Example:**
```csharp
public class CraftCard : CraftComponent, IThemeable
{
    [Parameter] public ComponentSize Size { get; set; } = ComponentSize.Medium;
    [Parameter] public ComponentVariant Variant { get; set; } = ComponentVariant.Default;

    protected override string BuildCssClass()
    {
        var baseClass = base.BuildCssClass();
        var builder = new CssBuilder(baseClass)
            .AddClass(((IThemeable)this).GetSizeCssClass())
            .AddClass(((IThemeable)this).GetVariantCssClass());
        
        return builder.Build();
    }
}
```

### IAnimatable

Provides animation capabilities.

**Properties:**
- `AnimationType Animation { get; set; }` - Animation type (None, Fade, Slide, Scale, Collapse, Bounce, Shake, Pulse, Flip, Rotate)
- `AnimationDuration AnimationDuration { get; set; }` - Duration preset (None, ExtraFast, Fast, Normal, Slow, ExtraSlow)
- `int? CustomAnimationDurationMs { get; set; }` - Custom duration in milliseconds (overrides AnimationDuration)

**Methods:**
- `string? GetAnimationCssClass()` - Returns CSS class for animation
- `string? GetAnimationStyle()` - Returns inline style for animation duration

**Usage Example:**
```csharp
public class CraftModal : CraftComponent, IAnimatable
{
    [Parameter] public AnimationType Animation { get; set; } = AnimationType.Fade;
    [Parameter] public AnimationDuration AnimationDuration { get; set; } = AnimationDuration.Normal;
    [Parameter] public int? CustomAnimationDurationMs { get; set; }

    protected override string BuildCssClass()
    {
        var baseClass = base.BuildCssClass();
        var builder = new CssBuilder(baseClass)
            .AddClass(((IAnimatable)this).GetAnimationCssClass());
        
        return builder.Build();
    }

    protected override string? BuildStyle()
    {
        var baseStyle = base.BuildStyle();
        var animationStyle = ((IAnimatable)this).GetAnimationStyle();
        
        if (string.IsNullOrEmpty(baseStyle))
            return animationStyle;
        if (string.IsNullOrEmpty(animationStyle))
            return baseStyle;
            
        return $"{baseStyle}; {animationStyle}";
    }
}
```

## Design Benefits

### Memory Efficiency
Components only carry the properties they need. A simple semantic wrapper doesn't need theming or animation properties.

### Flexibility
Mix and match interfaces based on component requirements:
- Simple components: Just `CraftComponent`
- Themed components: `CraftComponent` + `IThemeable`
- Animated components: `CraftComponent` + `IAnimatable`
- Full-featured components: `ThemedCraftComponent` (or manually implement both interfaces)

### Maintainability
- Single Responsibility: Base component focuses on core functionality
- Separation of Concerns: Theming and animation are distinct, optional features
- Clear Contracts: Interfaces define explicit capabilities

### Extensibility
Easy to add new optional features through additional interfaces without bloating the base class.

## Migration Guide

### For Existing Components

If your component previously extended `CraftComponent` and used `Size`, `Variant`, `Animation`, or related properties:

**Option 1: Use ThemedCraftComponent** (Recommended for most cases)
```csharp
// Before
public class MyComponent : CraftComponent { }

// After
public class MyComponent : ThemedCraftComponent { }
```

**Option 2: Implement Interfaces Selectively**
```csharp
// Only need theming
public class MyComponent : CraftComponent, IThemeable
{
    [Parameter] public ComponentSize Size { get; set; } = ComponentSize.Medium;
    [Parameter] public ComponentVariant Variant { get; set; } = ComponentVariant.Default;
    
    protected override string BuildCssClass()
    {
        return new CssBuilder(base.BuildCssClass())
            .AddClass(((IThemeable)this).GetSizeCssClass())
            .AddClass(((IThemeable)this).GetVariantCssClass())
            .Build();
    }
}
```

### For Simple Semantic Components

Components that don't need theming or animation can continue to extend `CraftComponent` directly with no changes required:

```csharp
public class CraftDiv : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-div";
}
```

## Examples

### Lightweight Semantic Component
```csharp
public class CraftSection : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-section";
}
```

### Themed Component
```csharp
public class CraftBadge : CraftComponent, IThemeable
{
    [Parameter] public ComponentSize Size { get; set; } = ComponentSize.Medium;
    [Parameter] public ComponentVariant Variant { get; set; } = ComponentVariant.Default;

    protected override string BuildCssClass()
    {
        return new CssBuilder(base.BuildCssClass())
            .AddClass("craft-badge")
            .AddClass(((IThemeable)this).GetSizeCssClass())
            .AddClass(((IThemeable)this).GetVariantCssClass())
            .Build();
    }
}
```

### Animated Component
```csharp
public class CraftCollapse : CraftComponent, IAnimatable
{
    [Parameter] public AnimationType Animation { get; set; } = AnimationType.Collapse;
    [Parameter] public AnimationDuration AnimationDuration { get; set; } = AnimationDuration.Normal;
    [Parameter] public int? CustomAnimationDurationMs { get; set; }

    protected override string BuildCssClass()
    {
        return new CssBuilder(base.BuildCssClass())
            .AddClass("craft-collapse")
            .AddClass(((IAnimatable)this).GetAnimationCssClass())
            .Build();
    }

    protected override string? BuildStyle()
    {
        var baseStyle = base.BuildStyle();
        var animationStyle = ((IAnimatable)this).GetAnimationStyle();
        return string.IsNullOrEmpty(baseStyle) ? animationStyle 
            : string.IsNullOrEmpty(animationStyle) ? baseStyle 
            : $"{baseStyle}; {animationStyle}";
    }
}
```

### Full-Featured Component
```csharp
public class CraftButton : ThemedCraftComponent
{
    protected override string? GetComponentCssClass() => "craft-button";
}
```
