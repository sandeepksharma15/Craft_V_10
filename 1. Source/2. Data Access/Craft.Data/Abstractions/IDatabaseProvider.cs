using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

public interface IDatabaseProvider
{
    public const string MsSqlMigrationAssembly = "Migrators.MSSQL";
    public const string PostgreSqlMigrationAssembly = "Migrators.PostgreSQL";
    // public const string MySqlMigrationAssembly = "Migrators.MySQL";

    bool CanHandle(string dbProvider);
    void Configure(DbContextOptionsBuilder builder, string connectionString, DatabaseOptions options);
    bool ValidateConnection(string connectionString);
}

