using Craft.UiComponents;
using Craft.Utilities.Helpers;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components.RunningNumber;

public partial class CraftRunningNumber : CraftComponent, IDisposable
{
    [Parameter] public int TotalTime { get; set; } = 1000;      // In Milliseconds
    [Parameter] public long FirstNumber { get; set; } = 0;
    [Parameter] public long LastNumber { get; set; } = 100;
    [Parameter] public bool UseThousandsSeparator { get; set; } = true;
    [Parameter] public bool UseSmoothEasing { get; set; } = true;

    private CountdownTimer? _countdownTimer;
    private long _currentValue;
    private bool _isInitialized;
    private const int TotalTicks = 60; // Increased for smoother animation

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _currentValue = FirstNumber;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        // Only start timer once
        if (_isInitialized)
            return;
            
        _isInitialized = true;
        InitializeTimer();
    }

    private void InitializeTimer()
    {
        _countdownTimer?.Dispose();
        
        // Convert milliseconds to seconds for CountdownTimer
        var timeoutSeconds = Math.Max(1, TotalTime / 1000);
        _countdownTimer = new CountdownTimer(timeoutSeconds, TotalTicks);
        _countdownTimer.OnTick += UpdateNumber;
        _countdownTimer.OnElapsed += OnAnimationComplete;
        _countdownTimer.Start();
    }

    private async void UpdateNumber(int tickNumber)
    {
        // tickNumber goes from 1 to TotalTicks
        // Convert to progress from 0.0 to 1.0
        var progress = Math.Min(tickNumber / (double)TotalTicks, 1.0);

        // Apply easing if enabled
        if (UseSmoothEasing)
            progress = EaseInOutQuad(progress);

        // Calculate the range
        var range = LastNumber - FirstNumber;

        // Calculate current value based on progress
        var newValue = FirstNumber + (long)(range * progress);
        
        // Only update if value changed to reduce unnecessary renders
        if (newValue != _currentValue)
        {
            _currentValue = newValue;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnAnimationComplete()
    {
        // Ensure we end exactly at LastNumber
        if (_currentValue != LastNumber)
        {
            _currentValue = LastNumber;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Quadratic ease-in-out - smoother and less aggressive than cubic
    /// </summary>
    private static double EaseInOutQuad(double t)
    {
        return t < 0.5
            ? 2 * t * t
            : 1 - Math.Pow(-2 * t + 2, 2) / 2;
    }

    /// <summary>
    /// Cubic ease-in-out - more dramatic easing (kept for reference)
    /// </summary>
    private static double EaseInOutCubic(double t)
    {
        return t < 0.5
            ? 4 * t * t * t
            : 1 - Math.Pow(-2 * t + 2, 3) / 2;
    }

    private string GetFormattedValue()
    {
        if (UseThousandsSeparator)
            return _currentValue.ToString("N0");
        
        return _currentValue.ToString();
    }

    public void Dispose()
    {
        if (_countdownTimer != null)
        {
            _countdownTimer.OnTick -= UpdateNumber;
            _countdownTimer.OnElapsed -= OnAnimationComplete;
            _countdownTimer.Dispose();
            _countdownTimer = null;
        }
    }
}
