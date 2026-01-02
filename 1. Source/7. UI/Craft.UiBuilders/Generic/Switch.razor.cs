using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that renders different content based on matching a value against multiple cases.
/// Similar to a switch statement in C#.
/// </summary>
/// <typeparam name="TValue">The type of the value to match.</typeparam>
public partial class Switch<TValue> : CraftComponent
{
    /// <summary>
    /// Gets or sets the value to match against the cases.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the collection of case components.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public List<Case<TValue>> Cases { get; set; } = [];

    /// <summary>
    /// Gets or sets the default content to render when no case matches.
    /// </summary>
    [Parameter]
    public RenderFragment? Default { get; set; }
}

/// <summary>
/// Represents a single case in a Switch component.
/// </summary>
/// <typeparam name="TValue">The type of the value to match.</typeparam>
public class Case<TValue> : ComponentBase
{
    /// <summary>
    /// Gets or sets the value to match.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public TValue? When { get; set; }

    /// <summary>
    /// Gets or sets the content to render when this case matches.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [CascadingParameter]
    private Switch<TValue>? Parent { get; set; }

    protected override void OnInitialized()
    {
        Parent?.Cases.Add(this);
    }
}
