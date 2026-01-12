# CraftRunningNumber Component

A Blazor component that displays an animated running number with a slot machine style effect, where digits roll vertically from the starting value to the ending value.

## Features

- ? **Slot Machine Animation**: Each digit animates vertically like an odometer or slot machine
- ? **Smooth Easing**: Uses cubic ease-in-out for natural acceleration and deceleration
- ? **Bidirectional**: Supports both counting up and counting down
- ? **Thousand Separators**: Optional comma separators for large numbers
- ? **Negative Numbers**: Full support for negative values
- ? **MudBlazor Integration**: Uses MudText with customizable typography and colors
- ? **Performance**: 60 FPS animation with hardware-accelerated CSS transforms
- ? **Proper Disposal**: Automatically cleans up timer resources

## Basic Usage

```razor
<CraftRunningNumber 
    FirstNumber="0" 
    LastNumber="100" 
    TotalTime="2" 
    TextType="Typo.h3" 
    TextColor="Color.Primary" />
```

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `FirstNumber` | `long` | `0` | The starting value of the animation |
| `LastNumber` | `long` | `100` | The ending value where animation stops |
| `TotalTime` | `int` | `1` | Duration of the animation in seconds |
| `TextType` | `Typo` | `Typo.h5` | MudBlazor typography variant |
| `TextColor` | `Color` | `Color.Primary` | MudBlazor color for the text |
| `UseThousandsSeparator` | `bool` | `true` | Whether to display thousand separators (commas) |

## Examples

### Count Up
```razor
<CraftRunningNumber 
    FirstNumber="0" 
    LastNumber="1000" 
    TotalTime="3" />
```

### Count Down
```razor
<CraftRunningNumber 
    FirstNumber="100" 
    LastNumber="0" 
    TotalTime="2" 
    TextColor="Color.Error" />
```

### Large Numbers with Separators
```razor
<CraftRunningNumber 
    FirstNumber="0" 
    LastNumber="1000000" 
    TotalTime="5" 
    TextType="Typo.h2" 
    UseThousandsSeparator="true" />
```

### Currency Display
```razor
<div style="display: flex; align-items: center; gap: 8px;">
    <MudText Typo="Typo.h3">$</MudText>
    <CraftRunningNumber 
        FirstNumber="0" 
        LastNumber="5750000" 
        TotalTime="4" 
        TextType="Typo.h3" />
</div>
```

### Percentage
```razor
<div style="display: flex; align-items: center; gap: 8px;">
    <CraftRunningNumber 
        FirstNumber="0" 
        LastNumber="98" 
        TotalTime="2" 
        UseThousandsSeparator="false" />
    <MudText Typo="Typo.h3">%</MudText>
</div>
```

### Negative Numbers
```razor
<CraftRunningNumber 
    FirstNumber="-25" 
    LastNumber="35" 
    TotalTime="3" />
```

## Animation Details

### Easing Function
The component uses a cubic ease-in-out easing function for smooth, natural-looking animation:
- **Start**: Slow acceleration
- **Middle**: Fast movement
- **End**: Smooth deceleration

### Frame Rate
- Runs at 60 FPS for smooth transitions
- Uses CSS transforms for hardware acceleration
- Minimal CPU usage

### Digit Animation
Each digit position animates independently:
- When a digit changes from 0?1?2...?9, it scrolls vertically
- When reaching 9 and rolling over to 0, the next higher digit also increments
- All digits synchronize to reach the target value at the same time

## Best Practices

### Choosing Animation Duration

Match the `TotalTime` to your number range for optimal visual effect:

| Number Range | Recommended Duration |
|--------------|---------------------|
| 0 - 100 | 1-2 seconds |
| 0 - 10,000 | 2-3 seconds |
| 0 - 1,000,000 | 3-5 seconds |

### Performance Considerations

- The component automatically disposes of timer resources
- Uses CSS transforms for GPU-accelerated animations
- Suitable for multiple instances on the same page

### Accessibility

- Consider users who may be sensitive to motion
- Don't overuse animations on a single page
- For dashboard statistics, consider staggering start times

### Styling

The component supports all standard `CraftComponent` properties:
- `Class`: Add custom CSS classes
- `Style`: Add inline styles
- `Id`: Set a unique identifier

## Dashboard Example

```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="3">
        <MudPaper Class="pa-4" Elevation="3">
            <MudText Typo="Typo.body2" Class="mb-2">Total Users</MudText>
            <CraftRunningNumber 
                FirstNumber="0" 
                LastNumber="12543" 
                TotalTime="3" />
        </MudPaper>
    </MudItem>
    
    <MudItem xs="12" sm="6" md="3">
        <MudPaper Class="pa-4" Elevation="3">
            <MudText Typo="Typo.body2" Class="mb-2">Revenue</MudText>
            <div style="display: flex; align-items: center; gap: 4px;">
                <MudText Typo="Typo.h5">$</MudText>
                <CraftRunningNumber 
                    FirstNumber="0" 
                    LastNumber="125890" 
                    TotalTime="4" />
            </div>
        </MudPaper>
    </MudItem>
</MudGrid>
```

## Technical Details

### Component Inheritance
- Inherits from `CraftComponent`
- Implements `IDisposable` for proper cleanup
- Supports all base component features (theming, logging, etc.)

### CSS Isolation
The component uses scoped CSS to prevent style conflicts. All styles are prefixed with the component name.

### Browser Compatibility
- Modern browsers (Chrome, Firefox, Safari, Edge)
- Uses CSS transforms and transitions (widely supported)
- Fallback: Static display if transforms not supported

## See Also

- [Example.txt](./Example.txt) - More detailed examples
- [MudBlazor Documentation](https://mudblazor.com/) - Typography and color options
