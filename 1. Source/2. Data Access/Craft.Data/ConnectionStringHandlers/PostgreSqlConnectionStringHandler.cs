using Npgsql;

namespace Craft.Data;

/// <summary>
/// Handles build, validation and masking operations for PostgreSQL connection strings.
/// </summary>
public sealed class PostgreSqlConnectionStringHandler : IConnectionStringHandler
{
    /// <summary>
    /// Builds a normalized PostgreSQL connection string using provided options.
    /// </summary>
    /// <param name="options">Database options containing a base connection string and timeout.</param>
    /// <returns>Normalized connection string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when connection string is null/empty or invalid.</exception>
    public string Build(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        NpgsqlConnectionStringBuilder builder;

        try
        {
#pragma warning disable IDE0028 // Simplify collection initialization
            builder = new NpgsqlConnectionStringBuilder(connectionString);
#pragma warning restore IDE0028 // Simplify collection initialization
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Invalid PostgreSQL connection string provided", nameof(connectionString), ex);
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Returns a masked connection string with credential fields obfuscated.
    /// </summary>
    /// <param name="connectionString">Raw connection string.</param>
    /// <returns>Masked connection string if valid; otherwise original string.</returns>
    public string Mask(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);

            if (!string.IsNullOrEmpty(builder.Username))
                builder.Username = IConnectionStringHandler.HiddenValueDefault;

            // Always mask the password (even if empty) to avoid revealing absence vs presence
            builder.Password = IConnectionStringHandler.HiddenValueDefault;

            return builder.ConnectionString;
        }
        catch
        {
            return connectionString; // If parsing fails, return original to avoid accidental alteration.
        }
    }

    /// <summary>
    /// Validates whether the provided connection string can be parsed.
    /// </summary>
    /// <param name="connectionString">Connection string to validate.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public bool Validate(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

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

