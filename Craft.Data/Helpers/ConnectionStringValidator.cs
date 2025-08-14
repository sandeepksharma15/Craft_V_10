using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Craft.Data.Helpers;

public class ConnectionStringValidator(IOptions<DatabaseOptions> dbSettings,
    ILogger<ConnectionStringValidator> logger) : IConnectionStringValidator
{
    private readonly DatabaseOptions _dbOptions = dbSettings.Value;
    private readonly ILogger<ConnectionStringValidator> _logger = logger;

    public bool TryValidate(string connectionString, string? dbProvider = null)
    {
        if (string.IsNullOrWhiteSpace(dbProvider))
            dbProvider = _dbOptions.DbProvider;

        try
        {
            switch (dbProvider?.ToLowerInvariant())
            {
                case DbProviderKeys.Npgsql:
                    _ = new NpgsqlConnectionStringBuilder(connectionString);
                    break;

                //case DbProviderKeys.MySql:
                //    _ = new MySqlConnectionStringBuilder(connectionString);
                //    break;

                case DbProviderKeys.SqlServer:
                    _ = new SqlConnectionStringBuilder(connectionString);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connection String Validation Exception : {ex.Message}");
            return false;
        }
    }
}
