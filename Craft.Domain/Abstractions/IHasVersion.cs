namespace Craft.Domain;

public interface IHasVersion
{
    public const string ColumnName = "Version";

    public long Version { get; set; }

    public void DecrementVersion()
    {
        if (Version > 0)
            Version--;
    }

    public long GetVersion() => Version;

    public void IncrementVersion() => Version++;

    public void SetVersion(long version) => Version = version;
}
