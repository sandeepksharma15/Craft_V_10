using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that delays the rendering of its content for a specified duration.
/// Useful for progressive loading patterns and reducing visual clutter.
/// </summary>
public partial class Delay : CraftComponent
{
    private bool _isVisible;
    private Timer? _timer;

    /// <summary>
    /// Gets or sets the delay in milliseconds before showing the content.
    /// </summary>
    [Parameter]
    public int Milliseconds { get; set; } = 500;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _timer = new Timer(_ =>
        {
            _isVisible = true;
            InvokeAsync(StateHasChanged);
        }, null, Milliseconds, System.Threading.Timeout.Infinite);
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
