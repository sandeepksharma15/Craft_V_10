namespace Craft.Data;

/// <summary>
/// Supported database providers for Entity Framework Core features.
/// Used for provider-specific SQL syntax in filtered indexes and other database features.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// Auto-detect the database provider from DbContext configuration.
    /// Falls back to PostgreSQL if detection fails.
    /// </summary>
    AutoDetect = 0,

    /// <summary>
    /// Microsoft SQL Server
    /// </summary>
    SqlServer = 1,

    /// <summary>
    /// PostgreSQL (via Npgsql)
    /// </summary>
    PostgreSql = 2,

    /// <summary>
    /// MySQL or MariaDB
    /// </summary>
    MySql = 3,

    /// <summary>
    /// SQLite (limited support for filtered indexes)
    /// </summary>
    Sqlite = 4
}
