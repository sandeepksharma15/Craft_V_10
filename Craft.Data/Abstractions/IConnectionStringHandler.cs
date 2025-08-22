namespace Craft.Data;

public interface IConnectionStringHandler
{
    /// <summary>
    /// Standard masked value used to obfuscate sensitive connection string fields (usernames, passwords, keys).
    /// </summary>
    public const string HiddenValueDefault = "*******";

    string Build(DatabaseOptions options);
    bool Validate(string connectionString);
    string Mask(string connectionString);
}
