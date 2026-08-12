using System.Text.RegularExpressions;

namespace Craft.Domain;


/// <summary>
/// Contract for string-based value objects that can create themselves.
/// </summary>
public interface IStringValueObject<TSelf> where TSelf : StringValueObject<TSelf>, IStringValueObject<TSelf>
{
    static abstract TSelf Create(string value);
}

/// <summary>
/// Base type for immutable string-based value objects.
/// Provides normalization, validation, parsing and common conversions.
/// </summary>
public abstract class StringValueObject<TSelf> : SingleValueObject<string>, IParsable<TSelf>
    where TSelf : StringValueObject<TSelf>, IStringValueObject<TSelf>
{
    protected StringValueObject(string value) : base(value)
    {
        // Avoid mutating it after the base constructor has run
        //string normalized = Normalize(NotEmpty(value, nameof(value)));
        //Ensure(ValidationExpression.IsMatch(normalized), ValidationMessage);
        //Value = normalized;
        //Value = value;
    }

    /// <summary>
    /// Regular expression used to validate the value.
    /// </summary>
    protected abstract Regex ValidationExpression { get; }

    /// <summary>
    /// Error message returned when validation fails.
    /// </summary>
    protected abstract string ValidationMessage { get; }

    /// <summary>
    /// Normalizes the value before validation and storage.
    /// </summary>
    protected virtual string Normalize(string value)
        => value;

    private static string ValidateAndNormalize(string value) 
    { 
        value = NotEmpty(value, nameof(value)); 
        return value; 
    }

    protected string NormalizeAndValidate(string value) 
    { 
        var normalized = Normalize(value); 
        Ensure(ValidationExpression.IsMatch(normalized), ValidationMessage); 
        return normalized; 
    }

    /// <summary>
    /// Parses the supplied value.
    /// </summary>
    public static TSelf Parse(string s, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new FormatException($"{typeof(TSelf).Name} cannot be null or empty.");

        try
        {
            return TSelf.Create(s);
        }
        catch (ArgumentException exception)
        {
            throw new FormatException($"Invalid {typeof(TSelf).Name}.", exception);
        }
    }

    /// <summary>
    /// Attempts to parse the supplied value.
    /// </summary>
    public static bool TryParse(string? s, IFormatProvider? provider, out TSelf result)
    {
        result = default!;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        try
        {
            result = TSelf.Create(s);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public sealed override string ToString()
        => Value;

    public static implicit operator string(StringValueObject<TSelf> value)
        => value.Value;
}
