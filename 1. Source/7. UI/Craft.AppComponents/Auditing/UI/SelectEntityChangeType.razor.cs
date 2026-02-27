using Craft.Auditing;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.AppComponents.Auditing;

/// <summary>
/// A dropdown component for selecting an <see cref="EntityChangeType"/> value.
/// Entries with an empty description (e.g. <see cref="EntityChangeType.None"/>) are excluded from the list.
/// </summary>
public partial class SelectEntityChangeType
{
    private static readonly Dictionary<EntityChangeType, string> _descriptions =
        EnumExtensions.GetDescriptions<EntityChangeType>()
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    /// <summary>The currently selected change type. Null means no selection.</summary>
    [Parameter] public EntityChangeType? Value { get; set; }

    /// <summary>Callback invoked when the selected change type changes.</summary>
    [Parameter] public EventCallback<EntityChangeType?> ValueChanged { get; set; }

    /// <summary>Label displayed on the dropdown. Defaults to "Action".</summary>
    [Parameter] public string Label { get; set; } = "Action";

    /// <summary>Placeholder text shown when no value is selected.</summary>
    [Parameter] public string Placeholder { get; set; } = "Select an action";

    /// <summary>Input variant. Defaults to <see cref="Variant.Outlined"/>.</summary>
    [Parameter] public Variant Variant { get; set; } = Variant.Outlined;

    /// <summary>Whether the field is required.</summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>Whether the dropdown is read-only.</summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>Whether the dropdown is disabled.</summary>
    [Parameter] public bool Disabled { get; set; }
}
