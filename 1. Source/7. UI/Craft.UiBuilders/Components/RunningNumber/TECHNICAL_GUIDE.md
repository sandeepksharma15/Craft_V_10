# CraftRunningNumber - Animation Technical Guide

## How the Slot Machine Animation Works

### Visual Representation

```
???????????????????????????????????????????
?  Final Display: 1,234                   ?
?                                         ?
?  ?????  ?????  ?????  ?????           ?
?  ? 1 ? ,? 2 ?  ? 3 ?  ? 4 ?           ?
?  ?????  ?????  ?????  ?????           ?
???????????????????????????????????????????
```

### Each Digit Container (Scrollable)

```
???????????  ? Visible Window (height: 1em)
?    2    ?  ? Currently Visible Digit
???????????
?    0    ?  ?
?    1    ?  ?
?    2    ?  ? Transform: translateY(-2em) shows this
?    3    ?  ?
?    4    ?  ?
?    5    ?  
?    6    ?  
?    7    ?  
?    8    ?  
?    9    ?  
???????????
```

## Animation Flow

### Step 1: Initialize
```
FirstNumber = 0
LastNumber = 1234
TotalTime = 3 seconds
```

### Step 2: Calculate Progress
```
Frame Rate: 60 FPS (every ~16ms)
Timer Tick ? Calculate Progress (0.0 to 1.0)
Apply Easing Function
```

### Step 3: Update Current Value
```
Progress: 0.0   ? Value: 0      ? Display: 0,000
Progress: 0.25  ? Value: 308    ? Display: 0,308
Progress: 0.50  ? Value: 617    ? Display: 0,617
Progress: 0.75  ? Value: 925    ? Display: 0,925
Progress: 1.0   ? Value: 1234   ? Display: 1,234
```

### Step 4: Break Into Digits
```
Value: 617

Thousands: 0
Hundreds: 6
Tens: 1
Ones: 7
```

### Step 5: Render Each Digit
```html
<div class="digit-scroll" data-value="6">
  <div class="digit-item">0</div>
  <div class="digit-item">1</div>
  <div class="digit-item">2</div>
  <div class="digit-item">3</div>
  <div class="digit-item">4</div>
  <div class="digit-item">5</div>
  <div class="digit-item">6</div> ? Visible
  <div class="digit-item">7</div>
  <div class="digit-item">8</div>
  <div class="digit-item">9</div>
</div>
```

### Step 6: CSS Transform
```css
.digit-scroll[data-value="6"] {
  transform: translateY(-6em); /* Shows digit 6 */
  transition: transform 0.3s cubic-bezier(0.4, 0.0, 0.2, 1);
}
```

## Easing Function Explained

### Cubic Ease-In-Out

```
EaseInOutCubic(t)
  if t < 0.5:
    return 4 × t³
  else:
    return 1 - ((-2t + 2)³ / 2)
```

### Visualization

```
 1.0 ?                    ??????????
     ?                  ??
     ?               ???
     ?            ???
 0.5 ?         ???
     ?      ???
     ?    ??
     ?  ??
 0.0 ???????????????????????????????
     0.0                         1.0
          Time Progress
```

**Effect:**
- Slow start (0.0 - 0.25): Smooth acceleration
- Fast middle (0.25 - 0.75): Quick movement
- Slow end (0.75 - 1.0): Smooth deceleration

## Example: Animating from 0 to 100

### Frame-by-Frame (at 60 FPS over 1 second)

```
Frame | Time    | Progress | Eased  | Value | Tens | Ones
------|---------|----------|--------|-------|------|------
  0   | 0.00s   | 0.000    | 0.000  |   0   |  0   |  0
  6   | 0.10s   | 0.100    | 0.004  |   0   |  0   |  0
 15   | 0.25s   | 0.250    | 0.063  |   6   |  0   |  6
 30   | 0.50s   | 0.500    | 0.500  |  50   |  5   |  0
 45   | 0.75s   | 0.750    | 0.937  |  94   |  9   |  4
 54   | 0.90s   | 0.900    | 0.996  |  99   |  9   |  9
 60   | 1.00s   | 1.000    | 1.000  | 100   |  1   |  0
```

### Visual Progression

```
Time: 0.00s    Time: 0.25s    Time: 0.50s    Time: 0.75s    Time: 1.00s
   ?????         ?????          ?????          ?????          ?????
   ? 0 ?         ? 0 ?          ? 5 ?          ? 9 ?          ? 1 ?
   ?????         ?????          ?????          ?????          ?????
   ?????         ?????          ?????          ?????          ?????
   ? 0 ?         ? 6 ?          ? 0 ?          ? 4 ?          ? 0 ?
   ?????         ?????          ?????          ?????          ?????
     0              6             50             94            100
```

## Counting Down (Reverse)

When `FirstNumber > LastNumber`, the component counts down:

```
FirstNumber = 100
LastNumber = 0

Frame | Progress | Value
------|----------|-------
  0   | 0.000    | 100
 15   | 0.250    |  94
 30   | 0.500    |  50
 45   | 0.750    |   6
 60   | 1.000    |   0
```

## Negative Numbers

When negative, a minus sign is rendered before the digits:

```
Value: -42

Display: - 4 2
         ? ? ?
         ? ? ?? Ones digit (animated)
         ? ???? Tens digit (animated)
         ?????? Static minus sign
```

## Thousand Separators

For large numbers, commas are inserted:

```
Value: 1234567

Position:  6 5 4 3 2 1 0
Digit:     1 2 3 4 5 6 7
Comma:       ?   ?   ?
             
Display: 1,234,567

Logic: Insert comma before positions 3, 6, 9, etc.
```

## Performance Optimization

### Hardware Acceleration
```css
.digit-scroll {
  will-change: transform;
  /* Tells browser to optimize this property */
}
```

### Minimal Reflows
- Only transforms change (not layout)
- `overflow: hidden` prevents repaints of hidden digits
- CSS transitions handled by GPU

### Timer Efficiency
```csharp
// 60 FPS = ~16.67ms per frame
var intervalMs = 1000 / FramesPerSecond;
_animationTimer = new Timer(callback, null, intervalMs, intervalMs);
```

### Early Termination
```csharp
if (progress >= 1.0)
{
    _currentValue = LastNumber;
    _animationTimer?.Dispose(); // Stop immediately
}
```

## Browser Rendering Pipeline

```
1. Timer Tick (JavaScript/C#)
   ?
2. Update State (_currentValue changes)
   ?
3. StateHasChanged() called
   ?
4. Blazor Re-renders Markup
   ?
5. data-value attribute changes
   ?
6. CSS selector matches
   ?
7. CSS transition starts
   ?
8. GPU handles transform animation
   ?
9. User sees smooth digit scroll
```

## Common Animation Patterns

### Dashboard Statistics
```
Stagger start times for visual appeal:
- Users count: Start at 0ms
- Orders count: Start at 200ms
- Revenue count: Start at 400ms
- Sessions count: Start at 600ms
```

### Currency Display
```
Wrap in container with currency symbol:
<div class="currency">
  <span>$</span>
  <CraftRunningNumber ... />
</div>
```

### Percentage
```
<div class="percentage">
  <CraftRunningNumber LastNumber="98" ... />
  <span>%</span>
</div>
```

## Debugging Tips

### Check Current Value
```razor
<p>Current Value: @_currentValue</p>
<CraftRunningNumber ... />
```

### Monitor Progress
```csharp
LogDebug("Progress: {Progress}, Value: {Value}", progress, _currentValue);
```

### Verify Digit Calculation
```csharp
var digits = GetDigitInfos();
foreach (var digit in digits)
{
    LogDebug("Position: {Pos}, Value: {Val}, Comma: {Comma}", 
        digit.Position, digit.Value, digit.NeedsComma);
}
```

## CSS Class Reference

```
.craft-running-number     ? Root container
.number-container         ? Holds all digits and separators
.digit-wrapper            ? Individual digit container (overflow hidden)
.digit-scroll             ? Scrollable list of 0-9
.digit-item               ? Single digit element
.digit-static             ? Non-animated character (-, comma)
```

## Memory Management

```csharp
public void Dispose()
{
    _animationTimer?.Dispose();  // Stop timer
    _animationTimer = null;       // Release reference
}
```

**Called automatically when:**
- Component is removed from page
- User navigates away
- Parent component disposes

## Summary

The CraftRunningNumber component creates smooth, visually appealing animations by:
1. ? Using a high-frequency timer (60 FPS)
2. ? Applying mathematical easing for natural motion
3. ? Breaking numbers into individual digits
4. ? Using CSS transforms for GPU acceleration
5. ? Synchronizing all digits to complete simultaneously
6. ? Properly cleaning up resources on disposal

This creates the classic "slot machine" or "odometer" effect where digits roll vertically to their target values!
