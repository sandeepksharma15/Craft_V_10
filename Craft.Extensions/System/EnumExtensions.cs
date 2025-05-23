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
        /// Retrieves a list of all values in the specified enumeration type, ordered by their underlying integer values.
        /// </summary>
        public static List<T> GetOrderedEnumValues() =>
            [.. Enum.GetValues<T>().Cast<T>().OrderBy(x => Convert.ToInt32(x))];

        /// <summary>
        /// Retrieves the extreme value of an enumeration, either the highest or the lowest, based on the specified condition.
        /// </summary>
        public static T GetExtremeEnumValue(bool highest = true) =>
            Enum.GetValues<T>()
                .Cast<T>()
                .OrderBy(x => Convert.ToInt32(x) * (highest ? -1 : 1))
                .First();

        /// <summary>
        /// Retrieves the highest value defined in the specified enumeration type.
        /// </summary>
        public static T GetHighestEnumValue() => GetExtremeEnumValue<T>(highest: true);

        /// <summary>
        /// Retrieves the lowest value of the specified enumeration type.
        /// </summary>
        public static T GetLowestEnumValue() => GetExtremeEnumValue<T>(highest: false);

        /// <summary>
        /// Gets a dictionary of all enum values and their descriptions.
        /// </summary>
        public static Dictionary<T, string> GetDescriptions() =>
            Enum.GetValues<T>().ToDictionary(value => value, value => value.GetDescription());

        /// <summary>
        /// Gets a dictionary of all enum values and their names.
        /// </summary>
        public static Dictionary<T, string> GetNames() =>
            Enum.GetValues<T>().ToDictionary(value => value, value => value.ToString());

        /// <summary>
        /// Gets an array of all enum values.
        /// </summary>
        public static T[] GetValues() => Enum.GetValues<T>();
    }

    extension<T>(T enumValue) where T : struct, Enum
    {
        /// <summary>
        /// Retrieves the next value in the enumeration sequence for the specified enum value.
        /// </summary>
        public T GetNextEnumValue()
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
        public T GetPrevEnumValue()
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
        public string GetDescription()
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()!);
            if (memberInfo is { Length: > 0 })
            {
                var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                return attributes is { Length: > 0 }
                    ? ((DescriptionAttribute)attributes[0]).Description
                    : enumValue.ToString()!;
            }
            return string.Empty;
        }

        /// <summary>
        /// Retrieves all individual flags that are set in the specified enumeration value.
        /// </summary>
        public IEnumerable<T> GetFlags() =>
            GetValues<T>().Where(item => enumValue.HasFlag(item));

        /// <summary>
        /// Determines whether the specified flag or combination of flags is set in the current enumeration value.
        /// </summary>
        public bool IsSet(Enum matchTo) =>
            (Convert.ToUInt32(enumValue) & Convert.ToUInt32(matchTo)) != 0;

        /// <summary>
        /// Gets the name of a specific enum value.
        /// </summary>
        public string GetName()
        {
            var names = GetNames<T>();
            return names.TryGetValue(enumValue, out var result)
                ? result
                : string.Join(',', enumValue.GetFlags().Select(x => names[x]));
        }

        /// <summary>
        /// Tries to get the description of a specific enum value.
        /// </summary>
        public bool TryGetSingleDescription(out string? result) =>
            GetDescriptions<T>().TryGetValue(enumValue, out result);

        /// <summary>
        /// Tries to get the name of a specific enum value.
        /// </summary>
        public bool TryGetSingleName(out string? result) =>
            GetNames<T>().TryGetValue(enumValue, out result);

        /// <summary>
        /// Converts the specified enumeration value to its string representation using the invariant culture.
        /// </summary>
        public string ToStringInvariant() => enumValue.GetName();
    }

    extension<T>(T) where T : struct
    {
        /// <summary>
        /// Validates that the specified generic type parameter <typeparamref name="T"/> is an enumeration type.
        /// </summary>
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
        public T ToEnum<T>() where T : struct
        {
            ValidateEnumType<T>();
            return (T)Enum.ToObject(typeof(T), value);
        }
    }

    extension(string value)
    {
        /// <summary>
        /// Converts the specified string representation of the name or numeric value of an enumeration to the equivalent enumeration value.
        /// </summary>
        public T ToEnum<T>(bool ignoreCase = true) where T : struct
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            ValidateEnumType<T>();
            return Enum.Parse<T>(value, ignoreCase);
        }

        /// <summary>
        /// Determines whether the specified flag or any of the flags in the specified enumeration value are contained within the current value.
        /// </summary>
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

