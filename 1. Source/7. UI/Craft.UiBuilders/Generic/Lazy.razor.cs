using Craft.UiComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that lazy loads its content when it becomes visible in the viewport.
/// Uses Intersection Observer API for efficient visibility detection.
/// </summary>
public partial class Lazy : CraftComponent
{
    private ElementReference _elementRef;
    private bool _isVisible;
    private IJSObjectReference? _module;
    private DotNetObjectReference<Lazy>? _dotNetRef;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets the threshold for intersection (0.0 to 1.0).
    /// </summary>
    [Parameter]
    public double Threshold { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets the root margin for intersection detection.
    /// </summary>
    [Parameter]
    public string RootMargin { get; set; } = "0px";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                
                // Create inline JS for intersection observer
                var jsCode = @"
                    export function observe(element, dotNetRef, threshold, rootMargin) {
                        const observer = new IntersectionObserver(
                            (entries) => {
                                entries.forEach(entry => {
                                    if (entry.isIntersecting) {
                                        dotNetRef.invokeMethodAsync('OnIntersecting');
                                        observer.disconnect();
                                    }
                                });
                            },
                            { threshold: threshold, rootMargin: rootMargin }
                        );
                        observer.observe(element);
                        return observer;
                    }
                    export function disconnect(observer) {
                        if (observer) observer.disconnect();
                    }
                ";

                _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    $"data:text/javascript;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsCode))}"
                );

                await _module.InvokeVoidAsync("observe", _elementRef, _dotNetRef, Threshold, RootMargin);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize Lazy component");
                // Fallback: show content immediately if JS fails
                _isVisible = true;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    /// <summary>
    /// Called from JavaScript when the element intersects the viewport.
    /// </summary>
    [JSInvokable]
    public async Task OnIntersecting()
    {
        _isVisible = true;
        await InvokeAsync(StateHasChanged);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_module != null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        _dotNetRef?.Dispose();

        await base.DisposeAsyncCore();
    }
}
