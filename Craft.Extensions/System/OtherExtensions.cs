using System.Globalization;
using System.Text;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class OtherExtensions
{
    extension(byte[]? bytes)
    {
        /// <summary>
        /// Converts a byte array to its hexadecimal representation.
        /// </summary>
        /// <returns>The hexadecimal string representation of the byte array, or null if input is null.</returns>
        public string? BytesToHex()
        {
            if (bytes is null) return null;

            if (bytes.Length == 0) return string.Empty;

            var hex = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
                hex.Append(b.ToString("X2", CultureInfo.InvariantCulture));

            return hex.ToString();
        }
    }

    extension(string? hex)
    {
        /// <summary>
        /// Converts a hexadecimal string to a byte array.
        /// </summary>
        /// <returns>
        /// A byte array representing the converted values.
        /// Returns an empty array if the input string is null or empty.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown if parsing fails for any hex code within the string, or string is not in proper format.
        /// </exception>
        public byte[] HexToBytes()
        {
            if (string.IsNullOrWhiteSpace(hex)) return Array.Empty<byte>();

            var trimmed = hex.Trim();

            if (trimmed.Length == 0) return Array.Empty<byte>();

            if (trimmed.Length % 2 != 0)
                throw new FormatException("Hex string must have an even number of characters.");

            var bytes = new byte[trimmed.Length / 2];

            ReadOnlySpan<char> span = trimmed.AsSpan();

            for (int i = 0; i < bytes.Length; i++)
            {
                if (!byte.TryParse(span.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                    throw new FormatException($"Failed to parse hex string at position {i * 2}: '{span.Slice(i * 2, 2).ToString()}'");

                bytes[i] = b;
            }
            return bytes;
        }
    }

    extension(decimal value)
    {
        /// <summary>
        /// Converts a decimal value to its percentage representation with up to two decimal places.
        /// </summary>
        public string ToPercentage() 
            => (value * 100).ToString("0.##", CultureInfo.CurrentCulture) + "%";
    }

    extension(double value)
    {
        /// <summary>
        /// Converts a double value to its percentage representation with up to two decimal places.
        /// </summary>
        public string ToPercentage() 
            => (value * 100).ToString("0.##", CultureInfo.CurrentCulture) + "%";
    }

    extension(float value)
    {
        /// <summary>
        /// Converts a float value to its percentage representation with up to two decimal places.
        /// </summary>
        public string ToPercentage() 
            => (value * 100).ToString("0.##", CultureInfo.CurrentCulture) + "%";
    }
}
