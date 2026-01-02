using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that debounces the rendering of its child content based on a trigger value.
/// Useful for search inputs and other scenarios where you want to delay rendering until input stabilizes.
/// </summary>
/// <typeparam name="TValue">The type of the value to watch.</typeparam>
public partial class Debounce<TValue> : CraftComponent
{
    private Timer? _timer;
    private TValue? _lastValue;
    private bool _shouldRender = true;
    private bool _isInitialized;

    /// <summary>
    /// Gets or sets the value to watch for changes.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the delay in milliseconds before rendering after the value changes.
    /// </summary>
    [Parameter]
    public int DelayMs { get; set; } = 300;

    /// <summary>
    /// Gets or sets the callback invoked after the debounce period when the value has stabilized.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> OnDebounced { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!_isInitialized)
        {
            _isInitialized = true;
            _lastValue = Value;
            return;
        }

        if (!EqualityComparer<TValue>.Default.Equals(Value, _lastValue))
        {
            _lastValue = Value;
            _shouldRender = false;

            _timer?.Dispose();
            _timer = new Timer(async _ =>
            {
                _shouldRender = true;
                await InvokeAsync(async () =>
                {
                    StateHasChanged();
                    if (OnDebounced.HasDelegate)
                    {
                        await OnDebounced.InvokeAsync(Value);
                    }
                });
            }, null, DelayMs, System.Threading.Timeout.Infinite);
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_timer != null)
        {
            await _timer.DisposeAsync();
            _timer = null;
        }

        await base.DisposeAsyncCore();
    }
}
