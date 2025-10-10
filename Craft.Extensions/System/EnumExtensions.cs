using System.ComponentModel;
using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class EnumExtensions
{
    /// <summary>
    /// Retrieves a list of all values in the specified enumeration type, ordered by their underlying integer values.
    /// </summary>
    public static List<T> GetOrderedEnumValues<T>() where T : struct, Enum
        => [.. Enum
            .GetValues<T>()
            .OrderBy(x => Convert.ToInt32(x, CultureInfo.InvariantCulture))];

    /// <summary>
    /// Retrieves the extreme value of an enumeration, either the highest or the lowest, based on the specified condition.
    /// </summary>
    public static T GetExtremeEnumValue<T>(bool highest = true) where T : struct, Enum
        => Enum.GetValues<T>()
            .OrderBy(x => Convert.ToInt32(x, CultureInfo.InvariantCulture) * (highest ? -1 : 1))
            .First();

    /// <summary>
    /// Retrieves the highest value defined in the specified enumeration type.
    /// </summary>
    public static T GetHighestEnumValue<T>() where T : struct, Enum
        => GetExtremeEnumValue<T>(highest: true);

    /// <summary>
    /// Retrieves the lowest value of the specified enumeration type.
    /// </summary>
    public static T GetLowestEnumValue<T>() where T : struct, Enum
        => GetExtremeEnumValue<T>(highest: false);

    /// <summary>
    /// Gets a dictionary of all enum values and their descriptions.
    /// </summary>
    public static Dictionary<T, string> GetDescriptions<T>() where T : struct, Enum
        => Enum.GetValues<T>().ToDictionary(value => value, value => value.GetDescription());

    /// <summary>
    /// Gets a dictionary of all enum values and their names.
    /// </summary>
    public static Dictionary<T, string> GetNames<T>() where T : struct, Enum
        => Enum.GetValues<T>().ToDictionary(value => value, value => value.ToString()!);

    /// <summary>
    /// Gets an array of all enum values.
    /// </summary>
    public static T[] GetValues<T>() where T : struct, Enum
        => Enum.GetValues<T>();

    /// <summary>
    /// Retrieves the next value in the enumeration sequence for the specified enum value.
    /// </summary>
    public static T GetNextEnumValue<T>(this T enumValue) where T : struct, Enum
    {
        var values = GetOrderedEnumValues<T>();

        var currentIndex = values.IndexOf(enumValue);

        return currentIndex >= 0 && currentIndex < values.Count - 1
            ? values[currentIndex + 1]
            : values[0];
    }

    /// <summary>
    /// Retrieves the previous value in the enumeration relative to the specified value.
    /// </summary>
    public static T GetPrevEnumValue<T>(this T enumValue) where T : struct, Enum
    {
        var values = GetOrderedEnumValues<T>();

        var currentIndex = values.IndexOf(enumValue);

        return currentIndex > 0 && currentIndex < values.Count
            ? values[currentIndex - 1]
            : values[^1];
    }

    /// <summary>
    /// Retrieves the description attribute of the specified enumeration value.
    /// </summary>
    public static string GetDescription<T>(this T enumValue) where T : struct, Enum
    {
        var memberInfo = typeof(T).GetMember(enumValue.ToString()!);

        if (memberInfo is { Length: > 0 })
        {
            var attributes = memberInfo[0]
                .GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes is { Length: > 0 }
                ? ((DescriptionAttribute)attributes[0]).Description
                : enumValue.ToString()!;
        }

        return string.Empty;
    }

    /// <summary>
    /// Retrieves all individual flags that are set in the specified enumeration value.
    /// </summary>
    public static IEnumerable<T> GetFlags<T>(this T enumValue) where T : struct, Enum
        => GetValues<T>().Where(item => enumValue.HasFlag(item));

    /// <summary>
    /// Determines whether the specified flag or combination of flags is set in the current enumeration value.
    /// </summary>
    public static bool IsSet<T>(this T enumValue, Enum matchTo) where T : struct, Enum
        => (Convert.ToUInt32(enumValue, CultureInfo.InvariantCulture) & Convert.ToUInt32(matchTo, CultureInfo.InvariantCulture)) != 0;

    /// <summary>
    /// Gets the name of a specific enum value.
    /// </summary>
    public static string GetName<T>(this T enumValue) where T : struct, Enum
    {
        var names = GetNames<T>();

        return names.TryGetValue(enumValue, out var result)
            ? result
            : string.Join(',', enumValue.GetFlags().Select(x => names[x]));
    }

    /// <summary>
    /// Tries to get the description of a specific enum value.
    /// </summary>
    public static bool TryGetSingleDescription<T>(this T enumValue, out string? result) where T : struct, Enum
        => GetDescriptions<T>().TryGetValue(enumValue, out result);

    /// <summary>
    /// Tries to get the name of a specific enum value.
    /// </summary>
    public static bool TryGetSingleName<T>(this T enumValue, out string? result) where T : struct, Enum
        => GetNames<T>().TryGetValue(enumValue, out result);

    /// <summary>
    /// Converts the specified enumeration value to its string representation using the invariant culture.
    /// </summary>
    public static string ToStringInvariant<T>(this T enumValue) where T : struct, Enum
        => enumValue.GetName();

    /// <summary>
    /// Validates that the specified generic type parameter <typeparamref name="T"/> is an enumeration type.
    /// </summary>
    public static void ValidateEnumType<T>() where T : struct
    {
        if (!typeof(T).IsEnum)
            throw new Exception("T must be an Enumeration type.");
    }

    /// <summary>
    /// Converts the specified integer value to the corresponding enumeration value of type <typeparamref name="T"/>.
    /// </summary>
    public static T ToEnum<T>(this int value) where T : struct
    {
        ValidateEnumType<T>();

        return (T)Enum.ToObject(typeof(T), value);
    }

    /// <summary>
    /// Converts the specified string representation of the name or numeric value of an enumeration to the equivalent enumeration value.
    /// </summary>
    public static T ToEnum<T>(this string value, bool ignoreCase = true) where T : struct
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        ValidateEnumType<T>();

        return Enum.Parse<T>(value, ignoreCase);
    }

    /// <summary>
    /// Determines whether the specified flag or any of the flags in the specified enumeration value are contained within the current value.
    /// </summary>
    public static bool Contains<T>(this string value, T flags) where T : struct, Enum
    {
        return flags.TryGetSingleName(out var flagValue) && flagValue != null
            ? value.Contains(flagValue, StringComparison.InvariantCultureIgnoreCase)
            : flags
            .GetFlags()
            .Any(item => value.Contains(item.ToStringInvariant(), StringComparison.InvariantCultureIgnoreCase));
    }
}
