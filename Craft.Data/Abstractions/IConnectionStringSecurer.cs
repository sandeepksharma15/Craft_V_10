namespace Craft.Data;

public interface IConnectionStringSecurer
{
    string MakeSecure(string connectionString, string dbProvider = null!);
}
