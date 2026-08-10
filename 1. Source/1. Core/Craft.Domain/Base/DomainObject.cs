namespace Craft.Domain;

/// <summary>
/// Base type for all domain objects.
/// Provides common guard helpers used across entities, value objects,
/// aggregates, domain events, and other domain abstractions.
/// </summary>
public abstract record DomainObject
{
    /// <summary>
    /// Ensures that the supplied condition is true.
    /// </summary>
    protected static void Ensure(bool condition, string message)
    {
        if (!condition)
            throw new ArgumentException(message);
    }

    /// <summary>
    /// Ensures that the supplied string is not null, empty or whitespace.
    /// Returns the trimmed value.
    /// </summary>
    protected static string NotEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName);

        return value.Trim();
    }

    /// <summary>
    /// Ensures that the supplied reference is not null.
    /// </summary>
    protected static T NotNull<T>(T? value, string paramName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }
}
