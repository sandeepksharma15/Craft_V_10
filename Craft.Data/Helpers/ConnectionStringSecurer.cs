using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Craft.Data.Helpers;

public class ConnectionStringSecurer(IOptions<DatabaseOptions> dbSettings) : IConnectionStringSecurer
{
    private const string HiddenValueDefault = "*******";
    private readonly DatabaseOptions _dbOptions = dbSettings.Value;

    //private static string MakeSecureMySqlConnectionString(string connectionString)
    //{
    //    var builder = new MySqlConnectionStringBuilder(connectionString);

    //    if (!string.IsNullOrEmpty(builder.Password))
    //        builder.Password = HiddenValueDefault;

    //    if (!string.IsNullOrEmpty(builder.UserID))
    //        builder.UserID = HiddenValueDefault;

    //    return builder.ToString();
    //}

    private static string MakeSecureNpgsqlConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password))
            builder.Password = HiddenValueDefault;

        if (!string.IsNullOrEmpty(builder.Username))
            builder.Username = HiddenValueDefault;

        return builder.ToString();
    }

    private static string MakeSecureSqlConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString); // Updated to Microsoft.Data.SqlClient.SqlConnectionStringBuilder

        if (!(string.IsNullOrEmpty(builder.Password) && builder.IntegratedSecurity))
            builder.Password = HiddenValueDefault;

        if (!(string.IsNullOrEmpty(builder.UserID) && builder.IntegratedSecurity))
            builder.UserID = HiddenValueDefault;

        return builder.ToString();
    }

    public string MakeSecure(string connectionString, string? dbProvider = null)
    {
        if (connectionString.IsNullOrEmpty())
            return connectionString;

        if (dbProvider.IsNullOrWhiteSpace())
            dbProvider = _dbOptions.DbProvider;

        return dbProvider?.ToLower() switch
        {
            DbProviderKeys.Npgsql => MakeSecureNpgsqlConnectionString(connectionString),
            DbProviderKeys.SqlServer => MakeSecureSqlConnectionString(connectionString),
            // DbProviderKeys.MySql => MakeSecureMySqlConnectionString(connectionString),
            _ => connectionString
        };
    }
}
