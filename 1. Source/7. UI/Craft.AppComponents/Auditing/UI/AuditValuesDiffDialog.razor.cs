using System.Text.Json;
using Craft.Auditing;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.AppComponents.Auditing;

public partial class AuditValuesDiffDialog
{
    [CascadingParameter] private IMudDialogInstance? MudDialog { get; set; }

    [Parameter] public string? TableName { get; set; }
    [Parameter] public string? OldValues { get; set; }
    [Parameter] public string? NewValues { get; set; }
    [Parameter] public string? ChangedColumns { get; set; }
    [Parameter] public EntityChangeType ChangeType { get; set; }

    private record DiffRow(string Property, string? OldValue, string? NewValue, bool HasChanged);

    private List<DiffRow> _diffRows = [];

    protected override void OnParametersSet()
    {
        _diffRows = BuildDiffRows();
    }

    private List<DiffRow> BuildDiffRows()
    {
        var oldDict = ParseJsonToDictionary(OldValues);
        var newDict = ParseJsonToDictionary(NewValues);

        var allKeys = oldDict.Keys.Union(newDict.Keys).Order();

        return [.. allKeys.Select(key =>
        {
            oldDict.TryGetValue(key, out var oldVal);
            newDict.TryGetValue(key, out var newVal);

            return new DiffRow(key, oldVal, newVal, oldVal != newVal);
        })];
    }

    private static Dictionary<string, string?> ParseJsonToDictionary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.EnumerateObject()
                .ToDictionary(
                    p => p.Name,
                    p => p.Value.ValueKind == JsonValueKind.Null ? null : p.Value.ToString());
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private Color GetHeaderColor() => ChangeType switch
    {
        EntityChangeType.Created => Color.Success,
        EntityChangeType.Deleted => Color.Error,
        EntityChangeType.Updated => Color.Warning,
        _ => Color.Default
    };

    private string GetChangeTypeLabel() => ChangeType switch
    {
        EntityChangeType.Created => "Created",
        EntityChangeType.Deleted => "Deleted",
        EntityChangeType.Updated => "Updated",
        _ => "Unknown"
    };

    private void Close() => MudDialog?.Close();
}
