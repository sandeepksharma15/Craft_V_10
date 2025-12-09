namespace Craft.Domain;

public interface IHasConcurrency
{
    public const string ColumnName = "ConcurrencyStamp";

    public const int MaxLength = 40;

    string? ConcurrencyStamp { get; set; }

    public string? GetConcurrencyStamp() => ConcurrencyStamp;

    public bool HasConcurrencyStamp() => !string.IsNullOrWhiteSpace(ConcurrencyStamp);

    public void SetConcurrencyStamp(string? stamp = null)
        => ConcurrencyStamp = stamp ?? Guid.NewGuid().ToString();

    public void ClearConcurrencyStamp() => ConcurrencyStamp = null;
}
