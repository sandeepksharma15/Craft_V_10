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
        /// Retrieves a list of all values in the specified enumeration type, ordered by their underlying integer
        /// values.
        /// </summary>
        /// <remarks>This method uses the underlying integer values of the enumeration to determine the
        /// order. Ensure that <typeparamref name="T"/> is a valid enum type; otherwise, an exception will be
        /// thrown.</remarks>
        /// <returns>A list of enumeration values of type <typeparamref name="T"/>, sorted in ascending order by their underlying
        /// integer values.</returns>
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
        /// <remarks>This method uses the underlying integer values of the enumeration to determine the
        /// extreme value.</remarks>
        /// <param name="highest">A boolean value indicating whether to retrieve the highest value.  <see langword="true"/> to retrieve the
        /// highest value; <see langword="false"/> to retrieve the lowest value.</param>
        /// <returns>The extreme value of the enumeration <typeparamref name="T"/>.  Returns the highest value if <paramref
        /// name="highest"/> is <see langword="true"/>; otherwise, the lowest value.</returns>
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
        /// Retrieves the lowest value of the specified enumeration type.
        /// </summary>
        /// <remarks>This method determines the lowest value of the enumeration by comparing the
        /// underlying numeric values of the enum members.</remarks>
        /// <returns>The lowest value of the enumeration type <typeparamref name="T"/>.</returns>
        public static T GetLowestEnumValue()
            => GetExtremeEnumValue<T>(highest: false);

        /// <summary>
        /// Gets a dictionary of all enum values and their descriptions.
        /// </summary>
        /// <returns>A dictionary of enum values and descriptions.</returns>
        public static Dictionary<T, string> GetDescriptions()
        {
            var values = Enum.GetValues<T>();
            var descriptions = values.ToDictionary(value => value, value => value.GetDescription());

            return descriptions;
        }

        /// <summary>
        /// Gets a dictionary of all enum values and their names.
        /// </summary>
        /// <returns>A dictionary of enum values and names.</returns>
        public static Dictionary<T, string> GetNames()
        {
            var values = Enum.GetValues<T>();

            return values.ToDictionary(value => value, value => value.ToString());
        }

        /// <summary>
        /// Gets an array of all enum values.
        /// </summary>
        /// <returns>An array of enum values.</returns>
        public static T[] GetValues() => Enum.GetValues<T>();
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
        public string GetDescription()
        {
            MemberInfo[] memberInfo = enumValue.GetType().GetMember(enumValue.ToString()!);

            if (memberInfo?.Length > 0)
            {
                object[] attributess = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                return attributess?.Length > 0
                    ? ((DescriptionAttribute)attributess[0]).Description
                    : enumValue.ToString()!;
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
        public IEnumerable<T> GetFlags() 
            => GetValues<T>().Where(item => enumValue.HasFlag(item));

        /// <summary>
        /// Determines whether the specified flag or combination of flags is set in the current enumeration value.
        /// </summary>
        /// <param name="input">The enumeration value to check.</param>
        /// <param name="matchTo">The flag or combination of flags to match against <paramref name="input"/>.</param>
        /// <returns><see langword="true"/> if all flags specified in <paramref name="matchTo"/> are set in <paramref name="input"/>;
        /// otherwise, <see langword="false"/>.</returns>
        public bool IsSet(Enum matchTo)
            => (Convert.ToUInt32(enumValue) & Convert.ToUInt32(matchTo)) != 0;

        /// <summary>
        /// Gets the name of a specific enum value.
        /// If the name is not found, returns a comma-separated string of flag names.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The name of the enum value.</returns>
        public string GetName()
        {
            var values = Enum.GetValues<T>();
            var names = values.ToDictionary(value => value, value => value.ToString());

            return names.TryGetValue(enumValue, out var result)
                       ? result
                       : string.Join(',', enumValue.GetFlags().Select(x => names[x]));
        }

        /// <summary>
        /// Tries to get the description of a specific enum value.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <param name="result">The description of the enum value if found.</param>
        /// <returns>True if the description is found; otherwise, false.</returns>
        public bool TryGetSingleDescription(out string? result)
            => GetDescriptions<T>().TryGetValue(enumValue, out result);

        /// <summary>
        /// Tries to get the name of a specific enum value.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <param name="result">The name of the enum value if found.</param>
        /// <returns>True if the name is found; otherwise, false.</returns>
        public bool TryGetSingleName(out string? result)
            => GetNames<T>().TryGetValue(enumValue, out result);

        /// <summary>
        /// Converts the specified enumeration value to its string representation using the invariant culture.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="value">The enumeration value to convert.</param>
        /// <returns>A string that represents the name of the enumeration value, or <see langword="null"/> if the value does not have
        /// a corresponding name.</returns>
        public string ToStringInvariant()
            => enumValue.GetName();
    }

    extension<T>(T) where T : struct
    {
        /// <summary>
        /// Validates that the specified generic type parameter <typeparamref name="T"/> is an enumeration type.
        /// </summary>
        /// <remarks>This method ensures that the generic type parameter <typeparamref name="T"/> is constrained
        /// to enumeration types. If <typeparamref name="T"/> is not an enumeration, an exception is thrown.</remarks>
        /// <typeparam name="T">The type to validate. Must be an enumeration type.</typeparam>
        /// <exception cref="Exception">Thrown if <typeparamref name="T"/> is not an enumeration type.</exception>
        public static void ValidateEnumType()
        {
            if (!typeof(T).IsEnum)
                throw new Exception("T must be an Enumeration type.");
        }
    }

    extension(int value)
    {
        /// <summary>
        /// Converts the specified integer value to the corresponding enumeration value of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>This method performs a direct conversion of the integer value to the specified enumeration
        /// type.  Ensure that the integer value corresponds to a valid enumeration value in <typeparamref name="T"/>  to
        /// avoid unexpected results.</remarks>
        /// <typeparam name="T">The enumeration type to which the integer value will be converted. Must be a value type that is an enumeration.</typeparam>
        /// <param name="value">The integer value to convert to the enumeration type <typeparamref name="T"/>.</param>
        /// <returns>The enumeration value of type <typeparamref name="T"/> that corresponds to the specified integer value.</returns>
        public T ToEnum<T>() where T : struct
        {
            ValidateEnumType<T>();

            return (T)Enum.ToObject(typeof(T), value);
        }
    }

    extension(string value)
    {
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
        public T ToEnum<T>(bool ignoreCase = true) where T : struct
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            ValidateEnumType<T>();

            return Enum.Parse<T>(value, ignoreCase);
        }

        /// <summary>
        /// Determines whether the specified flag or any of the flags in the specified enumeration value  are contained
        /// within the current value.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration. Must be a struct and an enumeration type.</typeparam>
        /// <param name="flags">The enumeration value to check. This can be a single flag or a combination of flags.</param>
        /// <returns><see langword="true"/> if the specified flag or any of the flags in the specified enumeration  value are
        /// contained within the current value; otherwise, <see langword="false"/>.</returns>
        public bool Contains<T>(T flags) where T : struct, Enum
        {
            return flags.TryGetSingleName(out var flagValue) && flagValue != null
                ? value.Contains(flagValue, StringComparison.InvariantCultureIgnoreCase)
                : flags
                    .GetFlags<T>()
                    .Any(item => value.Contains(item.ToStringInvariant<T>(), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

