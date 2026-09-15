using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Components;

public partial class HexInput
{
    private string _displayValue = string.Empty;

    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public Expression<Func<string>>? ValueExpression { get; set; }
    [Parameter] public Expression<Func<string>>? For { get; set; }
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public Variant Variant { get; set; } = Variant.Outlined;
    [Parameter] public bool Required { get; set; }
    [Parameter] public string RequiredError { get; set; } = "This field is required.";
    [Parameter] public string? HelperText { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public int MaxLength { get; set; } = 32;
    [Parameter] public int MinLength { get; set; }
    [Parameter] public bool FormatAsBytePairs { get; set; } = true;
    [Parameter] public bool RequireEvenLength { get; set; } = true;

    private int DisplayMaxLength => FormatAsBytePairs ? GetFormattedLength(MaxLength) : MaxLength;
    private Expression<Func<string>>? FieldExpression => For ?? ValueExpression;

    protected override void OnParametersSet()
    {
        _displayValue = FormatValue(Value);
    }

    private async Task HandleValueChangedAsync(string? input)
    {
        var normalizedValue = NormalizeValue(input);
        _displayValue = FormatValue(normalizedValue);

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(normalizedValue);
    }

    private string FormatValue(string? value)
    {
        var normalizedValue = NormalizeValue(value);

        if (!FormatAsBytePairs || string.IsNullOrEmpty(normalizedValue))
            return normalizedValue;

        var builder = new StringBuilder(GetFormattedLength(normalizedValue.Length));

        for (var index = 0; index < normalizedValue.Length; index++)
        {
            if (index > 0 && index % 2 == 0)
                builder.Append(' ');

            builder.Append(normalizedValue[index]);
        }

        return builder.ToString();
    }

    private string NormalizeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || MaxLength <= 0)
            return string.Empty;

        var builder = new StringBuilder(Math.Min(value.Length, MaxLength));

        foreach (var character in value)
        {
            if (!Uri.IsHexDigit(character))
                continue;

            builder.Append(char.ToUpperInvariant(character));

            if (builder.Length >= MaxLength)
                break;
        }

        return builder.ToString();
    }

    private static int GetFormattedLength(int rawLength)
    {
        if (rawLength <= 1)
            return Math.Max(rawLength, 0);

        return rawLength + ((rawLength - 1) / 2);
    }
}
