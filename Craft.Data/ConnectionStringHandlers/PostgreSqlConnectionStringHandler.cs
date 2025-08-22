
using Npgsql;

namespace Craft.Data;

public class PostgreSqlConnectionStringHandler : IConnectionStringHandler
{
    private const string HiddenValueDefault = "*******";

    public string Build(DatabaseOptions options)
    {
        var builder = new NpgsqlConnectionStringBuilder(options.ConnectionString)
        {
            Timeout = options.CommandTimeout
        };

        return builder.ToString();
    }

    public string Mask(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password))
            builder.Password = HiddenValueDefault;

        if (!string.IsNullOrEmpty(builder.Username))
            builder.Username = HiddenValueDefault;

        return builder.ToString();
    }

    public bool Validate(string connectionString)
    {
        try
        {
            _ = new NpgsqlConnectionStringBuilder(connectionString);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
