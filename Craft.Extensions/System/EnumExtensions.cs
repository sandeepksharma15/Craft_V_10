using System.ComponentModel;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class EnumExtensions
{
    extension<T>(T) where T : struct, Enum
    {
        /// <summary>
        /// Retrieves a list of all values of the specified enum type, ordered by their underlying integer values.
        /// </summary>
        /// <remarks>This method uses the underlying integer values of the enum to determine the order of the
        /// returned list.</remarks>
        /// <typeparam name="T">The enum type whose values are to be retrieved. Must be a non-nullable enum.</typeparam>
        /// <returns>A list of enum values of type <typeparamref name="T"/>, sorted in ascending order by their underlying integer
        /// values.</returns>
        public static List<T> GetOrderedEnumValues()
        {
            return [.. Enum.GetValues<T>()
            .Cast<T>()
            .OrderBy(x => Convert.ToInt32(x))];
        }

        /// <summary>
        /// Retrieves the extreme value of an enumeration, either the highest or the lowest, based on the specified
        /// condition.
        /// </summary>
        /// <remarks>This method uses the underlying integer values of the enumeration to determine the extreme
        /// value.</remarks>
        /// <typeparam name="T">The enumeration type from which to retrieve the value. Must be a struct and an enumeration.</typeparam>
        /// <param name="highest">A boolean value indicating whether to retrieve the highest value.  <see langword="true"/> to retrieve the
        /// highest value; <see langword="false"/> to retrieve the lowest value.</param>
        /// <returns>The extreme value of the specified enumeration type <typeparamref name="T"/>.  Returns the highest value if
        /// <paramref name="highest"/> is <see langword="true"/>; otherwise, the lowest value.</returns>
        public static T GetExtremeEnumValue(bool highest = true)
        {
            return Enum.GetValues<T>()
                .Cast<T>()
                .OrderBy(x => Convert.ToInt32(x) * (highest ? -1 : 1))
                .First();
        }

        /// <summary>
        /// Retrieves the highest value defined in the specified enumeration type.
        /// </summary>
        /// <typeparam name="T">The enumeration type from which to retrieve the highest value. Must be a valid enum type.</typeparam>
        /// <returns>The highest value defined in the enumeration of type <typeparamref name="T"/>.</returns>
        public static T GetHighestEnumValue()
            => GetExtremeEnumValue<T>(highest: true);

        /// <summary>
        /// Retrieves the lowest defined value of the specified enumeration type.
        /// </summary>
        /// <remarks>This method is generic and constrained to enumeration types. It is useful for scenarios where
        /// the smallest value of an enumeration is required, such as initializing variables or performing range
        /// checks.</remarks>
        /// <typeparam name="T">The enumeration type for which the lowest value is to be retrieved. Must be a struct and an <see
        /// cref="System.Enum"/>.</typeparam>
        /// <returns>The lowest value defined in the enumeration of type <typeparamref name="T"/>.</returns>
        public static T GetLowestEnumValue()
            => GetExtremeEnumValue<T>(highest: false);
    }

    extension<T>(T enumValue) where T : struct, Enum
    {
        /// <summary>
        /// Retrieves the next value in the enumeration sequence for the specified enum value.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration. Must be a struct and an enumeration type.</typeparam>
        /// <param name="enumValue">The current enumeration value.</param>
        /// <returns>The next value in the enumeration sequence. If the current value is the last in the sequence,  the method wraps
        /// around and returns the first value.</returns>
        public T GetNextEnumValue()
        {
            var values = GetOrderedEnumValues<T>();
            var currentIndex = values.IndexOf(enumValue);

            return currentIndex >= 0 && currentIndex < values.Count - 1
                ? values[currentIndex + 1]
                : values[0]; // Return first item if there is no next value
        }

        /// <summary>
        /// Retrieves the previous value in the enumeration relative to the specified value.
        /// </summary>
        /// <typeparam name="T">The enumeration type.</typeparam>
        /// <param name="enumValue">The current enumeration value.</param>
        /// <returns>The previous enumeration value in the defined order. If the specified value is the first in the order, the
        /// method returns the last value in the enumeration.</returns>
        public T GetPrevEnumValue()
        {
            var values = GetOrderedEnumValues<T>();
            var currentIndex = values.IndexOf(enumValue);

            return currentIndex > 0 && currentIndex < values.Count
                ? values[currentIndex - 1]
                : values[^1]; // Return last item if there is no prev value
        }
    }

    /// <summary>
    /// Validates that the specified generic type parameter <typeparamref name="T"/> is an enumeration type.
    /// </summary>
    /// <remarks>This method ensures that the generic type parameter <typeparamref name="T"/> is constrained
    /// to enumeration types. If <typeparamref name="T"/> is not an enumeration, an exception is thrown.</remarks>
    /// <typeparam name="T">The type to validate. Must be an enumeration type.</typeparam>
    /// <exception cref="Exception">Thrown if <typeparamref name="T"/> is not an enumeration type.</exception>
    public static void ValidateEnumType<T>()
    {
        if (!typeof(T).IsEnum)
            throw new Exception("T must be an Enumeration type.");
    }

    /// <summary>
    /// Converts the specified integer value to the corresponding enumeration value of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>This method performs a direct conversion of the integer value to the specified enumeration
    /// type.  Ensure that the integer value corresponds to a valid enumeration value in <typeparamref name="T"/>  to
    /// avoid unexpected results.</remarks>
    /// <typeparam name="T">The enumeration type to which the integer value will be converted. Must be a value type that is an enumeration.</typeparam>
    /// <param name="value">The integer value to convert to the enumeration type <typeparamref name="T"/>.</param>
    /// <returns>The enumeration value of type <typeparamref name="T"/> that corresponds to the specified integer value.</returns>
    public static T ToEnum<T>(this int value) where T : struct
    {
        ValidateEnumType<T>();

        return (T)Enum.ToObject(typeof(T), value);
    }

    /// <summary>
    /// Converts the specified string representation of the name or numeric value of an enumeration to the equivalent
    /// enumeration value.
    /// </summary>
    /// <typeparam name="T">The type of the enumeration to convert to. Must be a value type that is an enumeration.</typeparam>
    /// <param name="value">The string representation of the enumeration name or numeric value to convert. Cannot be null or empty.</param>
    /// <param name="ignoreCase">A boolean value indicating whether the case of the string should be ignored during the conversion. Defaults to
    /// <see langword="true"/>.</param>
    /// <returns>The enumeration value of type <typeparamref name="T"/> that corresponds to the specified string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null or empty.</exception>
    public static T ToEnum<T>(this string value, bool ignoreCase = true) where T : struct
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        ValidateEnumType<T>();

        return Enum.Parse<T>(value, ignoreCase);
    }

    /// <summary>
    /// Determines whether the specified string contains the name of the given enumeration value or any of its flags.
    /// </summary>
    /// <remarks>The comparison is performed using <see cref="StringComparison.InvariantCultureIgnoreCase"/>.
    /// If the enumeration value represents a combination of flags, the method checks for the presence of any individual
    /// flag.</remarks>
    /// <typeparam name="T">The enumeration type of the flags. Must be an <see cref="Enum"/>.</typeparam>
    /// <param name="agent">The string to search within. Cannot be <see langword="null"/>.</param>
    /// <param name="flags">The enumeration value or combination of flags to search for.</param>
    /// <returns><see langword="true"/> if the string contains the name of the specified enumeration value or any of its flags;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool Contains<T>(this string agent, T flags) where T : Enum
    {
        if (EnumValues<T>.TryGetSingleName(flags, out var value) && value != null)
            return agent.Contains(value, StringComparison.InvariantCultureIgnoreCase);

        return flags
            .GetFlags()
            .Any(item => agent.Contains(item.ToStringInvariant(), StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Retrieves the description attribute of the specified enumeration value.
    /// </summary>
    /// <remarks>This method uses the <see cref="System.ComponentModel.DescriptionAttribute"/> to retrieve the
    /// description of the enumeration value. If the attribute is not present, the method returns the name of the
    /// enumeration value.</remarks>
    /// <typeparam name="T">The type of the enumeration. Must be a valid <see cref="System.Enum"/> type.</typeparam>
    /// <param name="someEnum">The enumeration value for which to retrieve the description.</param>
    /// <returns>A <see cref="string"/> containing the description of the enumeration value,  or the name of the enumeration
    /// value if no description attribute is defined.</returns>
    public static string GetDescription<T>(this T someEnum)
    {
        if (someEnum is Enum)
        {
            MemberInfo[] memberInfo = someEnum.GetType().GetMember(someEnum.ToString()!);

            if (memberInfo?.Length > 0)
            {
                object[] attributess = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributess?.Length > 0)
                    return ((DescriptionAttribute)attributess[0]).Description;
                else
                    return someEnum.ToString()!;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Retrieves all individual flags that are set in the specified enumeration value.
    /// </summary>
    /// <remarks>This method uses the <see cref="Enum.HasFlag(Enum)"/> method to determine which flags are
    /// set. It is particularly useful for working with bitwise enumeration types.</remarks>
    /// <typeparam name="T">The enumeration type. Must be an <see cref="Enum"/>.</typeparam>
    /// <param name="value">The enumeration value to analyze for set flags.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all flags of type <typeparamref name="T"/> that are set in the
    /// specified <paramref name="value"/>. If no flags are set, the collection will be empty.</returns>
    public static IEnumerable<T> GetFlags<T>(this T value) where T : Enum
        => EnumValues<T>
            .GetValues()
            .Where(item => value.HasFlag(item));

    /// <summary>
    /// Determines whether the specified flag or combination of flags is set in the current enumeration value.
    /// </summary>
    /// <param name="input">The enumeration value to check.</param>
    /// <param name="matchTo">The flag or combination of flags to match against <paramref name="input"/>.</param>
    /// <returns><see langword="true"/> if all flags specified in <paramref name="matchTo"/> are set in <paramref name="input"/>;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsSet(this Enum input, Enum matchTo)
        => (Convert.ToUInt32(input) & Convert.ToUInt32(matchTo)) != 0;

    /// <summary>
    /// Converts the specified enumeration value to its string representation using the invariant culture.
    /// </summary>
    /// <typeparam name="T">The type of the enumeration.</typeparam>
    /// <param name="value">The enumeration value to convert.</param>
    /// <returns>A string that represents the name of the enumeration value, or <see langword="null"/> if the value does not have
    /// a corresponding name.</returns>
    public static string ToStringInvariant<T>(this T value) where T : Enum
        => EnumValues<T>.GetName(value);
}

/// <summary>
/// A utility class for working with enumeration types in C#. It provides methods to retrieve
/// names, descriptions, values, and other information associated with enum values.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
public static class EnumValues<T> where T : Enum
{
    // Static dictionaries to store enum values, names, and descriptions.
    private static readonly Lazy<Dictionary<T, string>> Description = new(() =>
        GetValues().ToDictionary(value => value, value => value.GetDescription()));

    private static readonly Lazy<Dictionary<T, string>> Names = new(() =>
        GetValues().ToDictionary(value => value, value => value.ToString()));

    private static readonly Lazy<T[]> Values = new(() =>
        (T[])Enum.GetValues(typeof(T)));

    /// <summary>
    /// Gets the description of a specific enum value.
    /// If the description is not found, returns a comma-separated string of flag descriptions.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>The description of the enum value.</returns>
    public static string GetDescription(T value)
        => Description.Value.TryGetValue(value, out var result)
           ? result
           : string.Join(',', value.GetFlags().Select(x => Description.Value[x]));

    /// <summary>
    /// Gets a dictionary of all enum values and their descriptions.
    /// </summary>
    /// <returns>A dictionary of enum values and descriptions.</returns>
    public static Dictionary<T, string> GetDescriptions()
        => Description.Value;

    /// <summary>
    /// Gets the name of a specific enum value.
    /// If the name is not found, returns a comma-separated string of flag names.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>The name of the enum value.</returns>
    public static string GetName(T value)
        => Names.Value.TryGetValue(value, out var result)
               ? result
               : string.Join(',', value.GetFlags().Select(x => Names.Value[x]));

    /// <summary>
    /// Gets a dictionary of all enum values and their names.
    /// </summary>
    /// <returns>A dictionary of enum values and names.</returns>
    public static Dictionary<T, string> GetNames()
        => Names.Value;

    /// <summary>
    /// Gets an array of all enum values.
    /// </summary>
    /// <returns>An array of enum values.</returns>
    public static T[] GetValues()
        => Values.Value;

    /// <summary>
    /// Tries to get the description of a specific enum value.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <param name="result">The description of the enum value if found.</param>
    /// <returns>True if the description is found; otherwise, false.</returns>
    public static bool TryGetSingleDescription(T value, out string? result)
        => Description.Value.TryGetValue(value, out result);

    /// <summary>
    /// Tries to get the name of a specific enum value.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <param name="result">The name of the enum value if found.</param>
    /// <returns>True if the name is found; otherwise, false.</returns>
    public static bool TryGetSingleName(T value, out string? result)
        => Names.Value.TryGetValue(value, out result);
}
