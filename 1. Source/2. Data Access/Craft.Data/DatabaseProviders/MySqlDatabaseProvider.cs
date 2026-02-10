using Microsoft.EntityFrameworkCore;

namespace Craft.Value.DatabaseProviders;

public class MySqlDatabaseProvider : IDatabaseProvider
{
    public bool CanHandle(string dbProvider)
    {
        throw new NotImplementedException();
    }

    public void Configure(DbContextOptionsBuilder builder, string connectionString, DatabaseOptions options)
    {
        throw new NotImplementedException();
    }

    public bool ValidateConnection(string connectionString)
    {
        throw new NotImplementedException();
    }
}

