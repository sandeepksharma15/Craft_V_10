# CraftRunningNumber Component

A simple, lightweight Blazor component that animates a number from a starting value to an ending value.

## Quick Start

```razor
<MudText Typo="Typo.h2" Color="Color.Primary">
    <CraftRunningNumber FirstNumber="0" LastNumber="1234" TotalTime="3" />
</MudText>
```

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `FirstNumber` | `long` | `0` | Starting value |
| `LastNumber` | `long` | `100` | Ending value |
| `TotalTime` | `int` | `1` | Animation duration in seconds |
| `UseThousandsSeparator` | `bool` | `true` | Format with commas (culture-specific) |
| `UseSmoothEasing` | `bool` | `true` | Use smooth easing or linear |

## Examples

### Dashboard Card
```razor
<DashboardCard Icon="@Icons.Material.Filled.LocationOn" Title="Locations">
    <MudText Typo="Typo.h2" Color="Color.Primary">
        <CraftRunningNumber FirstNumber="0" LastNumber="@_locationCount" TotalTime="2" />
    </MudText>
</DashboardCard>
```

### With Currency
```razor
<div class="d-flex align-center">
    <MudText Typo="Typo.h3">$</MudText>
    <MudText Typo="Typo.h3">
        <CraftRunningNumber FirstNumber="0" LastNumber="5000000" TotalTime="3" />
    </MudText>
</div>
```

### Count Down
```razor
<MudText Typo="Typo.h3">
    <CraftRunningNumber FirstNumber="100" LastNumber="0" TotalTime="2" />
</MudText>
```

## Features

- ? Smooth animation with optional easing
- ? Count up or down automatically detected
- ? Culture-aware thousand separators
- ? Negative number support
- ? Lightweight (uses CountdownTimer)
- ? Proper resource disposal

## Notes

- Wrap in any MudBlazor typography component for styling
- Animation starts automatically on first render
- Timer disposes automatically when component is removed
- For large numbers (>10,000), use TotalTime of 3-5 seconds for best effect
