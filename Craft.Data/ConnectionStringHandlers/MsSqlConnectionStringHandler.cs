using Microsoft.Data.SqlClient;

namespace Craft.Data;

/// <summary>
/// Handles build, validation and masking operations for Microsoft SQL Server connection strings.
/// </summary>
public sealed class MsSqlConnectionStringHandler : IConnectionStringHandler
{
    private const string HiddenValueDefault = "*******";

    /// <summary>
    /// Builds a normalized SQL Server connection string applying the provided <see cref="DatabaseOptions"/>.
    /// </summary>
    /// <param name="options">The database options containing a base connection string and timeout.</param>
    /// <returns>The normalized connection string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the base connection string is null/empty or invalid.</exception>
    public string Build(DatabaseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString, nameof(options));

        SqlConnectionStringBuilder builder;

        try
        {
            builder = new SqlConnectionStringBuilder(options.ConnectionString);
        }
        catch (Exception ex)
        {
            // Surface a meaningful exception instead of letting low-level parsing bubble up unhandled.
            throw new ArgumentException("Invalid SQL Server connection string provided in DatabaseOptions.", nameof(options), ex);
        }

        // Only override when a positive timeout is supplied (0 can mean use default driver value)
        if (options.CommandTimeout > 0)
            builder.ConnectTimeout = options.CommandTimeout;

        return builder.ConnectionString;
    }

    /// <summary>
    /// Returns a masked representation of the supplied connection string with credential fields obfuscated.
    /// </summary>
    /// <param name="connectionString">The raw connection string.</param>
    /// <returns>The masked connection string if parsable; otherwise the original value.</returns>
    public string Mask(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            // Only mask credential fields when SQL authentication is in use (Integrated Security = false)
            if (!builder.IntegratedSecurity)
            {
                // Always mask the password (even if empty) to avoid revealing absence vs presence
                builder.Password = HiddenValueDefault;

                if (!string.IsNullOrEmpty(builder.UserID))
                    builder.UserID = HiddenValueDefault;
            }

            return builder.ConnectionString;
        }
        catch
        {
            // If the string cannot be parsed, do not modify / risk exposing partial info.
            return connectionString;
        }
    }

    /// <summary>
    /// Validates that the provided connection string can be parsed by <see cref="SqlConnectionStringBuilder"/>.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns><c>true</c> when valid; otherwise <c>false</c>.</returns>
    public bool Validate(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

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
