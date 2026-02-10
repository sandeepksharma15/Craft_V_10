using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

/// <summary>
/// Defines database provider-specific configuration and connection management.
/// </summary>
public interface IDatabaseProvider
{
    public const string MsSqlMigrationAssembly = "Migrators.MSSQL";
    public const string PostgreSqlMigrationAssembly = "Migrators.PostgreSQL";
    public const string MySqlMigrationAssembly = "Migrators.MySQL";

    /// <summary>
    /// Determines if this provider can handle the specified database provider key.
    /// </summary>
    /// <param name="dbProvider">The database provider key (e.g., "mssql", "postgresql").</param>
    /// <returns>True if this provider can handle the specified key; otherwise false.</returns>
    bool CanHandle(string dbProvider);

    /// <summary>
    /// Configures the DbContext options builder with provider-specific settings.
    /// </summary>
    /// <param name="builder">The options builder to configure.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="options">Database configuration options.</param>
    void Configure(DbContextOptionsBuilder builder, string connectionString, DatabaseOptions options);

    /// <summary>
    /// Validates that a connection can be established using the provided connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns>True if connection is valid; otherwise false.</returns>
    bool ValidateConnection(string connectionString);

    /// <summary>
    /// Tests connection resilience by attempting to connect with retry logic.
    /// Returns detailed connection test results including latency and errors.
    /// </summary>
    /// <param name="connectionString">The connection string to test.</param>
    /// <param name="timeout">Optional timeout for the test (default: 5 seconds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connection test result with latency and error information.</returns>
    Task<ConnectionTestResult> TestConnectionAsync(
        string connectionString,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ConnectionTestResult
        {
            IsSuccessful = ValidateConnection(connectionString),
            Provider = GetType().Name,
            Message = ValidateConnection(connectionString) ? "Connection successful" : "Connection failed"
        });
    }
}

/// <summary>
/// Represents the result of a database connection test.
/// </summary>
public class ConnectionTestResult
{
    /// <summary>
    /// Indicates whether the connection test was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// The database provider that performed the test.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Connection latency in milliseconds (if successful).
    /// </summary>
    public double? LatencyMs { get; set; }

    /// <summary>
    /// Error message if the connection failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Detailed message about the test result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Server version information (if available).
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    /// Database name (if available).
    /// </summary>
    public string? DatabaseName { get; set; }
}

