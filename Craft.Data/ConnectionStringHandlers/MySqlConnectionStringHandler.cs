namespace Craft.Data;

public class MySqlConnectionStringHandler : IConnectionStringHandler
{
    private const string HiddenValueDefault = "*******";

    public string Build(DatabaseOptions options)
    {
        throw new NotImplementedException();
    }

    public string Mask(string connectionString)
    {
        throw new NotImplementedException();
    }

    public bool Validate(string connectionString)
    {
        throw new NotImplementedException();
    }
}
