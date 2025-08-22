using Microsoft.Data.SqlClient;

namespace Craft.Data;

public class MsSqlConnectionStringHandler : IConnectionStringHandler
{
    private const string HiddenValueDefault = "*******";

    public string Build(DatabaseOptions options)
    {
        var builder = new SqlConnectionStringBuilder(options.ConnectionString)
        {
            ConnectTimeout = options.CommandTimeout
        };

        return builder.ToString();
    }

    public string Mask(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (!(string.IsNullOrEmpty(builder.Password) && builder.IntegratedSecurity))
            builder.Password = HiddenValueDefault;

        if (!(string.IsNullOrEmpty(builder.UserID) && builder.IntegratedSecurity))
            builder.UserID = HiddenValueDefault;

        return builder.ToString();
    }

    public bool Validate(string connectionString)
    {
        try
        {
            _ = new SqlConnectionStringBuilder(connectionString);

            return true;
        }
        catch 
        { 
            return false; 
        }
    }
}
