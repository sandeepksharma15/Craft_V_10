using Microsoft.AspNetCore.Components;

namespace Craft.UiConponents;

public abstract class CraftComponent : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public ElementReference ElementRef { get; set; }
    [Parameter] public Action<ElementReference>? ElementRefChanged { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public object? Tag { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? UserAttributes { get; set; }
    [Parameter] public virtual bool Visible { get; set; } = true;

    protected override void OnInitialized()
    {
        Id ??= Guid.NewGuid().ToString("N")[..10];
    }

    /// <summary>
    /// Gets the component's ID, prioritizing the user-defined "id" attribute over the default Id property.
    /// </summary>
    /// <returns>The component's ID as a string.</returns>
    protected internal string GetId()
    {
        if (UserAttributes?.TryGetValue("id", out var id) == true)
        {
            var idString = Convert.ToString(id);

            if (!idString.IsNullOrEmpty())
                return idString!;
        }

        return Id ?? string.Empty;
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        parameters.TryGetValue(nameof(Visible), out bool visibleState);

        await base.SetParametersAsync(parameters);

        if (Visible != visibleState)
            StateHasChanged();
    }
}
