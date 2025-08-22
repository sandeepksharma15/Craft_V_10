namespace Craft.Data;

public interface IConnectionStringHandler
{
    string Build(DatabaseOptions options);
    bool Validate(string connectionString);
    string Mask(string connectionString);
}
