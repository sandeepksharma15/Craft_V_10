namespace Craft.Data;

public interface IConnectionStringValidator
{
    bool TryValidate(string connectionString, string dbProvider = null!);
}
