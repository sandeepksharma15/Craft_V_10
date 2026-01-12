using Craft.UiComponents;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Components.RunningNumber;

public partial class CraftRunningNumber : CraftComponent, IDisposable
{
    [Parameter] public int TotalTime { get; set; } = 1;         // In Seconds
    [Parameter] public long FirstNumber { get; set; } = 0;
    [Parameter] public long LastNumber { get; set; } = 100;
    [Parameter] public Typo TextType { get; set; } = Typo.h5;
    [Parameter] public Color TextColor { get; set; } = Color.Primary;
    [Parameter] public bool UseThousandsSeparator { get; set; } = true;

    private Timer? _animationTimer;
    private DateTime _startTime;
    private long _currentValue;
    private int _digitCount;
    private bool _isCountingDown;
    private const int FramesPerSecond = 60;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _currentValue = FirstNumber;
        _isCountingDown = FirstNumber > LastNumber;
        
        // Calculate digit count based on the larger absolute value
        var maxValue = Math.Max(Math.Abs(FirstNumber), Math.Abs(LastNumber));
        _digitCount = maxValue == 0 ? 1 : (int)Math.Floor(Math.Log10(maxValue)) + 1;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            StartAnimation();
        }
    }

    private void StartAnimation()
    {
        _startTime = DateTime.UtcNow;
        var intervalMs = 1000 / FramesPerSecond;
        
        _animationTimer = new Timer(_ =>
        {
            UpdateValue();
            InvokeAsync(StateHasChanged);
        }, null, intervalMs, intervalMs);
    }

    private void UpdateValue()
    {
        var elapsed = DateTime.UtcNow - _startTime;
        var progress = Math.Min(elapsed.TotalSeconds / TotalTime, 1.0);
        
        // Apply smooth easing (ease-in-out)
        var easedProgress = EaseInOutCubic(progress);
        
        // Calculate current value based on direction
        if (_isCountingDown)
        {
            _currentValue = FirstNumber - (long)((FirstNumber - LastNumber) * easedProgress);
        }
        else
        {
            _currentValue = FirstNumber + (long)((LastNumber - FirstNumber) * easedProgress);
        }
        
        // Stop animation when complete
        if (progress >= 1.0)
        {
            _currentValue = LastNumber;
            _animationTimer?.Dispose();
            _animationTimer = null;
        }
    }

    private static double EaseInOutCubic(double t)
    {
        return t < 0.5 
            ? 4 * t * t * t 
            : 1 - Math.Pow(-2 * t + 2, 3) / 2;
    }

    private List<DigitInfo> GetDigitInfos()
    {
        var digits = new List<DigitInfo>();
        var absValue = Math.Abs(_currentValue);
        var valueStr = absValue.ToString().PadLeft(_digitCount, '0');
        
        for (int i = 0; i < valueStr.Length; i++)
        {
            var digit = int.Parse(valueStr[i].ToString());
            var position = valueStr.Length - i - 1;
            
            // Add comma separator before this digit if needed
            var needsComma = UseThousandsSeparator && position > 0 && position % 3 == 0;
            
            digits.Add(new DigitInfo
            {
                Value = digit,
                Position = position,
                NeedsComma = needsComma
            });
        }
        
        return digits;
    }

    private string GetFormattedNumber()
    {
        var format = UseThousandsSeparator ? "N0" : "F0";
        return _currentValue.ToString(format);
    }

    public void Dispose()
    {
        _animationTimer?.Dispose();
        _animationTimer = null;
    }

    private class DigitInfo
    {
        public int Value { get; set; }
        public int Position { get; set; }
        public bool NeedsComma { get; set; }
    }
}
